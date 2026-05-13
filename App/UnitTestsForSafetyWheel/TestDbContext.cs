using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;


namespace UnitTestsForSafetyWheel
{
    public static class TestDbContext
    {
        public static SafetyWheelContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<SafetyWheelContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new SafetyWheelContext(options);
        }
    }
}

