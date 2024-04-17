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
            string cacheKey = $"{repoOwner}-{repoName}";

            if (_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
                return cachedCommits;

            try
            {
                var commitsFromGitHub = await FetchGitHubCommitsAsync(repoName, repoOwner);
                var existingCommits = await _gitHubCommitRepository.GetSavedRepoCommitsAsync(repoName, repoOwner);

                var allCommits = existingCommits?.Concat(commitsFromGitHub).ToList();
                var uniqueCommits = allCommits.GroupBy(c => c.CommitId).Select(group => group.First()).ToList();

                UpdateCache(cacheKey, uniqueCommits);

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

        public async Task<IActionResult> SaveGitHubCommitAsync(string commitId, string repoName, string repoOwner)
        {
            string cacheKey = $"{repoOwner}-{repoName}";

            List<GitHubCommit> cachedCommits = await GetGitHubCommitsAsync(repoName, repoOwner);

            var commitToUpdate = cachedCommits.FirstOrDefault(c => c.CommitId == commitId);
            if (commitToUpdate != null)
            {
                // Update the commit
                commitToUpdate.StarredTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                commitToUpdate.Starred = true;
                await _gitHubCommitRepository.InsertAsync(commitToUpdate);
                var savedCommits = await GetSavedCommitsAsync();
                savedCommits.Add(commitToUpdate);
                UpdateCache("saved-Commits", savedCommits);
                UpdateCache(cacheKey, cachedCommits);
                return new OkResult();
            }

            // If commit not found, return BadRequest
            return new BadRequestResult();
        }


        public async Task<IActionResult> RemoveGitHubCommitAsync(string commitId, string repoName, string repoOwner)
        {
            string cacheKey = $"{repoOwner}-{repoName}";

            List<GitHubCommit> cachedCommits = await GetGitHubCommitsAsync(repoName, repoOwner);

            var commitToRemove = cachedCommits.FirstOrDefault(c => c.CommitId == commitId);
            if (commitToRemove != null)
            {
                cachedCommits.Remove(commitToRemove);
                _memoryCache.Set(cacheKey, cachedCommits, timeSpan);

                await RemoveFromSavedCommitsCacheAsync(commitId);
                await _gitHubCommitRepository.DeleteAsync(commitToRemove.CommitId);
                
                return new OkResult();
            }

            return new BadRequestResult();
        }
        public async Task<List<GitHubCommit>> SearchGitHubCommitsAsync(string repoName, string repoOwner, string message)
        {
            List<GitHubCommit> cachedCommits = await GetGitHubCommitsAsync(repoName, repoOwner);

            return cachedCommits.Where(c => c.CommitMessage.ToLower().Contains(message.ToLower())).ToList();
        }

        public async Task<List<GitHubCommit>> GetSavedCommitsAsync()
        {
            string cacheKey = "saved-Commits";
            if (!_memoryCache.TryGetValue(cacheKey, out List<GitHubCommit> cachedCommits))
            {
                cachedCommits = await  _gitHubCommitRepository.GetSavedCommitsAsync();
                UpdateCache(cacheKey, cachedCommits);
            }
            return cachedCommits;
        }
        private async Task RemoveFromSavedCommitsCacheAsync(string commitId)
        {
            string savedCommitsCacheKey = "saved-Commits";
            List<GitHubCommit> savedCommits = await GetSavedCommitsAsync();
            var commitToRemove = savedCommits.FirstOrDefault(c => c.CommitId == commitId);
            if (commitToRemove != null)
            {
                savedCommits.Remove(commitToRemove);
                UpdateCache(savedCommitsCacheKey, savedCommits);
            }
        }
        public async Task<List<GitHubCommit>> SearchSavedCommitsAsync(string message)
        {
            string cacheKey = "saved-Commits";
            List<GitHubCommit> cachedCommits = await GetSavedCommitsAsync();

            return cachedCommits.Where(c => c.CommitMessage.ToLower().Contains(message.ToLower())).ToList();
        }

        private void UpdateCache(string cacheKey, List<GitHubCommit> commits)
        {
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(timeSpan);
            _memoryCache.Set(cacheKey, commits, cacheEntryOptions);
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
