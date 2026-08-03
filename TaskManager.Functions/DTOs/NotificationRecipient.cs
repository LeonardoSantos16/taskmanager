using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager_Functions.DTOs
{
    public record NotificationRecipient(Guid UserId, string Email, string Name, string Message);
}
