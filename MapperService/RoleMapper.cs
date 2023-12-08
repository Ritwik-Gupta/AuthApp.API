using RoleDTO = AuthApp.API.DTO.RoleDTO;
using Roles = AuthApp.API.Models.Roles;

namespace AuthApp.API.MapperService
{
    public class RoleMapper
    {
        public static Roles MapRoleFromDTO(string role)
        {
            Enum.TryParse<RoleDTO>(role, out var roleType);
            return (Roles)roleType;
        }
    }
}
