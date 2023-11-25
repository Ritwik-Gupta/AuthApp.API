using AuthApp.API.Models;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

namespace AuthApp.API.Domain
{
    public class AuthAppDbContext : DbContext
    {
        private readonly DbContextOptions _options;
        private readonly IConfiguration _configuration; 

        public AuthAppDbContext(DbContextOptions<AuthAppDbContext> options, IConfiguration configuration): base(options)
        {
            _options = options;
            _configuration = configuration;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<UserSecret> UsersSecrets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(_configuration.GetConnectionString("AdminApp"));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Creating default schema
            modelBuilder.HasDefaultSchema("AuthAppDB");

            //Map entity to table
            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<User>().
                HasOne(e => e.SecretSalt).
                WithOne(e => e.User).
                HasForeignKey<UserSecret>(e => e.UserId)
                .IsRequired();
        }
    }
}
