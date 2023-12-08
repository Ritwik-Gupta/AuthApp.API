namespace AuthApp.API.DTO
{
    public class UserDTO
    {
        public string fname { get; set; }
        public string lname { get; set; }
        public string email { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string role { get; set; }

    }

    public class UserViewDTO
    {
        public int? id { get; set; }
        public string? fname { get; set; }
        public string? lname { get; set; }
        public string email { get; set; }
        public string? username { get; set; }
        public string? role { get; set; }

    }


    public enum RoleDTO
    {
        User,
        Admin,
        Visitor
    }
}
