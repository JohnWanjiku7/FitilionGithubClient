using Fitilion.Server.Models;
using Fitilion.Server.Persistense.Interface;

namespace Fitilion.Server.Persistense.Repository
{
    public class GitHubCommitRepository : IGitHubCommitRepository
    {
        private readonly GitHubCommitsDbContext _dbContext;

        public GitHubCommitRepository(GitHubCommitsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task DeleteAsync(string id)
        {
            var entity = await _dbContext.GitHubCommits.FindAsync(id);
            if (entity != null)
            {
                _dbContext.GitHubCommits.Remove(entity);
                _dbContext.SaveChanges();
            }
        }

        public List<GitHubCommit> GetSavedRepoCommits(string repoName, string repoOwner)
        {
            return _dbContext.GitHubCommits.Where(x => x.RepoName == repoName && x.RepoOwner == repoOwner).ToList();
        }

        public GitHubCommit GetById(object id)
        {
            return _dbContext.GitHubCommits.Find(id);
        }

        public void Insert(GitHubCommit entity)
        {
            _dbContext.GitHubCommits.Add(entity);
             _dbContext.SaveChanges();

            // Example: Get the recently added entity
            var commit = GetById(entity.CommitId);
            if (commit != null)
            {
                var starred = commit.Starred;
                // You can perform any additional operations with the newly added entity here
            }
        }

        public List<GitHubCommit> GetSavedCommits()
        {
            return _dbContext.GitHubCommits.ToList() ?? new List<GitHubCommit>();
        }


    }
}
