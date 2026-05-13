using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System;


namespace CozyTest.Services
{
    public class ParticipantFavoriteTestService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        private List<ParticipantsFavoriteTest> _favorites = new();

        public IReadOnlyList<ParticipantsFavoriteTest> Favorites => _favorites;

        public ParticipantFavoriteTestService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();
            _favorites = await db.ParticipantsFavoriteTests
                .Where(f => f.ParticipantId == participantId)
                .ToListAsync();
        }

        public async Task<List<ParticipantsFavoriteTest>> GetParticipantFavoritesAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();
            return await db.ParticipantsFavoriteTests
                .Where(f => f.ParticipantId == participantId)
                .ToListAsync();
        }

        public bool IsFavorite(int testId, int partId)
        {
            using var db = _factory.CreateDbContext();

            return db.ParticipantsFavoriteTests.Any(f => f.TestId == testId && f.ParticipantId == partId);
        }

        public async Task AddToFavoritesAsync(int participantId, int testId)
        {
            using var db = _factory.CreateDbContext();
            
            var exists = await db.ParticipantsFavoriteTests
                .AnyAsync(f => f.ParticipantId == participantId && f.TestId == testId);
            
            if (!exists)
            {
                var favorite = new ParticipantsFavoriteTest
                {
                    ParticipantId = participantId,
                    TestId = testId
                };
                
                db.ParticipantsFavoriteTests.Add(favorite);
                await db.SaveChangesAsync();
                
                _favorites.Add(favorite);
            }
        }

        public async Task RemoveFromFavoritesAsync(int participantId, int testId)
        {
            using var db = _factory.CreateDbContext();
            
            var favorite = await db.ParticipantsFavoriteTests
                .FirstOrDefaultAsync(f => f.ParticipantId == participantId && f.TestId == testId);
            
            if (favorite != null)
            {
                db.ParticipantsFavoriteTests.Remove(favorite);
                await db.SaveChangesAsync();
                
                _favorites.Remove(favorite);
            }
        }

        public async Task ToggleFavoriteAsync(int participantId, int testId)
        {
            if (IsFavorite(testId, participantId))
            {
                await RemoveFromFavoritesAsync(participantId, testId);
            }
            else
            {
                await AddToFavoritesAsync(participantId, testId);
            }
        }

        public async Task<List<Test>> GetFavoriteTestsAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();
            
            return await db.ParticipantsFavoriteTests
                .Where(f => f.ParticipantId == participantId)
                .Include(f => f.Test)
                    .ThenInclude(t => t.Topic)
                .Include(f => f.Test)
                    .ThenInclude(t => t.TestType)
                .Include(f => f.Test)
                    .ThenInclude(t => t.CuratorCreate)
                .Select(f => f.Test)
                .ToListAsync();
        }
    }
}