using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using AuthorizeTest.dntipsAPI.Entities;
using AuthorizeTest.dntipsAPI.Utils;
using AuthorizeTest.dntipsAPI.Context;
using AuthorizeTest.Shared.Enums;
using AuthorizeTest.Shared.Utils;

namespace AuthorizeTest.dntipsAPI.Services
{
    public interface IRolesService
    {
        Task<List<RolesEnum>> FindUserRolesAsync(int userId);
        Task<bool> IsUserInRoleAsync(int userId, RolesEnum roleName);
        Task<List<User>> FindUsersInRoleAsync(RolesEnum roleName);
    }

    public class RolesService : IRolesService
    {
        private readonly IUnitOfWork _uow;
        private readonly DbSet<UserRole> _userRoles;
        private readonly DbSet<User> _users;

        public RolesService(IUnitOfWork uow)
        {
            _uow = uow;
            _uow.CheckArgumentIsNull(nameof(_uow));

            _users = _uow.Set<User>();
            _userRoles = _uow.Set<UserRole>();
        }

        public Task<List<RolesEnum>> FindUserRolesAsync(int userId)
        {
            var userRolesQuery = from userRoles in _userRoles
                                 where userRoles.UserId == userId
                                 select userRoles.Role;

            return userRolesQuery.OrderBy(x => x).ToListAsync();
        }

        public async Task<bool> IsUserInRoleAsync(int userId, RolesEnum roleName)
        {
            var isUserInRole = await _userRoles.AnyAsync(a => a.UserId == userId && a.Role == roleName);
            return isUserInRole;
        }

        public async Task<List<User>> FindUsersInRoleAsync(RolesEnum roleName)
        {
            var roleUserIdsQuery = from userRoles in _userRoles.Where(a => a.Role == roleName)
                                   from user in _users.Where(a => a.Id == userRoles.UserId)
                                   select user;
            var usersInRole = await roleUserIdsQuery.ToListAsync();
            return usersInRole;
        }
    }
}
