using Fitilion.Server.Models;

namespace Fitilion.Server.Persistense.Interface
{
    public interface IGitHubCommitRepository
    {
        Task<List<GitHubCommit>> GetSavedRepoCommitsAsync(string repoName, string repoOwner);
        Task<List<GitHubCommit>> GetSavedCommitsAsync();
        Task<GitHubCommit> GetByIdAsync(object id);
        Task InsertAsync(GitHubCommit entity);
        Task DeleteAsync(string id);
    }


}
