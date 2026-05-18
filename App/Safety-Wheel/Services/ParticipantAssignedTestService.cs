using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using CozyTest;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.ObjectModel;
using System.Diagnostics;

public class ParticipantAssignedTestService
{
    private readonly IDbContextFactory<CozyTestContext> _factory;
    private HashSet<int> _assignedTestIds = new();
    private List<ParticipantsAssignedTest> _assignments = new();

    public IReadOnlyList<ParticipantsAssignedTest> Assignments => _assignments;
    public ParticipantAssignedTestService(IDbContextFactory<CozyTestContext> factory)
    {
        _factory = factory;
    }

    public async Task GetParticipantAssignmentsAsync(int participantId)
    {
        using var db = _factory.CreateDbContext();
        _assignments = await db.ParticipantsAssignedTests
            .Where(a => a.ParticipantId == participantId)
            .Include(a => a.Test)
            .Include(a => a.Participant)
            .Include(a => a.Curator)
            .ToListAsync();

        _assignedTestIds = _assignments.Select(a => a.TestId).ToHashSet();

        Debug.WriteLine($"GetParticipantAssignmentsAsync: Загружено {_assignments.Count} назначений для участника {participantId}");
        foreach (var a in _assignments)
        {
            Debug.WriteLine($"  - TestId: {a.TestId}, Дата: {a.DateTimeAssigned}, Test: {a.Test?.Name}");
        }
    }

    public bool IsAssigned(int testId) => _assignedTestIds.Contains(testId);

    public ParticipantsAssignedTest? GetAssignment(int testId, int partId)
    {
        return _assignments.FirstOrDefault(a => a.TestId == testId && a.ParticipantId == partId);
    }
    public async Task GetAllAssignmentsForCuratorAsync(int curatorId)
    {
        using var context = _factory.CreateDbContext();

        _assignments = await context.ParticipantsAssignedTests
            .Where(a => a.ResponsibleId == curatorId)
            .Include(a => a.Participant)
            .Include(a => a.Curator)
            .Include(a => a.Test)
            .ToListAsync();
    }

    public bool IsAssigned(int testId, int participantId)
    {
        return _assignments.Any(a => a.TestId == testId && a.ParticipantId == participantId);
    }

    public async Task AssignTestToParticipantAsync(int participantId, int testId, DateTime? assignedDate)
    {
        using var context = _factory.CreateDbContext();

        var assignment = new ParticipantsAssignedTest
        {
            ParticipantId = participantId,
            TestId = testId,
            ResponsibleId = CurrentUser.Id,
            DateTimeAssigned = assignedDate ?? DateTime.Now
        };

        context.ParticipantsAssignedTests.Add(assignment);
        await context.SaveChangesAsync();

        _assignments.Add(assignment);
    }

    public async Task AssignTestToParticipantsAsync(int testId, List<Participant> participants, DateTime assignedDate)
    {
        using var context = _factory.CreateDbContext();

        foreach (var participant in participants)
        {
            var existing = await context.ParticipantsAssignedTests
                .FirstOrDefaultAsync(a => a.ParticipantId == participant.Id && a.TestId == testId);

            if (existing == null)
            {
                var assignment = new ParticipantsAssignedTest
                {
                    ParticipantId = participant.Id,
                    TestId = testId,
                    ResponsibleId = CurrentUser.Id,
                    DateTimeAssigned = assignedDate
                };

                context.ParticipantsAssignedTests.Add(assignment);
                _assignments.Add(assignment);
            }
            else
            {
                existing.DateTimeAssigned = assignedDate;
                context.Entry(existing).State = EntityState.Modified;
            }
        }

        await context.SaveChangesAsync();
    }
    public async Task AssignTestToParticipantsAsync(int testId, ObservableCollection<Participant> participants, DateTime assignedDate)
    {
        using var context = _factory.CreateDbContext();

        foreach (var participant in participants)
        {
            var existing = await context.ParticipantsAssignedTests
                .FirstOrDefaultAsync(a => a.ParticipantId == participant.Id && a.TestId == testId);

            if (existing == null)
            {
                var assignment = new ParticipantsAssignedTest
                {
                    ParticipantId = participant.Id,
                    TestId = testId,
                    ResponsibleId = CurrentUser.Id,
                    DateTimeAssigned = assignedDate
                };

                context.ParticipantsAssignedTests.Add(assignment);
                _assignments.Add(assignment);
            }
            else
            {
                existing.DateTimeAssigned = assignedDate;
                context.Entry(existing).State = EntityState.Modified;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveAssignmentAsync(int participantId, int testId)
    {
        using var context = _factory.CreateDbContext();

        var assignment = await context.ParticipantsAssignedTests
            .FirstOrDefaultAsync(a => a.ParticipantId == participantId && a.TestId == testId);

        if (assignment != null)
        {
            context.ParticipantsAssignedTests.Remove(assignment);
            await context.SaveChangesAsync();

            _assignments.Remove(assignment);
        }
    }
}