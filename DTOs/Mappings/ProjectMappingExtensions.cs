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

        public static void ApplyToPut(this ProjectDtoUpdateRequest request, Project project)
        {
            project.Name = request.Name;
            project.Description = request.Description;
            project.Status = request.Status;
            project.UpdatedAt = DateTime.UtcNow;
        }

        public static void ApplyToPatch(this ProjectDtoPatchRequest request, Project project)
        {
            if (!string.IsNullOrWhiteSpace(request.Name))
                project.Name = request.Name;

            if (request.Description is not null)
                project.Description = request.Description;

            if (request.Status.HasValue)
                project.Status = request.Status.Value;

            project.UpdatedAt = DateTime.UtcNow;
        }
   }
}