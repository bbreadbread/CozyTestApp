using CozyTest.Models;
using Microsoft.EntityFrameworkCore;

namespace UnitTestsForCozyTest
{
    public class TestDbContext
    {
        public static CozyTestContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<CozyTestContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new CozyTestContext(options);
        }
    }
}
