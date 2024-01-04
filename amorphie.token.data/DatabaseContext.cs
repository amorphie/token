using amorphie.token.core.Models.Token;
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

    public class DatabaseContext : DbContext
    {
        public DbSet<TokenInfo> Tokens { get; set; }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
        {

        }
    }
}