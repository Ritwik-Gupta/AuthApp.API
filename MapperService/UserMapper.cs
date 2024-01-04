using AuthApp.API.DTO;
using AuthApp.API.Models;

namespace AuthApp.API.MapperService
{
    public static class UserMapper
    {
        public static User? MapFromUserDTO(UserDTO userDTO)
        {
            return new User()
            {
                FirstName = userDTO.Fname,
                LastName = userDTO.Lname,
                Email = userDTO.Email,
                Username = userDTO.Username,
                Password = userDTO.Password,
                RoleId =  RoleMapper.MapRoleFromDTO(userDTO.Role)
            };
        }

        public static User? MapFromUserViewDTO(UserViewDTO userDTO)
        {
            return new User()
            {
                Id = userDTO.Id,
                FirstName = userDTO.Fname,
                LastName = userDTO.Lname,
                Email = userDTO.Email,
                Username = userDTO.Username,
                RoleId = RoleMapper.MapRoleFromDTO(userDTO.Role)
            };
        }

        public static UserViewDTO? MapToUserViewDTO(User userObj)
        {
            return new UserViewDTO()
            {
                Id = userObj.Id,
                Fname = userObj.FirstName,
                Lname = userObj.LastName,
                Email = userObj.Email,
                Username = userObj.Username,
                Role = userObj.RoleId.ToString()
            };
        }
    }
}
