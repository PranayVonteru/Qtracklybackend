
using Demoproject.Data;
using Demoproject.Dto_s;
using Demoproject.Dtos;
using Demoproject.Models;
using Demoproject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Graph.Models;
using User = Demoproject.Models.User;

namespace Demoproject.Services
{
    public class UserService : IUserService
    {
        private readonly QTraklyDBContext _dbContext;
        private readonly ILogger<UserService> _logger;
        private readonly string _encryptionKey;
        private readonly string _encryptionIV;

        public UserService(QTraklyDBContext dbContext, ILogger<UserService> logger, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _logger = logger;

            // Get encryption keys from appsettings.json - matching your existing structure
            _encryptionKey = configuration["Encryption:AESKey"];
            _encryptionIV = configuration["Encryption:AESIV"];

            // Generate keys if not provided (since your appsettings has empty values)
            if (string.IsNullOrEmpty(_encryptionKey))
            {
                _encryptionKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)); // 256-bit key
                _logger.LogWarning("AESKey is empty in appsettings.json. Generated temporary key for development. IMPORTANT: Set proper keys in production!");
                _logger.LogInformation("Generated AESKey: {Key}", _encryptionKey);
            }

            if (string.IsNullOrEmpty(_encryptionIV))
            {
                _encryptionIV = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)); // 128-bit IV
                _logger.LogWarning("AESIV is empty in appsettings.json. Generated temporary IV for development. IMPORTANT: Set proper IV in production!");
                _logger.LogInformation("Generated AESIV: {IV}", _encryptionIV);
            }

            _logger.LogInformation("UserService initialized with encryption enabled.");
        }

       



        #region Encryption Helper Methods

        private string EncryptString(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_encryptionIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream();
                using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                using var sw = new StreamWriter(cs);
                sw.Write(plainText);
                sw.Close();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error encrypting string");
                return plainText; // Return original if encryption fails
            }
        }

        private string DecryptString(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                using var aes = Aes.Create();
                aes.Key = Convert.FromBase64String(_encryptionKey);
                aes.IV = Convert.FromBase64String(_encryptionIV);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                using var ms = new MemoryStream(Convert.FromBase64String(cipherText));
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);
                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrypting string");
                return cipherText; // Return original if decryption fails
            }
        }

        private UserProfileDto EncryptUserProfileDto(UserProfileDto profile)
        {
            string[] parts = profile.Name.Split('(');
            string name = parts[0].Trim();
            return new UserProfileDto
            {
                Id = profile.Id, // ID is not encrypted for lookup purposes
                Name = EncryptString(name),
                Email = EncryptString(profile.Email),
                Roles = profile.Roles?.Select(EncryptString).ToList() ?? new List<string>()
            };
        }

        private UserDto EncryptUserDto(UserDto user)
        {
            string[] parts = user.Name.Split('(');
            string name = parts[0].Trim();
            return new UserDto
            {
                UserId = user.UserId, // ID is not encrypted for lookup purposes
                Name = EncryptString(name),
                Email = EncryptString(user.Email),
                Roles = user.Roles?.Select(EncryptString).ToList() ?? new List<string>(),
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt
            };
        }

        private UserDto DecryptUserDto(UserDto encryptedUser)
        {
            return new UserDto
            {
                UserId = encryptedUser.UserId,
                Name = DecryptString(encryptedUser.Name),
                Email = DecryptString(encryptedUser.Email),
                Roles = encryptedUser.Roles?.Select(DecryptString).ToList() ?? new List<string>(),
                LastLogin = encryptedUser.LastLogin,
                CreatedAt = encryptedUser.CreatedAt
            };
        }

        #endregion

         #region Public Methods

        public async Task<UserProfileDto> GetUserProfileAsync(GraphServiceClient graphClient, ClaimsPrincipal user)
        {
            var graphUser = await graphClient.Me.GetAsync();
            var roles = user.FindAll("roles")
                .Concat(user.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                .Select(c => c.Value.ToLower())
                .ToList();

            var profile = new UserProfileDto
            {
                Id = graphUser.Id,
                Name = graphUser.DisplayName,
                Email = graphUser.Mail ?? graphUser.UserPrincipalName,
                Roles = roles
            };

            await SaveUserToDatabaseAsync(graphUser, roles);
            return EncryptUserProfileDto(profile); // Return encrypted profile
        }

        public UserProfileDto GetUserProfileFromClaims(ClaimsPrincipal user)
        {
            var userId = user.FindFirst("oid")?.Value
                ?? user.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
                ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst("sub")?.Value;

            var userName = user.FindFirst("name")?.Value;
            var userEmail = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value
                ?? user.FindFirst("preferred_username")?.Value
                ?? user.FindFirst("email")?.Value;
            var roles = user.FindAll("roles")
                .Concat(user.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                .Select(c => c.Value.ToLower())
                .ToList();

            var profile = new UserProfileDto
            {
                Id = userId,
                Name = userName,
                Email = userEmail,
                Roles = roles
            };

            return EncryptUserProfileDto(profile); // Return encrypted profile
        }

        public async Task<(UserDto, string)> GetUserDetailsAsyncNotification(string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return (null, "User not found in the database.");

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = DecryptString(user.Name), // Decrypt when retrieving
                Email = DecryptString(user.Email), // Decrypt when retrieving
                Roles = JsonSerializer.Deserialize<List<string>>(user.Roles)?.Select(DecryptString).ToList() ?? new List<string>(),
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt
            };

            return (EncryptUserDto(userDto), null); // Return encrypted DTO
        }
        public async Task SaveUserToDatabaseAsync(Microsoft.Graph.Models.User graphUser, List<string> roles)
        {
            var userId = graphUser.Id;
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (dbUser == null)
            {
                dbUser = new User
                {
                    UserId = userId,
                    Name = EncryptString(graphUser.DisplayName), // Encrypt before saving
                    Email = EncryptString(graphUser.Mail ?? graphUser.UserPrincipalName), // Encrypt before saving
                    Roles = JsonSerializer.Serialize(roles.Select(EncryptString).ToList()), // Encrypt roles
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(dbUser);
            }
            else
            {
                dbUser.Name = EncryptString(graphUser.DisplayName);
                dbUser.Email = EncryptString(graphUser.Mail ?? graphUser.UserPrincipalName);
                dbUser.Roles = JsonSerializer.Serialize(roles.Select(EncryptString).ToList());
                dbUser.LastLogin = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task SaveBasicUserToDatabaseAsync(string userId, ClaimsPrincipal user)
        {
            var dbUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            var roles = user.FindAll("roles")
                .Concat(user.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role"))
                .Select(c => c.Value.ToLower())
                .ToList();

            var userName = user.FindFirst("name")?.Value;
            var userEmail = user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn")?.Value
                ?? user.FindFirst("preferred_username")?.Value;

            if (dbUser == null)
            {
                dbUser = new User
                {
                    UserId = userId,
                    Name = EncryptString(userName ?? "Unknown"),
                    Email = EncryptString(userEmail ?? "Unknown"),
                    Roles = JsonSerializer.Serialize(roles.Select(EncryptString).ToList()),
                    LastLogin = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(dbUser);
            }
            else
            {
                if (!string.IsNullOrEmpty(userName)) dbUser.Name = EncryptString(userName);
                if (!string.IsNullOrEmpty(userEmail)) dbUser.Email = EncryptString(userEmail);
                dbUser.Roles = JsonSerializer.Serialize(roles.Select(EncryptString).ToList());
                dbUser.LastLogin = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
        }

        public async Task<List<UserDto>> GetDetailsListAsync()
        {
            var users = await _dbContext.Users.ToListAsync();
            var userDtos = users.Select(user => new UserDto
            {
                UserId = user.UserId,
                Name = DecryptString(user.Name), // Decrypt when retrieving
                Email = DecryptString(user.Email), // Decrypt when retrieving
                Roles = JsonSerializer.Deserialize<List<string>>(user.Roles)?.Select(DecryptString).ToList() ?? new List<string>(),
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt

            }).ToList();

            // Return encrypted DTOs for API response
            return userDtos.Select(EncryptUserDto).ToList();
        }



        public async Task<IEnumerable<object>> GetRequestAsync(string id)
        {
            // Fetch DependencyFacts with User Emails
            var list = await _dbContext.DependencyFacts
                              .Where(df => df.UserId == id)
                              .Join(_dbContext.Users,
                                    df => df.TargetUserId,
                                    u => u.UserId,
                                    (df, u) => new
                                    {
                                        df.DependencyTaskId,
                                        df.TargetUserId,
                                        df.Status,
                                        Name = u.Name
                                    })
                              .ToListAsync(); // ✅ Async Execution

            // Fetch DependencyRequests separately
            var listRequests = await _dbContext.DependencyRequests.ToListAsync(); // ✅ Fetching Data First

            // ✅ Perform the join in memory
            var listRequestsWithDetails = listRequests
                                           .Join(list,
                                                 dr => dr.DependencyTaskId,
                                                 df => df.DependencyTaskId,
                                                 (dr, df) => new
                                                 {
                                                     dr.DependencyTaskId,
                                                     dr.TaskName,
                                                     dr.EstimatedImpact,
                                                     dr.RequestedTask,
                                                     dr.Description,
                                                     dr.Priority,
                                                     dr.RequestedDate,
                                                     df.Status, // ✅ Merging Status
                                                     df.Name    // ✅ Merging Name
                                                 })
                                           .ToList();

            return listRequestsWithDetails;
        }

        public async Task<IEnumerable<object>> GetRequestIncomeAsync(string id)
        {
            // Fetch DependencyFacts with User Emails
            var list = await _dbContext.DependencyFacts
                              .Where(df => df.TargetUserId == id && (df.Status=="Pending" || df.Status=="Accepted"))
                              .Join(_dbContext.Users,
                                    df => df.UserId,
                                    u => u.UserId,
                                    (df, u) => new
                                    {
                                        df.DependencyTaskId,
                                        df.UserId,
                                        df.Status,
                                        Name = u.Name
                                    })
                              .ToListAsync(); // ✅ Async Execution

            // Fetch DependencyRequests separately
            var listRequests = await _dbContext.DependencyRequests.ToListAsync(); // ✅ Fetching Data First

            // ✅ Perform the join in memory
            var listRequestsWithDetails = listRequests
                                           .Join(list,
                                                 dr => dr.DependencyTaskId,
                                                 df => df.DependencyTaskId,
                                                 (dr, df) => new
                                                 {
                                                     dr.DependencyTaskId,
                                                     dr.TaskName,
                                                     dr.EstimatedImpact,
                                                     dr.RequestedTask,
                                                     dr.Description,
                                                     dr.Priority,
                                                     dr.RequestedDate,
                                                     df.Status, // ✅ Merging Status
                                                     df.Name    // ✅ Merging Name
                                                 })
                                           .ToList();

            return listRequestsWithDetails;
        }

        public async Task<(UserDto, string)> GetUserDetailsAsync(string userId)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
                return (null, "User not found in the database.");

            var userDto = new UserDto
            {
                UserId = user.UserId,
                Name = DecryptString(user.Name), // Decrypt when retrieving
                Email = DecryptString(user.Email), // Decrypt when retrieving
                Roles = JsonSerializer.Deserialize<List<string>>(user.Roles)?.Select(DecryptString).ToList() ?? new List<string>(),
                LastLogin = user.LastLogin,
                CreatedAt = user.CreatedAt
            };

            return (EncryptUserDto(userDto), null); // Return encrypted DTO
        }

        public async Task RequestDetailsAsync(RequestDto request)
        {


            // Create a new DependencyRequest object
            DependencyRequest requestDto = new DependencyRequest()
            {
                TaskName = request.TaskName,
                Priority = request.Priority,
                Description = request.Description,
                RequestedTask = request.RequestedTask,
                EstimatedImpact = request.EstimatedImpact,
                RequestedDate = DateTime.Now
            };

            // Add the new DependencyRequest to the database
            _dbContext.DependencyRequests.Add(requestDto);
            _dbContext.SaveChanges();

            // Retrieve the TargetUserId based on the email
            var targetUserId = _dbContext.Users
                .Where(u => u.Email == EncryptString(request.TargetUser))
                .Select(u => u.UserId)
                .FirstOrDefault();

            if (targetUserId != null)
            {
                // Create a new DependencyFact object
                DependencyFact dependencyFact = new DependencyFact()
                {
                    DependencyTaskId = requestDto.DependencyTaskId,
                    UserId = request.UserId,
                    TargetUserId = targetUserId,
                    Status = "Pending" // Set a meaningful status
                };

                // Add the new DependencyFact to the database
                _dbContext.DependencyFacts.Add(dependencyFact);
                _dbContext.SaveChanges();

              
                    var fact = new TaskDependencyFact()
                    {
                        TaskId = request.TaskId,
                        DependencyTaskId = requestDto.DependencyTaskId
                    };
                    _dbContext.taskDependencyFacts.Add(fact);
                    _dbContext.SaveChanges();
                }
            else
            {
                // Handle the case where the target user is not found
                Console.WriteLine("Target user not found.");
            }


        }
        public async Task ChangeStatus(int id)
        {
            var dependencyFact = _dbContext.DependencyFacts.Where(u=>u.DependencyTaskId == id).FirstOrDefault();

            dependencyFact.Status = "Rejected";
            _dbContext.SaveChanges();
        }

        public async Task AcceptedStatus(DependencyTaskDto request)
        {
            var dependencyTask = new TaskItem()
            {

                TaskName = request.TaskName,
                Status = request.Status,
                Priority = request.Priority,
                Description = request.Description,
                StartDate = request.StartDate,
                DueDate = request.DueDate,
                EstimatedHours = request.EstimatedHours,
                CompletedHours = request.CompletedHours,
                CreatedBy = request.CreatedBy
            };

            _dbContext.Tasks.Add(dependencyTask);
            _dbContext.SaveChanges();

            var dependencyFacts = _dbContext.DependencyFacts.Where(e => e.DependencyTaskId == request.DependencyTaskId).FirstOrDefault();
            if (dependencyFacts != null && dependencyTask.Id != null)
            {
                dependencyFacts.DependsOnTaskId = dependencyTask.Id;
                dependencyFacts.Status = "Accepted";
            }


            _dbContext.SaveChanges();

            var dependency = _dbContext.DependencyFacts.Where(p => p.DependencyTaskId == request.DependencyTaskId).FirstOrDefault();

            var dependencydetails = _dbContext.DependencyRequests.Where(p => p.DependencyTaskId == request.DependencyTaskId).FirstOrDefault();

            var feedback = new Feedback
            {
                UserId = dependency.UserId,
                ManagerId = dependency.TargetUserId,
                Message = "Your Dependency " + dependencydetails.TaskName + " CreatedAt :" + dependencydetails.RequestedDate + " has been Accepeted",
                SentAt = DateTime.UtcNow,
                IsRead = false
            };
            _dbContext.Feedbacks.Add(feedback);
            _dbContext.SaveChanges();


        }





        //--------------------------------------------------------------------------------------------
        public async Task<List<UserDepartmentDto>> GetAllUserDetailsAsync()

        {

            try

            {

                // Fetch raw user data without deserialization in the query

                var users = await _dbContext.Users

                    .Select(u => new

                    {

                        u.UserId,

                        u.Name,

                        u.Email,

                        u.Roles,

                        u.LastLogin,

                        u.CreatedAt,
                        u.Department,
                        u.SubDepartment,
                        u.Manager

                    })

                    .ToListAsync();

                // Deserialize roles in memory

                var userDtos = users.Select(u => new UserDepartmentDto

                {

                    UserId = u.UserId,

                    Name = u.Name,

                    Email = u.Email,

                    Roles = !string.IsNullOrEmpty(u.Roles)

                        ? JsonSerializer.Deserialize<List<string>>(u.Roles) ?? new List<string>()

                        : new List<string>(),

                    LastLogin = u.LastLogin,

                    CreatedAt = u.CreatedAt,
                    Department=u.Department,
                    SubDepartment=u.SubDepartment,
                    Manager=u.Manager

                }).ToList();

                return userDtos;

            }

            catch (JsonException ex)

            {

                _logger.LogError(ex, "Failed to deserialize roles for users in GetAllUserDetailsAsync");

                throw;

            }

            catch (Exception ex)

            {

                _logger.LogError(ex, "Error fetching all user details. Exception: {Exception}", ex.ToString());

                throw;

            }

        }

        public async Task<(bool, string)> DepartmentDetailsAsync(string userId, DepartmentDto department)

        {

            if (string.IsNullOrEmpty(userId) || department == null)

            {

                return (false, "UserId and department details are required.");

            }

            // Check if the user exists in the Users table

            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.UserId == userId);

            Console.WriteLine(user + "qqqqqqqqqqqqqqqqq");

            if (user == null)

            {

                return (false, "User not found in the database.");

            }

            // Update the existing user with department details

            user.Manager = department.Manager;

            user.Department = department.Department;

            user.SubDepartment = department.SubDepartment;

            await _dbContext.SaveChangesAsync();

            return (true, null);

        }

        public async Task<List<User>> GetManagerDetailsAsync()

        {

            var managers = await _dbContext.Users

                                           .Where(u => u.Roles == $"[\"{EncryptString("manager")}\"]")

                                           .ToListAsync();

            return managers;

        }
        #endregion
    }
}



