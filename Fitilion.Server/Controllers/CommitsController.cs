using Fitilion.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Fitilion.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommitsController : ControllerBase
    {
        private readonly IGitHubCommitsService _gitHubCommitsService;
        public CommitsController(IGitHubCommitsService gitHubCommitsService)
        {
            _gitHubCommitsService = gitHubCommitsService;
        }
        [HttpGet("Comments")]
        public async Task<IActionResult> GetGitHubCommit([Required] string repoName, [Required] string repoOwner)
        {
           var result = await _gitHubCommitsService.GetGitHubCommitsAsync(repoName, repoOwner);
           return Ok(result);
        }

        [HttpGet("save-commit")]
        public async Task<IActionResult> SaveCommit([Required] string commitId, string repoName, string repoOwner)
        {
            var result = await _gitHubCommitsService.SaveGitHubCommitAsync(commitId, repoName, repoOwner);
            return (result);
        }

        [HttpGet("remove-commit")]
        public async Task<IActionResult> RemoveCommit([Required] string commitId, string repoName, string repoOwner)
        {
            var result = await _gitHubCommitsService.RemoveGitHubCommitAsync(commitId, repoName, repoOwner);
            return (result);
        }


        [HttpGet("search")]
        public async Task<IActionResult> SearchCommits([Required] string repoName, [Required] string repoOwner, string message)
        {
            var result = await _gitHubCommitsService.SearchGitHubCommitsAsync(repoName, repoOwner, message);
            return Ok(result);
        }

        [HttpGet("saved-commits")]
        public async Task<IActionResult> GetSavedCommits()
        {
            var result = await _gitHubCommitsService.GetSavedCommitsAsync();
            return Ok(result);
        }

        [HttpGet("search-saved-commits")]
        public async Task<IActionResult> SearchSavedCommits([Required] string searchQuery)
        {
            var result = await _gitHubCommitsService.SearchSavedCommitsAsync(searchQuery);
            return Ok(result);
        }
    }
}

