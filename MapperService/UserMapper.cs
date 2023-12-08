using AuthApp.API.DTO;
using AuthApp.API.Models;

namespace AuthApp.API.MapperService
{
    public static class UserMapper
    {
        public static User MapUserFromDTO(UserDTO userDTO)
        {
            if (userDTO != null)
            {
                return new User()
                {
                    FirstName = userDTO.fname,
                    LastName = userDTO.lname,
                    Email = userDTO.email,
                    Username = userDTO.username,
                    Password = userDTO.password,
                    RoleId =  RoleMapper.MapRoleFromDTO(userDTO.role)
                };
            }
            return null;
        }
    }
}
