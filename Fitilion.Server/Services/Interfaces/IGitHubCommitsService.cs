using Fitilion.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fitilion.Server.Services.Interfaces
{
    public interface IGitHubCommitsService
    {
        Task<List<GitHubCommit>> GetGitHubCommitsAsync(string repoName, string repoOwner);
        Task<IActionResult> SaveGitHubCommit(string commitId, string repoName, string repoOwner);
        Task<IActionResult> RemoveGitHubCommit(string commitId, string repoName, string repoOwner);
        Task<List<GitHubCommit>> SearchGitHubCommit(string repoName, string repoOwner, string message);

        Task<List<GitHubCommit>> GetSavedCommits();
        Task<List<GitHubCommit>> SearchSavedCommits(string message);
    }
}
