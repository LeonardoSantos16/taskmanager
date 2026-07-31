using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using taskmanager.DTOs;
using taskmanager.Models;

namespace taskmanager.Services
{
    public interface IUserService
    {
        Task RegisterUser(UserDtoRequest userDtoRequest);
        Task<User> AuthenticateAsync(string email, string password);
        Task<UserDtoResponse> GetUserById(Guid id, Guid requesterId);
        Task UpdateUserPassword(string email, string newPassword);
        Task UpdateUserName(string email, string newName);
        Task DeleteUser(Guid id);
        bool IsValidEmail(string email);
        bool IsValidPassword(string password);
        string HashPassword(User user, string password);
        bool VerifyPassword(User user, string hashedPassword, string providedPassword);
    }
}