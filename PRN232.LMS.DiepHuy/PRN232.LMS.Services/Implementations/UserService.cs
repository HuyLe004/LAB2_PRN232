using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PRN232.LMS.Repositories.Interfaces;
using PRN232.LMS.Repositories.Models;
using PRN232.LMS.Repositories.Models.Entities;
using PRN232.LMS.Services.Interfaces;
using PRN232.LMS.Services.Models;
using BC = BCrypt.Net.BCrypt;

namespace PRN232.LMS.Services.Implementations
{
    /// <summary>
    /// User Service - Updated with BCrypt password hashing
    /// YÊU CẦU 9: Password Security using BCrypt
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _repository;

        public UserService(IUserRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            return user == null ? null : MapToDto(user);
        }

        /// <summary>
        /// Get user by username
        /// </summary>
        public async Task<UserDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _repository.GetByUsernameAsync(username);
            return user == null ? null : MapToDto(user);
        }

        /// <summary>
        /// Get all users with pagination and search
        /// </summary>
        public async Task<(List<UserDto>, int)> GetUsersAsync(QueryParameters queryParams)
        {
            var (users, total) = await _repository.GetPagedAsync(
                queryParams.Page,
                queryParams.PageSize,
                queryParams.Search,
                queryParams.Sort
            );

            var userDtos = users.Select(MapToDto).ToList();
            return (userDtos, total);
        }

        /// <summary>
        /// Create new user with BCrypt password hashing
        /// YÊU CẦU 9: Passwords must NOT be stored as plain text - Using BCrypt
        /// </summary>
        public async Task<UserDto> CreateUserAsync(CreateUserRequest request)
        {
            // 🔴 BCrypt password hashing - YÊU CẦU 9: Password Security
            var hashedPassword = BC.HashPassword(request.Password);

            var user = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                Role = request.Role ?? "User"
            };

            await _repository.AddAsync(user);
            await _repository.SaveChangesAsync();

            return MapToDto(user);
        }

        /// <summary>
        /// Update user with BCrypt password hashing
        /// </summary>
        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return null;

            if (!string.IsNullOrEmpty(request.Username))
                user.Username = request.Username;

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BC.HashPassword(request.Password); // 🔴 BCrypt

            if (!string.IsNullOrEmpty(request.Role))
                user.Role = request.Role;

            await _repository.UpdateAsync(user);
            await _repository.SaveChangesAsync();

            return MapToDto(user);
        }

        /// <summary>
        /// Delete user
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                return false;

            await _repository.DeleteAsync(id);
            await _repository.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Map User entity to UserDto
        /// </summary>
        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}
