using Fitilion.Server.Models;
using Fitilion.Server.Persistense.Interface;
using Microsoft.EntityFrameworkCore;

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
                await _dbContext.SaveChangesAsync();
            }
        }
        public async Task<List<GitHubCommit>> GetSavedRepoCommitsAsync(string repoName, string repoOwner)
        {
            return await _dbContext.GitHubCommits
                .Where(x => x.RepoName == repoName && x.RepoOwner == repoOwner)
                .ToListAsync();
        }

        public async Task<GitHubCommit> GetByIdAsync(object id)
        {
            return _dbContext.GitHubCommits.Find(id);
        }

        public async Task InsertAsync(GitHubCommit entity)
        {
            _dbContext.GitHubCommits.Add(entity);
             _dbContext.SaveChanges();
        }
        public async Task<List<GitHubCommit>> GetSavedCommitsAsync()
        {
            return await _dbContext.GitHubCommits.ToListAsync() ?? new List<GitHubCommit>();
        }

    }
}
