using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs
{
    public class ProjectMemberDtoPatchRequest
    {
        public EnumRole? Role { get; set; }
    }
}