using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public record UpdatePasswordRequest(string Email, string NewPassword);
}