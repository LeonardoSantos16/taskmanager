using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public record UserDtoResponse
    {
        public Guid Id { get; init; }
        public required string Name { get; init; }
        public required string Email { get; init; }
    }
}