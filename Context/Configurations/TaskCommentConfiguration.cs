using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using taskmanager.Models;

namespace taskmanager.Context.Configurations
{
    public class TaskCommentConfiguration : IEntityTypeConfiguration<TaskComment>
    {
        public void Configure(EntityTypeBuilder<TaskComment> builder)
        {
            builder
                .HasOne(tc => tc.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(tc => tc.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);    

            builder
                .HasOne(tc => tc.TaskItem)
                .WithMany(ti => ti.Comments)
                .HasForeignKey(tc => tc.TaskItemId)
                .OnDelete(DeleteBehavior.Cascade);                
        }
    }
}