using CARDB_EF.Data;
using CARDB_EF.Models.DTOs;
using CARDB_EF.Models.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace CARDB_EF.Services
{
    /// <summary>
    /// Manages user-related operations including authentication, registration, and profile management.
    /// </summary>
    public class UserService
    {
        private readonly CarDbContext cdb;
        private readonly LogService logger;
        private readonly FileLoggerService fileLogger;
        private readonly IConfiguration config;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserService"/>.
        /// </summary>
        /// <param name="cdb">The database context for data access.</param>
        /// <param name="logger">The service for creating detailed log messages.</param>
        /// <param name="fileLogger">The service for logging messages to a file.</param>
        /// <param name="config">The application configuration for accessing settings like JWT keys.</param>
        public UserService(CarDbContext cdb, LogService logger, FileLoggerService fileLogger, IConfiguration config)
        {
            this.cdb = cdb;
            this.logger = logger;
            this.fileLogger = fileLogger;
            this.config = config;
        }

        /// <summary>
        /// Hashes a plain-text password using BCrypt.
        /// </summary>
        /// <param name="password">The plain-text password to hash.</param>
        /// <returns>The resulting password hash.</returns>
        private string HashPassword(string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
            return hashedPassword;
        }

        /// <summary>
        /// Verifies a plain-text password against a stored BCrypt hash.
        /// </summary>
        /// <param name="password">The plain-text password to verify.</param>
        /// <param name="hash">The stored hash to verify against.</param>
        /// <returns>True if the password matches the hash; otherwise, false.</returns>
        /// <remarks>
        /// This method includes exception handling and will return false if the hash is invalid.
        /// </remarks>
        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError verifying password: {ex.Message}\n");
            }
            return false;
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) for an authenticated user.
        /// </summary>
        /// <param name="user">The user for whom to generate the token.</param>
        /// <returns>A JWT string.</returns>
        private string GenerateJwtToken(RegisteredUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim("userId", user.UserId.ToString()),
                new Claim("role", user.IsAdmin == true ? "Admin" : "User")
            };

            var token = new JwtSecurityToken(
                issuer: config["Jwt:Issuer"],
                audience: config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Authenticates a user based on their username and password.
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="password">The user's plain-text password.</param>
        /// <returns>A JWT string if authentication is successful; otherwise, null.</returns>
        /// <remarks>
        /// This method checks for valid credentials, ensures the user account is active, and logs all login attempts.
        /// </remarks>
        public string ValidateLogin(string username, string password)
        {
            try
            {
                var user = cdb.Users.FirstOrDefault(u => u.Username == username);

                if (user == null || !VerifyPassword(password, user.Password))
                {
                    fileLogger.LogAsync($"Login failed: Invalid credentials for username '{username}'.");
                    return null;
                }

                if (user.IsActive == false)
                {
                    fileLogger.LogAsync($"Login failed: Attempt to log in to deactivated account for user '{username}'.");
                    return null;
                }

                fileLogger.LogAsync($"Login successful for user: '{username}'.");
                string token = GenerateJwtToken(user);

                return token;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError validating login for {username}: {ex.Message}\n");
                fileLogger.LogAsync($"DATABASE ERROR during login validation for username '{username}'. Exception: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// Retrieves a list of all registered users, ordered by UserId.
        /// </summary>
        /// <returns>A list of <see cref="RegisteredUser"/> objects. Returns an empty list if no users are found or an error occurs.</returns>
        public List<RegisteredUser> GetAllUsers()
        {
            var users = new List<RegisteredUser>();
            try
            {
                users = cdb.Users.OrderBy(u => u.UserId).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError retrieving all users: {ex.Message}\n");
            }
            return users;
        }

        /// <summary>
        /// Retrieves a user's profile by their unique identifier.
        /// </summary>
        /// <param name="userId">The ID of the user to retrieve.</param>
        /// <returns>The <see cref="RegisteredUser"/> object if found; otherwise, null.</returns>
        public RegisteredUser GetUserProfile(int userId)
        {
            RegisteredUser user = null;
            try
            {
                user = cdb.Users.FirstOrDefault(u => u.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError retrieving user profile for ID {userId}: {ex.Message}\n");
            }
            return user;
        }

        /// <summary>
        /// Creates a new user in the database.
        /// </summary>
        /// <param name="user">The user object containing the data for the new user. The password should be plain text.</param>
        /// <param name="error">An output parameter that will contain an error message if the operation fails.</param>
        /// <returns>True if the user was created successfully; otherwise, false.</returns>
        /// <remarks>
        /// This method will hash the user's password and set the account to active by default.
        /// It also handles database constraint violations (e.g., duplicate username, email).
        /// </remarks>
        public bool CreateUser(RegisteredUser user, out string error)
        {
            bool success = false;
            error = null;
            try
            {
                if (user == null || string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
                    error = "Missing required fields.";
                else
                {
                    user.Password = HashPassword(user.Password);
                    user.IsAdmin = false;
                    user.IsActive = true;
                    user.RecordTime = DateTime.Now;

                    cdb.Users.Add(user);
                    cdb.SaveChanges();

                    Console.WriteLine($"\n#{user.UserId} User created successfully.\n");

                    string logMessage = logger.UserCreateLogger(user);
                    var logToUpdate = cdb.UserLogs
                                         .Where(l => l.UserId == user.UserId && l.ActionType == "INSERT")
                                         .FirstOrDefault();

                    if (logToUpdate != null)
                    {
                        logToUpdate.ActionDetails = logMessage;
                        cdb.SaveChanges();
                    }

                    success = true;
                }
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Error creating user: {msg}");

                if (msg.Contains("ORA-00001"))
                {
                    var upper = msg.ToUpper();
                    if (upper.Contains("USERNAME"))
                        error = "This username is already in use.";
                    else if (upper.Contains("MAIL"))
                        error = "This email address is already in use.";
                    else if (upper.Contains("PHONE"))
                        error = "This phone number is already in use.";
                    else
                        error = "Duplicate value.";
                }
                else
                    error = "Could not create the user.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error creating user: {ex.Message}\n");
                error = "An error occurred.";
            }
            return success;
        }

        /// <summary>
        /// Updates an existing user's profile information.
        /// </summary>
        /// <param name="userId">The ID of the user to edit.</param>
        /// <param name="dto">A data transfer object containing the updated information.</param>
        /// <param name="error">An output parameter that will contain an error message if the operation fails.</param>
        /// <returns>True if the user was updated successfully; otherwise, false.</returns>
        /// <remarks>
        /// To change the password, both the old password and the new password must be provided in the DTO.
        /// </remarks>
        public bool EditUser(int userId, UserUpdateDto dto, out string error)
        {
            bool success = false;
            error = null;
            try
            {
                var userAsNoTracking = cdb.Users.AsNoTracking().FirstOrDefault(u => u.UserId == userId);
                if (userAsNoTracking == null)
                {
                    error = "User not found.";
                }
                else
                {
                    bool canUpdate = true;
                    if (!string.IsNullOrWhiteSpace(dto.Password))
                    {
                        if (string.IsNullOrWhiteSpace(dto.OldPassword))
                        {
                            error = "Current password is required to set a new password.";
                            canUpdate = false;
                        }
                        else if (!VerifyPassword(dto.OldPassword, userAsNoTracking.Password))
                        {
                            error = "The current password you entered is incorrect.";
                            canUpdate = false;
                        }
                    }

                    if (canUpdate)
                    {
                        string logMessage = logger.UserEditLogger(userAsNoTracking, dto);

                        var userToUpdate = cdb.Users.Find(userId);
                        userToUpdate.Mail = dto.Mail;
                        userToUpdate.Phone = dto.Phone;
                        userToUpdate.UserLocation = dto.UserLocation;

                        if (!string.IsNullOrWhiteSpace(dto.Password))
                            userToUpdate.Password = HashPassword(dto.Password);

                        cdb.SaveChanges();
                        Console.WriteLine($"\n#{userToUpdate.UserId} User edited successfully.\n");

                        if (!string.IsNullOrWhiteSpace(logMessage))
                        {
                            var logToUpdate = cdb.UserLogs
                                                 .Where(l => l.UserId == userId && l.ActionType == "UPDATE")
                                                 .OrderByDescending(l => l.LogId)
                                                 .FirstOrDefault();

                            if (logToUpdate != null)
                            {
                                logToUpdate.ActionDetails = logMessage;
                                cdb.SaveChanges();
                            }
                        }
                        success = true;
                    }
                }
            }
            catch (DbUpdateException ex)
            {
                var msg = ex.InnerException?.Message ?? ex.Message;
                Console.WriteLine($"Error updating user {userId}: {msg}");

                if (msg.Contains("ORA-00001"))
                {
                    var upper = msg.ToUpper();
                    if (upper.Contains("MAIL"))
                        error = "This email address is already in use.";
                    else if (upper.Contains("PHONE"))
                        error = "This phone number is already in use.";
                    else
                        error = "Duplicate value.";
                }
                else
                {
                    error = "Could not update the user.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nUnexpected error updating user {userId}: {ex.Message}\n");
                error = "An unexpected error occurred.";
            }
            return success;
        }

        /// <summary>
        /// Deactivates a user's account, preventing them from logging in.
        /// </summary>
        /// <param name="userId">The ID of the user to deactivate.</param>
        /// <returns>True if the user was found and deactivated; otherwise, false.</returns>
        public bool DeactivateUser(int userId)
        {
            bool success = false;
            try
            {
                var user = cdb.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.IsActive = false;
                    cdb.SaveChanges();
                    Console.WriteLine($"\n#{user.UserId} User deactivated successfully.\n");

                    string logMessage = logger.UserStatusLogger(userId, false);
                    var logToUpdate = cdb.UserLogs
                                         .Where(l => l.UserId == userId && l.ActionType == "UPDATE")
                                         .OrderByDescending(l => l.LogId)
                                         .FirstOrDefault();

                    if (logToUpdate != null)
                    {
                        logToUpdate.ActionDetails = logMessage;
                        cdb.SaveChanges();
                    }
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError deactivating user {userId}: {ex.Message}\n");
            }
            return success;
        }

        /// <summary>
        /// Activates a user's account, allowing them to log in.
        /// </summary>
        /// <param name="userId">The ID of the user to activate.</param>
        /// <returns>True if the user was found and activated; otherwise, false.</returns>
        public bool ActivateUser(int userId)
        {
            bool success = false;
            try
            {
                var user = cdb.Users.FirstOrDefault(u => u.UserId == userId);
                if (user != null)
                {
                    user.IsActive = true;
                    cdb.SaveChanges();
                    Console.WriteLine($"\n#{user.UserId} User activated successfully.\n");

                    string logMessage = logger.UserStatusLogger(userId, true);
                    var logToUpdate = cdb.UserLogs
                                         .Where(l => l.UserId == userId && l.ActionType == "UPDATE")
                                         .OrderByDescending(l => l.LogId)
                                         .FirstOrDefault();

                    if (logToUpdate != null)
                    {
                        logToUpdate.ActionDetails = logMessage;
                        cdb.SaveChanges();
                    }
                    success = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nError activating user {userId}: {ex.Message}\n");
            }
            return success;
        }
    }
}