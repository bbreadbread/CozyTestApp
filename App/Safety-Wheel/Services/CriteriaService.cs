using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using CozyTest.ViewModels;

namespace CozyTest.Services
{
    public class CriteriaService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Criteria> Criteria { get; } = new();

        public CriteriaService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Criteria criterion)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Criteria
            {
                TestId = criterion.TestId,
                Name = criterion.Name,
                MinPercent = criterion.MinPercent,
                IsActive = criterion.IsActive,
                OrderNumber = criterion.OrderNumber
            };

            await db.Criteria.AddAsync(entity);
            await db.SaveChangesAsync();

            criterion.Id = entity.Id;

            await App.Current.Dispatcher.InvokeAsync(() =>
                Criteria.Add(entity));
        }

        public async Task GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            var criteria = await db.Criteria.ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Criteria.Clear();
                foreach (var criterion in criteria)
                    Criteria.Add(criterion);
            });
        }

        public async Task GetAllByTestAsync(int? testId = null)
        {
            using var db = _factory.CreateDbContext();
            List<Criteria> criteria;

            if (testId != null)
            {
                criteria = await db.Criteria
                    .Where(c => c.TestId == testId)
                    .OrderBy(c => c.OrderNumber)
                    .ToListAsync();
            }
            else
            {
                criteria = await db.Criteria.ToListAsync();
            }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Criteria.Clear();
                foreach (var criterion in criteria)
                    Criteria.Add(criterion);
            });
        }

        public async Task<Criteria> GetLastAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Criteria
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
        }

        public Criteria GetById(int id)
        {
            return Criteria.FirstOrDefault(q => q.Id == id);
        }

        public async Task RemoveAsync(Criteria criterion)
        {
            if (criterion == null) return;

            using var db = _factory.CreateDbContext();
            db.Criteria.Remove(criterion);

            if (await db.SaveChangesAsync() > 0)
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Criteria.Contains(criterion))
                        Criteria.Remove(criterion);
                });
        }

        public async Task UpdateAsync(Criteria criteria)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Criteria.FindAsync(criteria.Id);
            if (existing != null)
            {
                existing.Name = criteria.Name;
                existing.MinPercent = criteria.MinPercent;
                existing.IsActive = criteria.IsActive;
                existing.OrderNumber = criteria.OrderNumber;
                await db.SaveChangesAsync();
            }
        }

        public async Task RemoveByTestIdAsync(int testId)
        {
            using var db = _factory.CreateDbContext();
            var criteriaToRemove = await db.Criteria
                .Where(c => c.TestId == testId)
                .ToListAsync();

            db.Criteria.RemoveRange(criteriaToRemove);
            await db.SaveChangesAsync();
        }
    }
}