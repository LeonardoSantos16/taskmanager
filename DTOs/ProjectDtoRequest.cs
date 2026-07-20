using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record ProjectDtoRequest
    {
        public Guid? Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public EnumProjectStatus Status { get; init; } = EnumProjectStatus.Active;
        public Guid OwnerId { get; init; }
    }
}