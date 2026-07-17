using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.Repositories
{
    public interface IProjectRepository : IRepository<Project>
    {
        Task<bool> OwnerExistsAsync(Guid ownerId);
    }
}