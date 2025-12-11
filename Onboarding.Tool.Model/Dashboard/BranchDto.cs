using Onboarding.Tool.Model.Data;

namespace Onboarding.Tool.Model.Dashboard
{
    public class BranchDto
    {
        public int Id { get; set; }

        public required bool IsDefault { get; set; }

        public required string Name { get; set; }

        public required string SalesEmail { get; set; }

        public required string SalesPhone { get; set; }

        public required string LettingsEmail { get; set; }

        public required string LettingsPhone { get; set; }

        public Address? Address { get; set; }
    }
}
