namespace AuthApp.API.DTO
{
    public class UserDTO
    {
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }

    }

    public class UserViewDTO
    {
        public int Id { get; set; }
        public string? Fname { get; set; }
        public string? Lname { get; set; }
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }

    }


    public enum RoleDTO
    {
        User,
        Admin,
        Visitor
    }
}
