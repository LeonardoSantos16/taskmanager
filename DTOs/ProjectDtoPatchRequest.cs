using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record ProjectDtoPatchRequest
    {
        public string? Name { get; init; }
        public string? Description { get; init; }
        public EnumProjectStatus? Status { get; init; }
    }
}