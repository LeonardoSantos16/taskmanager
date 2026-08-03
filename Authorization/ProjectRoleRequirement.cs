using Microsoft.AspNetCore.Authorization;
using taskmanager.Models;

namespace taskmanager.Authorization
{
    public class ProjectRoleRequirement : IAuthorizationRequirement
    {
        public IReadOnlySet<EnumRole> AllowedRoles { get; }

        public ProjectRoleRequirement(params EnumRole[] allowedRoles)
        {
            AllowedRoles = allowedRoles.ToHashSet();
        }
    }
}
