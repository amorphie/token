using amorphie.token.core.Models.Token;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace amorphie.token.data
{
    public class TokenDbContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            var connStr = "Host=localhost:5432;Database=token;Username=postgres;Password=postgres";
            builder.UseNpgsql(connStr);
            return new DatabaseContext(builder.Options);
        }
    }

    public class DatabaseContext : DbContext,IDataProtectionKeyContext
    {
        public DbSet<TokenInfo> Tokens { get; set; }
        public DbSet<Logon> Logon { get; set; }
        public DbSet<FailedLogon> FailedLogon { get; set; }
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;


        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Logon>()
                .HasMany(l => l.FailedLogons)
                .WithOne(l => l.Logon)
                .HasForeignKey(l => l.LogonId);

            builder
                .Entity<FailedLogon>()
                .Property(l => l.Id)
                .ValueGeneratedNever();
        }
    }
}
