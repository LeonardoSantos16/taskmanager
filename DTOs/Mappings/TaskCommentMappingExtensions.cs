using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using taskmanager.Models;

namespace taskmanager.DTOs.Mappings
{
    public static class TaskCommentMappingExtensions
    {
        public static TaskCommentDtoResponse ToDtoResponse(this TaskComment taskComment)
        {
            return new TaskCommentDtoResponse
            {
                Id = taskComment.Id,
                Content = taskComment.Content
            };
        }

        public static TaskComment ToModel(this TaskCommentDtoRequest taskComment, Guid authorId, Guid taskItemId)
        {
            return new TaskComment
            {
                Content = taskComment.Content,
                AuthorId = authorId,
                TaskItemId = taskItemId,
                CreatedAt = DateTime.UtcNow
            };
        }

    }
}