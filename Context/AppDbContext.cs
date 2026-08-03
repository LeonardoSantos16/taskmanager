using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using taskmanager.Models;

namespace taskmanager.Context
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            
        }

        public DbSet<Project> Projects {get; set;}
        public DbSet<ProjectMember> ProjectMembers {get; set;}
        public DbSet<TaskComment> TaskComments {get; set;}
        public DbSet<TaskItem> TaskItems {get; set;}
        public DbSet<User> Users {get; set;}
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Entity<User>().ToTable("users");
        }
    }
}