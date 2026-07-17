using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public record TaskCommentDtoResponse
    {
        public Guid Id {get; set;}
        public required string Content {get; set;}
    }
}