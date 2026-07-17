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
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task RegisterUser(UserDtoRequest userDtoRequest)
        {
            if (!IsValidEmail(userDtoRequest.Email))
            {
                throw new ArgumentException("Invalid email format.");
            }

            if (!IsValidPassword(userDtoRequest.Password))
            {
                throw new ArgumentException("Invalid password format.");
            }
            
            var emailExists = await _userRepository.GetByEmailAsync(userDtoRequest.Email);
            if (emailExists != null)
            {
                throw new ArgumentException("Email already exists");
            }
            
            var user = new User
            {
                Name = userDtoRequest.Name,
                Email = userDtoRequest.Email,
                PasswordHash = ""
            };
            

            user.PasswordHash = HashPassword(user, userDtoRequest.Password);

            var existingUser = await _userRepository.GetByEmailAsync(userDtoRequest.Email);
            if (existingUser != null)
            {
                throw new ArgumentException("Email already exists.");
            }

            await _userRepository.Create(user);
        }

        public async Task<UserDtoResponse> GetUserById(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            return new UserDtoResponse
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
        }

        public void UpdateUserPassword(string email, string newPassword)
        {
            if (!IsValidPassword(newPassword))
            {
                throw new ArgumentException("Invalid password format.");
            }

            var user = _userRepository.GetByEmailAsync(email).Result;

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            user.PasswordHash = HashPassword(user, newPassword);
            _userRepository.Update(user);
        }

        public async void UpdateUserName(string email, string newName)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            user.Name = newName;
            await _userRepository.Update(user);
        }

        public async void DeleteUser(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            await _userRepository.Delete(user);
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