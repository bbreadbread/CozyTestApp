using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace CozyTest.Services
{
    public class ParticipantPublicTestService
    {
        private readonly IDbContextFactory<CozyTestContext> _contextFactory;
        private List<ParticipantsPublicTest> _publicTests = new();

        public IReadOnlyList<ParticipantsPublicTest> PublicTests => _publicTests;

        public ParticipantPublicTestService(IDbContextFactory<CozyTestContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task GetAllAsync(int testId)
        {
            using var context = _contextFactory.CreateDbContext();

            _publicTests = await context.ParticipantsPublicTests
                .Where(p => p.TestId == testId)
                .Include(p => p.Participant)
                .Include(p => p.Responsible)
                .Include(p => p.Test)
                .ToListAsync();
        }

        public async Task GetParticipantPublicTestsAsync(int participantId)
        {
            using var context = _contextFactory.CreateDbContext();

            _publicTests = await context.ParticipantsPublicTests
                .Where(p => p.ParticipantId == participantId)
                .Include(p => p.Participant)
                .Include(p => p.Responsible)
                .Include(p => p.Test)
                .ToListAsync();
        }
        public async Task GetPublicTestsForParticipantAsync(int participantId)
        {
            using var context = _contextFactory.CreateDbContext();

            _publicTests = await context.ParticipantsPublicTests
                .Where(p => p.ParticipantId == participantId)
                .Include(p => p.Test)
                    .ThenInclude(t => t.Topic)
                .Include(p => p.Test)
                    .ThenInclude(t => t.TestType)
                .Include(p => p.Test)
                    .ThenInclude(t => t.CuratorCreate)
                .Include(p => p.Responsible)
                .ToListAsync();
        }

        public bool IsPublished(int testId, int participantId)
        {
            return _publicTests.Any(p => p.TestId == testId && p.ParticipantId == participantId);
        }

        public ParticipantsPublicTest? GetPublicTest(int testId, int partId)
        {
            return _publicTests.FirstOrDefault(p => p.TestId == testId && p.ParticipantId == partId);
        }
        public async Task SwitchParticipantPublicStatusAsync(int participantId, int testId)
        {
            using var context = _contextFactory.CreateDbContext();

            var existing = await context.ParticipantsPublicTests
                .FirstOrDefaultAsync(p => p.ParticipantId == participantId && p.TestId == testId);

            if (existing != null)
            {
                context.ParticipantsPublicTests.Remove(existing);
                _publicTests.Remove(existing);
            }
            else
            {
                var newPublicTest = new ParticipantsPublicTest
                {
                    ParticipantId = participantId,
                    TestId = testId,
                    ResponsibleId = CurrentUser.Id 
                };

                context.ParticipantsPublicTests.Add(newPublicTest);
                _publicTests.Add(newPublicTest);
            }

            await context.SaveChangesAsync();
        }

        public async Task SwitchParticipantPublicStatusAsync(int testId, ObservableCollection<Participant> participants, bool remove)
        {
            using var context = _contextFactory.CreateDbContext();

            if (remove)
            {
                var toRemove = await context.ParticipantsPublicTests
                    .Where(p => p.TestId == testId && participants.Select(x => x.Id).Contains(p.ParticipantId))
                    .ToListAsync();

                context.ParticipantsPublicTests.RemoveRange(toRemove);

                foreach (var item in toRemove)
                    _publicTests.Remove(item);
            }
            else
            {
                var existingIds = await context.ParticipantsPublicTests
                    .Where(p => p.TestId == testId && participants.Select(x => x.Id).Contains(p.ParticipantId))
                    .Select(p => p.ParticipantId)
                    .ToListAsync();

                var newParticipants = participants.Where(p => !existingIds.Contains(p.Id)).ToList();

                foreach (var participant in newParticipants)
                {
                    var newPublicTest = new ParticipantsPublicTest
                    {
                        ParticipantId = participant.Id,
                        TestId = testId,
                        ResponsibleId = CurrentUser.Id
                    };

                    context.ParticipantsPublicTests.Add(newPublicTest);
                    _publicTests.Add(newPublicTest);
                }
            }

            await context.SaveChangesAsync();
        }

        public async Task<List<Participant>> GetPublishedParticipantsAsync(int testId)
        {
            using var context = _contextFactory.CreateDbContext();

            var participants = await context.ParticipantsPublicTests
                .Where(p => p.TestId == testId)
                .Include(p => p.Participant)
                .Select(p => p.Participant)
                .ToListAsync();

            return participants;
        }

        public async Task RemoveAllPublicationsAsync(int testId)
        {
            using var context = _contextFactory.CreateDbContext();

            var toRemove = await context.ParticipantsPublicTests
                .Where(p => p.TestId == testId)
                .ToListAsync();

            context.ParticipantsPublicTests.RemoveRange(toRemove);

            _publicTests = _publicTests.Where(p => p.TestId != testId).ToList();

            await context.SaveChangesAsync();
        }

    }
}