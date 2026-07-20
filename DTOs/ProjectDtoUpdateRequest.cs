using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record ProjectDtoUpdateRequest
    {
        public required string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required EnumProjectStatus Status { get; init;}
       
    }
}