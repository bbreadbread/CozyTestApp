using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class CorrespondenceService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Сorrespondence> Correspondences { get; } = new();

        public CorrespondenceService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Сorrespondence correspondence)
        {
            using var db = _factory.CreateDbContext();
            await db.Correspondences.AddAsync(correspondence);
            await db.SaveChangesAsync();
            await App.Current.Dispatcher.InvokeAsync(() =>
                Correspondences.Add(correspondence));
        }

        public async Task GetAllAsync(int? questionId = null)
        {
            using var db = _factory.CreateDbContext();
            List<Сorrespondence> correspondences;

            if (questionId != null)
            {
                var optionIds = await db.Options
                    .Where(o => o.QuestionId == questionId)
                    .Select(o => o.Id)
                    .ToListAsync();

                correspondences = await db.Correspondences
                    .Where(c => optionIds.Contains(c.ConstantId) || optionIds.Contains(c.СorrespondingId))
                    .ToListAsync();
            }
            else
            {
                correspondences = await db.Correspondences.ToListAsync();
            }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Correspondences.Clear();
                foreach (var corr in correspondences)
                    Correspondences.Add(corr);
            });
        }

        public async Task RemoveAsync(Сorrespondence correspondence)
        {
            if (correspondence == null) return;

            using var db = _factory.CreateDbContext();
            db.Correspondences.Remove(correspondence);
            if (await db.SaveChangesAsync() > 0)
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Correspondences.Contains(correspondence))
                        Correspondences.Remove(correspondence);
                });
        }

        public async Task RemoveByQuestionIdAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();
            var optionIds = await db.Options
                .Where(o => o.QuestionId == questionId)
                .Select(o => o.Id)
                .ToListAsync();

            var existing = await db.Correspondences
                .Where(c => optionIds.Contains(c.ConstantId) || optionIds.Contains(c.СorrespondingId))
                .ToListAsync();

            db.Correspondences.RemoveRange(existing);
            await db.SaveChangesAsync();
        }

        public async Task RemoveByOptionIdAsync(int optionId)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Correspondences
                .Where(c => c.ConstantId == optionId || c.СorrespondingId == optionId)
                .ToListAsync();

            db.Correspondences.RemoveRange(existing);
            await db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Сorrespondence correspondence)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Correspondences.FindAsync(correspondence.ConstantId, correspondence.СorrespondingId);
            if (existing != null)
            {
                existing.ConstantId = correspondence.ConstantId;
                existing.СorrespondingId = correspondence.СorrespondingId;
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Сorrespondence>> GetByQuestionIdAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();
            var optionIds = await db.Options
                .Where(o => o.QuestionId == questionId)
                .Select(o => o.Id)
                .ToListAsync();

            return await db.Correspondences
                .Where(c => optionIds.Contains(c.ConstantId) || optionIds.Contains(c.СorrespondingId))
                .ToListAsync();
        }

        public async Task SaveForQuestionAsync(int questionId, List<(int constantId, int correspondingId)> correspondences)
        {
            using var db = _factory.CreateDbContext();

            await RemoveByQuestionIdAsync(questionId);

            foreach (var (constantId, correspondingId) in correspondences)
            {
                var correspondence = new Сorrespondence
                {
                    ConstantId = constantId,
                    СorrespondingId = correspondingId
                };
                db.Correspondences.Add(correspondence);
            }

            await db.SaveChangesAsync();
        }

        public async Task<bool> IsCorrectPairAsync(int constantId, int correspondingId)
        {
            using var db = _factory.CreateDbContext();
            return await db.Correspondences
                .AnyAsync(c => c.ConstantId == constantId && c.СorrespondingId == correspondingId);
        }

        public async Task<List<Сorrespondence>> GetPairsForQuestionAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();
            var optionIds = await db.Options
                .Where(o => o.QuestionId == questionId)
                .Select(o => o.Id)
                .ToListAsync();

            return await db.Correspondences
                .Where(c => optionIds.Contains(c.ConstantId))
                .ToListAsync();
        }

        public async Task<bool> IsConstantOptionAsync(int optionId)
        {
            using var db = _factory.CreateDbContext();

            var correspondences = await db.Correspondences
                .AsNoTracking()
                .Where(c => c.ConstantId == optionId)
                .ToListAsync();

            return correspondences.Any();
        }
    }
}