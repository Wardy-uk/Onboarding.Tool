using Microsoft.EntityFrameworkCore;
using Onboarding.Tool.Data;
using Onboarding.Tool.Model.Data;
using Onboarding.Tool.Model.Enums;
using Instance = Onboarding.Tool.Model.BriefYourMarket.Instances.Instance;

namespace Onboarding.Tool.Server.Services.Dashboard.Progress;

public class DashboardProgressService : IDashboardProgressService
{
    private readonly AppDbContext _context;

    public DashboardProgressService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsSetupStepStatusAsync(Instance instance, string task, TemplateConfirmationState confirmationState)
    {
        InstanceSetupStep? instanceSetupStep = await _context.InstanceSetupStep.Where(iis => iis.InstanceId == instance.Id && iis.SetupStep == task).FirstOrDefaultAsync();
        if (instanceSetupStep == null)
            return false;

        return instanceSetupStep.Status == confirmationState;
    }

    public async Task SetSetupStatusAsync(Instance instance, string task, TemplateConfirmationState confirmationState)
    {
        InstanceSetupStep? instanceSetupStep = await _context.InstanceSetupStep.Where(iis => iis.InstanceId == instance.Id && iis.SetupStep == task).FirstOrDefaultAsync();
        if (instanceSetupStep == null)
        {
            instanceSetupStep = new()
            {
                SetupStep = task,
                Status = confirmationState,
                InstanceId = instance.Id
            };

            _context.InstanceSetupStep.Add(instanceSetupStep);
        }
        else
        {
            instanceSetupStep.Status = confirmationState;
        }

        await _context.SaveChangesAsync();
    }
}
