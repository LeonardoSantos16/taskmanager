using System;
using System.Collections.Generic;
using System.Text;

namespace TaskManager_Functions.Notifications
{
    public interface IDueSoonEmailSender
    {
        Task SendDueSoonEmailAsync(string toEmail, string toName, string taskTitle, string projectName, DateTime dueDateUtc, CancellationToken cancellationToken = default);
    }
}
