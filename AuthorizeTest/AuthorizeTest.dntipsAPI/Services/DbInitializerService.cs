using System;
using System.Linq;
using AuthorizeTest.dntipsAPI.Context;
using AuthorizeTest.dntipsAPI.Entities;
using AuthorizeTest.dntipsAPI.Utils;
using AuthorizeTest.Shared.Enums;
using AuthorizeTest.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthorizeTest.dntipsAPI.Services
{
    public interface IDbInitializerService
    {
        /// <summary>
        /// Applies any pending migrations for the context to the database.
        /// Will create the database if it does not already exist.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Adds some default values to the Db
        /// </summary>
        void SeedData();
    }

    public class DbInitializerService : IDbInitializerService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISecurityService _securityService;

        public DbInitializerService(
            IServiceScopeFactory scopeFactory,
            ISecurityService securityService)
        {
            _scopeFactory = scopeFactory;
            _scopeFactory.CheckArgumentIsNull(nameof(_scopeFactory));

            _securityService = securityService;
            _securityService.CheckArgumentIsNull(nameof(_securityService));
        }

        public void Initialize()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<AuthorizeTest.dntipsAPI.Context.ApplicationDbContext>();
            context.Database.Migrate();
        }

        public void SeedData()
        {
            using (var serviceScope = _scopeFactory.CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<AuthorizeTest.dntipsAPI.Context.ApplicationDbContext>())
                {
                    //// Add default roles
                    //var adminRole = new Role { Name = ConstantRoles.Admin };
                    //var userRole = new Role { Name = ConstantRoles.User };
                    //if (!context.Roles.Any())
                    //{
                    //    context.Add(adminRole);
                    //    context.Add(userRole);
                    //    context.SaveChanges();
                    //}

                    // Add Admin & User
                    if (!context.Users.Any())
                    {
                        var adminUser = new User
                        {
                            Username = "Admin",
                            DisplayName = "بهرام قدیری",
                            IsActive = true,
                            LastLoggedIn = null,
                            Password = _securityService.GetSha256Hash("12345678"),
                            SerialNumber = Guid.NewGuid().ToString("N")
                        };
                        context.Add(adminUser);
                        context.SaveChanges();

                        context.Add(new UserRole { Role = RolesEnum.Admin, User = adminUser });
                        context.SaveChanges();
                        var userUser = new User
                        {
                            Username = "User",
                            DisplayName = "پدارم قدیری",
                            IsActive = true,
                            LastLoggedIn = null,
                            Password = _securityService.GetSha256Hash("12345678"),
                            SerialNumber = Guid.NewGuid().ToString("N")
                        };
                        context.Add(userUser);
                        context.SaveChanges();

                        context.Add(new UserRole { Role = RolesEnum.User, User = userUser });
                        context.SaveChanges();
                    }
                }
            }
        }
    }
}
