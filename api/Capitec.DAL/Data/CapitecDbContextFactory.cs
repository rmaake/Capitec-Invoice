using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Capitec.DAL.Data
{
    internal class CapitecDbContextFactory : IDesignTimeDbContextFactory<CapitecDbContext>
    {
        public CapitecDbContext CreateDbContext(
            string[] args)
        {
            var optionsBuilder =
                new DbContextOptionsBuilder<CapitecDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=localhost;Database=CapitecDb;Trusted_Connection=True;TrustServerCertificate=True;");

            return new CapitecDbContext(
                optionsBuilder.Options, false);
        }
    }
}
