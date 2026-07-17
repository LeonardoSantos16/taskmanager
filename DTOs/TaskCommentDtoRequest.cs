using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public class TaskCommentDtoRequest
    {
        public required string Content {get; set;}  
    }
}