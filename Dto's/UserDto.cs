using System;
using System.Collections.Generic;

namespace Demoproject.Dtos
{
    public class UserDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class UserDepartmentDto
    {
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public DateTime LastLogin { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Manager { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string SubDepartment { get; set; } = string.Empty;
    }
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }

    public class AuthUserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IEnumerable<string> Roles { get; set; } = new List<string>();
        public bool IsAuthenticated { get; set; }
        public string AuthenticationType { get; set; } = string.Empty;
    }
}