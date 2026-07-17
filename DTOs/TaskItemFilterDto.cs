using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public record TaskItemFilterDto
    {
    public EnumStatusTask? Status {get; init;}
    public EnumPriority? Priority {get; init;}
    public Guid? AssigneeId {get; init;}
    public DateTime? DueDateFrom {get; init;}
    public DateTime? DueDateTo {get; init;}

    }
}