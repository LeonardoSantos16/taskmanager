using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record ProjectMemberDtoRequest
{
    public required Guid UserId { get; set; }
    public required EnumRole Role { get; set; }
}
}