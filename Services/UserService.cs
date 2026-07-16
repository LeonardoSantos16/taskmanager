using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using taskmanager.DTOs;
using taskmanager.Models;
using taskmanager.Repositories;

namespace taskmanager.Services
{
    public class UserService
    {
        private readonly PasswordHasher<User> _hasher = new();
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void RegisterUser(UserDtoRequest userDtoRequest)
        {
            if (!IsValidEmail(userDtoRequest.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }

            if (!IsValidPassword(userDtoRequest.Password))
            {
                throw new ArgumentException("Invalid password format.");
            }
            

            var user = new User
            {
                Name = userDtoRequest.Name,
                Email = userDtoRequest.Email,
                PasswordHash = ""
            };
            

            user.PasswordHash = HashPassword(user, userDtoRequest.Password);

            _userRepository.GetByEmailAsync(userDtoRequest.Email).ContinueWith(task =>
            {
                if (task.Result != null)
                {
                    throw new ArgumentException("Email already exists.");
                }
            }).Wait();
            
            _userRepository.Create(user);
        }

        public bool IsValidEmail(string email)
        {
            string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, emailPattern);
        }

        public bool IsValidPassword(string password)
        {
            // Password must be at least 8 characters long, contain at least one uppercase letter,
            // one lowercase letter, one digit, and one special character.
            string passwordPattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$";
            return Regex.IsMatch(password, passwordPattern);
        }

        public string HashPassword(User user, string password)
        {
           return _hasher.HashPassword(user, password);
        }

        public bool VerifyPassword(User user, string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }

}