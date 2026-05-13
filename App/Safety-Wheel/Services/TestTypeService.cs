using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class DTestTypeService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<DTestType> DTestTypes { get; } = new();

        public DTestTypeService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            var query = await db.DTestTypes.ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                DTestTypes.Clear();
                foreach (var testType in query)
                    DTestTypes.Add(testType);
            });
        }

        public DTestType GetTypeById(int? type)
        {
            return DTestTypes.FirstOrDefault(t => t.Id == type);
        }
    }
}