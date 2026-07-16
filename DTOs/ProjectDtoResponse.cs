using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record ProjectDtoResponse
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public string? Description { get; init; }
        public required EnumStatus Status { get; init; }
        public DateTime? CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public required Guid OwnerId { get; init; }
    }
}