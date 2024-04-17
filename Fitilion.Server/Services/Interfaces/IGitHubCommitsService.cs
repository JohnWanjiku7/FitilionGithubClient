using Fitilion.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace Fitilion.Server.Services.Interfaces
{
    public interface IGitHubCommitsService
    {
        Task<List<GitHubCommit>> GetGitHubCommitsAsync(string repoName, string repoOwner);
        Task<IActionResult> SaveGitHubCommitAsync(string commitId, string repoName, string repoOwner);
        Task<IActionResult> RemoveGitHubCommitAsync(string commitId, string repoName, string repoOwner);
        Task<List<GitHubCommit>> SearchGitHubCommitsAsync(string repoName, string repoOwner, string message);
        Task<List<GitHubCommit>> GetSavedCommitsAsync();
        Task<List<GitHubCommit>> SearchSavedCommitsAsync(string message);
    }
}
