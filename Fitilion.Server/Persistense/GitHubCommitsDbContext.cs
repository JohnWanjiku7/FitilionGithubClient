using Fitilion.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace Fitilion.Server.Persistense
{
    public class GitHubCommitsDbContext : DbContext
    {
        public GitHubCommitsDbContext(DbContextOptions<GitHubCommitsDbContext> options) : base(options)
        {
        }

        public DbSet<GitHubCommit> GitHubCommits { get; set; }
    }
}
