using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Context;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public class ProjectMemberRepository : Repository<ProjectMember>
    {
        public ProjectMemberRepository(AppDbContext context) : base(context)
        {
        }
    }
}