using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthApp.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public Roles RoleId { get; set; }
        public string? Password { get; set; }
        public string? Token { get; set; }
        public int IsDeleted { get; set; }
        //Adding foriegn key references
        public UserSecret? SecretSalt { get; set; }
    }

    public class UserSecret
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? secretSalt { get; set; }
        public User User { get; set; } = null!;
    }

    public class Role
    {
        public Roles RoleId { get; set; }
        public string? RoleName { get; set; }

        public List<User>? Users { get; set; }
    }

    public enum Roles : int
    {
        User = 0,
        Admin = 1,
        Visitor = 2
    }
}
