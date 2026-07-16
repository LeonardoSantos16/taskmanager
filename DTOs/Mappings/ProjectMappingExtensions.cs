using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs.Mappings
{
    public static class ProjectMappingExtensions
    {
        public static ProjectDtoResponse ToDtoResponse(this Project project)
        {
            return new ProjectDtoResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Status = project.Status,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt,
                OwnerId = project.OwnerId
            };
        }

        public static Project ToModel(this ProjectDtoRequest projectDto)
        {
            return new Project
            {
                Name = projectDto.Name,
                Description = projectDto.Description,
                Status = projectDto.Status,
                CreatedAt = DateTime.UtcNow,
                OwnerId = projectDto.OwnerId
            };
        }
    }
}