using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record TaskItemDtoUpdateRequest
    {
        public required string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public required DateTime DueDate { get; init; }
        public EnumPriority? Priority { get; init; }
        public Guid? AssignedToId { get; init; }
        public EnumStatusTask Status { get; init; }
    }
}