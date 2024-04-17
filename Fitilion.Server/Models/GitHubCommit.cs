using System.ComponentModel.DataAnnotations;

namespace Fitilion.Server.Models
{
    public class GitHubCommit
    {
        [Key]
        public string CommitId { get; set; }
        public string CommitMessage { get; set; }
        public string CommitDate { get; set; }
        public string CommitAuthor { get; set; }
        public string RepoName { get; set; }
        public string RepoOwner { get; set; }
        public bool Starred {  get; set; } = false;

        public string? StarredTime { get; set; } = null;

    }
}
