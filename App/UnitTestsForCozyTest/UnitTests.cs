using CozyTest.Models;
using CozyTest.Services;

namespace UnitTestsForCozyTest
{
    public class CozyTestAppTests
    {
        private CozyTestContext CreateContext()
        {
            var context = TestDbContext.CreateDbContext();
            BaseDbService.Instance.SetContext(context);
            return context;
        }


        private void BaseData(CozyTestContext context)
        {
            
        }

        [Fact]
        public void Test1()
        {

        }
    }
}