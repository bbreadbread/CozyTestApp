using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class AttemptService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public List<Attempt> Attempts { get; set; } = new();

        public AttemptService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }
        public async Task<List<Attempt>> GetParticipantAttemptsAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();

            return Attempts = await db.Attempts
                .Where(a => a.ParticipantId == participantId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();
        }

        public async Task<List<Attempt>> GetAttemptsByTestAsync(int participantId, int testId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Attempts
                .Where(a => a.ParticipantId == participantId && a.TestId == testId)
                .OrderByDescending(a => a.StartedAt)
                .ToListAsync();
        }
        public Attempt GetById(int atId)
        {
            using var db = _factory.CreateDbContext();

            return db.Attempts.FirstOrDefault(a => a.Id == atId);
        }

        public async Task<int> GetAttemptsCountAsync(int participantId, int testId)
        {
            using var db = _factory.CreateDbContext();

            return await db.Attempts
                .CountAsync(a => a.ParticipantId == participantId && a.TestId == testId);
        }

        public async Task<bool> HasAvailableAttemptsAsync(int participantId, int testId, int maxAttempts)
        {
            var attemptsCount = await GetAttemptsCountAsync(participantId, testId);
            return attemptsCount < maxAttempts;
        }
        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Attempt attempt)
        {
            if (Application.Current?.Dispatcher == null) return;
            using var db = _factory.CreateDbContext();
            var entity = new Attempt
            {
                ParticipantId = attempt.ParticipantId,
                CountQuestions = attempt.CountQuestions,
                TestId = attempt.TestId,
                StartedAt = attempt.StartedAt,
                FinishedAt = attempt.FinishedAt,
                Score = attempt.Score,
                Status = attempt.Status,
                MarkLvl = attempt.MarkLvl
            };

            await db.Attempts.AddAsync(entity);
            await db.SaveChangesAsync();
        }

        public async Task GetAllAsync(decimal? participantId = null, decimal? testId = null, decimal? curatorId = null, DateTime? date = null)
        {
            using var db = _factory.CreateDbContext();

            var query = db.Attempts
                .AsNoTracking()
                .Include(a => a.Participant)
                .Include(a => a.Test)
                    .ThenInclude(t => t.Topic)
                .Include(a => a.Test)
                    .ThenInclude(t => t.CuratorCreate)
                .AsQueryable();

            if (participantId != null)
                query = query.Where(a => a.ParticipantId == participantId);
            if (testId != null)
                query = query.Where(a => a.TestId == testId);
            if (curatorId != null)
                query = query.Where(a => a.Test.CuratorCreateId == curatorId);
            if (date.HasValue)
                query = query.Where(a => a.StartedAt.Value.Date == date.Value.Date);

            var attempts = await query.ToListAsync();

            var attemptIds = attempts.Select(a => a.Id).ToList();

            var testIds = attempts.Select(a => a.TestId).Distinct().ToList();
            var testsWithCurators = await db.Tests
                .AsNoTracking()
                .Where(t => testIds.Contains(t.Id))
                .Include(t => t.Curators)
                .ToDictionaryAsync(t => t.Id);

            var allAnswers = await db.ParticipantAnswers
                .AsNoTracking()
                .Where(pa => attemptIds.Contains(pa.AttemptId))
                .ToListAsync();

            var answersByAttempt = allAnswers
                .GroupBy(a => a.AttemptId)
                .ToDictionary(g => g.Key, g => g.ToList());

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Attempts.Clear();
                foreach (var attempt in attempts)
                {
                    if (testsWithCurators.TryGetValue((int)attempt.TestId, out var testWithCurators))
                    {
                        attempt.Test.Curators = testWithCurators.Curators;
                    }

                    if (answersByAttempt.TryGetValue(attempt.Id, out var answers))
                    {
                        attempt.ParticipantAnswers = answers;
                    }

                    Attempts.Add(attempt);
                }
            });
        }

        public async Task<List<DateTime>> GetUniqueAttemptDatesAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();
            return await db.Attempts
                .Where(a => a.ParticipantId == participantId && a.StartedAt.HasValue)
                .Select(a => a.StartedAt.Value.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToListAsync();
        }

        public async Task<Attempt> GetLastByTypeAsync(int participantId, int testId)
        {
            using var db = _factory.CreateDbContext();
            return await db.Attempts
                .Where(a => a.ParticipantId == participantId && a.TestId == testId)
                .OrderByDescending(a => a.StartedAt)
                .FirstOrDefaultAsync();
        }

        public async Task RemoveAsync(Attempt attempt)
        {
            if (attempt.Id == 0) return;

            using var db = _factory.CreateDbContext();
            db.Attempts.Remove(attempt);
            if (await db.SaveChangesAsync() > 0)
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Attempts.Contains(attempt))
                        Attempts.Remove(attempt);
                });
        }

        public async Task UpdateAsync(Attempt attempt)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Attempts.FindAsync(attempt.Id);
            if (existing != null)
            {
                existing.ParticipantId = attempt.ParticipantId;
                existing.CountQuestions = existing.CountQuestions;
                existing.TestId = attempt.TestId;
                existing.StartedAt = attempt.StartedAt;
                existing.FinishedAt = attempt.FinishedAt;
                existing.Score = attempt.Score;
                existing.MarkLvl = attempt.MarkLvl;
                existing.Status = attempt.Status;
                existing.AttemptNumber = attempt.AttemptNumber;
                await db.SaveChangesAsync();
            }
        }

        public async Task<List<Attempt>> GetAttemptsByTestAsync(int testId)
        {
            using var db = _factory.CreateDbContext();
            return await db.Attempts
                .Include(x => x.Participant)
                .Where(a => a.TestId == testId)
                .OrderBy(a => a.StartedAt)
                .ToListAsync();
        }
        public async Task<int?> GetLastNumPlusOne(int testId, int partId)
        {
            using var db = _factory.CreateDbContext();

            var lastAttempt = await db.Attempts
                .Where(a => a.TestId == testId && a.ParticipantId == partId)
                .OrderByDescending(a => a.AttemptNumber)
                .FirstOrDefaultAsync();

            return lastAttempt.AttemptNumber != null ? lastAttempt.AttemptNumber + 1 : 1;
        }
    }
}