using Microsoft.EntityFrameworkCore;
using RaceDay.API.Data;

namespace RaceDay.Tests.TestHelpers
{
    public static class TestDbContextFactory
    {
        public static RaceDayDbContext Create()
        {
            var options = new DbContextOptionsBuilder<RaceDayDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new RaceDayDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }

        public static void Destroy(RaceDayDbContext context)
        {
            context.Database.EnsureDeleted();
            context.Dispose();
        }
    }
}