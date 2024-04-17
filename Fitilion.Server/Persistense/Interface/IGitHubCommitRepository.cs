using Fitilion.Server.Models;

namespace Fitilion.Server.Persistense.Interface
{
    public interface IGitHubCommitRepository
    {
        List<GitHubCommit> GetSavedRepoCommits(string repoName, string repoOwner);

        List<GitHubCommit> GetSavedCommits();
        GitHubCommit GetById(object id);
        void Insert(GitHubCommit entity);
        Task DeleteAsync(string id);
    }


}
