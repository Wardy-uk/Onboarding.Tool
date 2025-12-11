using Microsoft.TeamFoundation.SourceControl.WebApi;

namespace Onboarding.Tool.Server.Services.BriefYourMarket.Tfs;

public interface ITfsService
{
    public Task<string> GetMasterCommitIdAsync();

    public Task<GitPush> CreateGitBranchAsync(GitPush push);

    public Task<GitPullRequest?> CreatePullRequestAsync(GitPush push);
}
