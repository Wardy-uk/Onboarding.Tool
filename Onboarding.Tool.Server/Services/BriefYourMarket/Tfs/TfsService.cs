using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using System.Text.Json;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Tfs;

public class TfsService : ITfsService
{
    private readonly GitHttpClient _gitHttpClient;
    private readonly GitSettings _options;

    public TfsService(IOptions<GitSettings> options)
    {
        _options = options.Value;
        
        VssConnection connection = new(new Uri(string.Format("{0}/{1}", _options.BaseUrl, _options.Project)), new VssBasicCredential(string.Empty, _options.Pat));
        _gitHttpClient = connection.GetClient<GitHttpClient>();
    }

    public async Task<string> GetMasterCommitIdAsync()
    {
        GitRepository repo = await _gitHttpClient.GetRepositoryAsync(_options.Area, _options.Repository);
        GitBranchStats master = await _gitHttpClient.GetBranchAsync(repo.Id, "master");

        return master.Commit.CommitId;
    }

    public async Task<GitPush> CreateGitBranchAsync(GitPush push)
    {
        try
        {
            GitPush pushed = await _gitHttpClient.CreatePushAsync(push, _options.Repository);
            return pushed;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    public async Task<GitPullRequest?> CreatePullRequestAsync(GitPush push)
    {
        try
        {
            GitRepository repo = await _gitHttpClient.GetRepositoryAsync(_options.Area, _options.Repository);

            string sourceBranchRef = push.RefUpdates.FirstOrDefault()!.Name;
            string repositoryId = push.Repository.Id.ToString();
            string targetBranchRef = "refs/heads/main";

            GitPullRequest pullRequest = new()
            {
                Title = $"PR for {sourceBranchRef} into master",
                Description = "Auto-generated Pull Request after branch creation",
                SourceRefName = sourceBranchRef,
                TargetRefName = targetBranchRef
            };
            
            GitPullRequest createdPr = await _gitHttpClient.CreatePullRequestAsync(pullRequest, repositoryId);
            return createdPr;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
