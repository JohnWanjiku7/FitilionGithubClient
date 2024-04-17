using Fitilion.Server.Models;
using Fitilion.Server.Persistense.Interface;
using Fitilion.Server.Persistense.Repository;
using Fitilion.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Fitilion.Server.Services
{
    public class GitHubCommitsService : IGitHubCommitsService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IGitHubCommitRepository _gitHubCommitRepository;
        private List<GitHubCommit> cachedCommits;
        private TimeSpan timeSpan = TimeSpan.FromSeconds(300);
        public GitHubCommitsService(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory, IGitHubCommitRepository gitHubCommitRepository)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _httpClientFactory = httpClientFactory;
            _gitHubCommitRepository = gitHubCommitRepository;
        }
        public async Task<List<GitHubCommit>> GetGitHubCommitsAsync(string repoName, string repoOwner)
        {
            // Construct cache key based on repoName and repoOwner
            string cacheKey = $"{repoOwner}-{repoName}-Commits";

            if (_memoryCache.TryGetValue(cacheKey, out cachedCommits))
                return cachedCommits;

            try
            {
                var commitsFromGitHub = await FetchGitHubCommitsAsync(repoName, repoOwner);
                var existingCommits = _gitHubCommitRepository.GetSavedRepoCommits(repoName, repoOwner);

                var allCommits = existingCommits.Concat(commitsFromGitHub).ToList();
                var uniqueCommits = allCommits.GroupBy(c => c.CommitId).Select(group => group.First()).ToList();
                foreach (var commit in uniqueCommits)
                {
                    if (commit.StarredTime != null)
                    {
                        commit.StarredTime = DateTime.Now.ToLongDateString();
                    }
                }


                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(timeSpan);
                _memoryCache.Set(cacheKey, uniqueCommits, cacheEntryOptions);

                return uniqueCommits;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching GitHub commits for {repoOwner}/{repoName}: {ex.Message}");
                return new List<GitHubCommit>(); // Provide default value in case of exception
            }
        }

        private async Task<List<GitHubCommit>> FetchGitHubCommitsAsync(string repoName, string repoOwner)
        {
            var client = _httpClientFactory.CreateClient("githubClient");
            var response = await client.GetStringAsync($"{repoOwner}/{repoName}/commits");
            var parsedResponse = JArray.Parse(response);

            return parsedResponse.Select(x => ParseGitHubCommitModel(x, repoName, repoOwner)).ToList();
        }

        public async Task<IActionResult> SaveGitHubCommit(string commitId, string repoName, string repoOwner)
        {
            string cacheKey = $"{repoOwner}-{repoName}-Commits";

            // Try to get the value from cache
            if (!_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
            {
                // If not found in cache, fetch from source
                cachedCommits = await GetGitHubCommitsAsync(repoName, repoOwner);
            }

            // Find and update the commit if found
            var commitToUpdate = cachedCommits?.FirstOrDefault(c => c.CommitId == commitId);
            if (commitToUpdate != null)
            {
                // Update the commit
                commitToUpdate.StarredTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                commitToUpdate.Starred = true;
                _gitHubCommitRepository.Insert(commitToUpdate);
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(timeSpan);
                _memoryCache.Set(cacheKey, cachedCommits, cacheEntryOptions);
                return new OkResult();
            }

            // If commit not found, return BadRequest
            return new BadRequestResult();
        }


        public async Task<IActionResult> RemoveGitHubCommit(string commitId, string repoName, string repoOwner)
        {
            string cacheKey = $"{repoOwner}-{repoName}-Commits";

            List<GitHubCommit> cachedCommits = _memoryCache.Get<List<GitHubCommit>>(cacheKey);

            if (cachedCommits != null)
            {
                var commitToRemove = cachedCommits.FirstOrDefault(c => c.CommitId == commitId);

                if (commitToRemove != null)
                {
                    cachedCommits.Remove(commitToRemove);
                    _memoryCache.Set(cacheKey, cachedCommits, timeSpan);

                    await _gitHubCommitRepository.DeleteAsync(commitToRemove.CommitId);
                    return new OkResult();
                }
            }
            else
            {
                var commitToRemove = _gitHubCommitRepository.GetById(commitId);

                if (commitToRemove != null)
                {
                    await _gitHubCommitRepository.DeleteAsync(commitToRemove.CommitId);
                    return new OkResult();
                }
            }

            return new BadRequestResult();
        }
        public async Task<List<GitHubCommit>> SearchGitHubCommit(string repoName, string repoOwner, string message)
        {
            string cacheKey = $"{repoOwner}-{repoName}-Commits";

            // Try to get the value from cache, fetch from source if not found
            if (!_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
                cachedCommits = await GetGitHubCommitsAsync(repoName, repoOwner);

            // Filter commits by message and author
            return cachedCommits.Where(c => c.CommitMessage.ToLower().Contains(message.ToLower())).ToList();
        }

        public async Task<List<GitHubCommit>> GetSavedCommits()
        {
            string cacheKey = "saved-Commits";
            if (!_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
            {
                cachedCommits = _gitHubCommitRepository.GetSavedCommits();
                var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(timeSpan);
                _memoryCache.Set(cacheKey, cachedCommits, cacheEntryOptions);
            }
            return cachedCommits;
        }


        public async Task<List<GitHubCommit>> SearchSavedCommits(string message)
        {
            string cacheKey = "saved-Commits";
            if (!_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
                cachedCommits = await GetSavedCommits();

            return cachedCommits.Where(c => c.CommitMessage.ToLower().Contains(message.ToLower())).ToList();
        }

        private static GitHubCommit ParseGitHubCommitModel(JToken x, string repoName, string repoOwner)
        {
            try
            {
                return new GitHubCommit
                {
                    CommitId = x["sha"]?.ToString() ?? "N/A",
                    CommitMessage = x["commit"]?["message"]?.ToString() ?? "N/A",
                    CommitDate = ToShortDateTime(x["commit"]?["author"]?["date"]?.ToString()),
                    CommitAuthor = x["commit"]?["author"]?["name"]?.ToString() ?? "N/A",
                    RepoName = repoName,
                    RepoOwner = repoOwner
                    
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error parsing GitHub commit: " + ex.Message);
                return null; // Return null if parsing fails
            }
        }

        private static string ToShortDateTime(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
            {
                return "Date string is null or empty.";
            }

            if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ssZ",
                                       CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTime dateTime))
            {
                // Format the DateTime object to display only date and time in a short format
                string formattedDateTime = dateTime.ToString("yyyy-MM-dd HH:mm");
                return formattedDateTime;
            }
            else if (DateTime.TryParseExact(dateString, "d/MM/yyyy HH:mm:ss",
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.None, out DateTime dateTime2))
            {
                // Format the DateTime object to display only date and time in a short format
                string formattedDateTime = dateTime2.ToString("yyyy-MM-dd HH:mm");
                return formattedDateTime;
            }
            else
            {
                return "Failed to parse datetime.";
            }
        }


    }
}
