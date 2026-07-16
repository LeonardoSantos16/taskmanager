using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public record UserDtoResponse
    {
        public int Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        
    }
}