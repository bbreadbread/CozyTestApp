using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class TestService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        private List<Test> _tests = new();

        public IReadOnlyList<Test> Tests => _tests;
        public List<Test> AvailableTests { get; private set; } = new();

        public async Task GetAvailableTestsForParticipantAsync(int participantId)
        {
            using var db = _factory.CreateDbContext();

            var assignedTestIds = await db.ParticipantsAssignedTests
                .Where(a => a.ParticipantId == participantId)
                .Select(a => a.TestId)
                .ToListAsync();

            var publicTestIds = await db.ParticipantsPublicTests
                .Where(p => p.ParticipantId == participantId)
                .Select(p => p.TestId)
                .ToListAsync();

            var allAvailableTestIds = assignedTestIds.Union(publicTestIds).Distinct().ToList();

            AvailableTests = await db.Tests
                .Where(t => allAvailableTestIds.Contains(t.Id) && (t.IsArchive == false || t.IsArchive == null))
                .Include(t => t.Topic)
                .Include(t => t.TestType)
                .Include(t => t.CuratorCreate)
                .ToListAsync();
        }
        public TestService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task<Test?> GetTestWithDetailsAsync(int testId)
        {
            using var db = _factory.CreateDbContext();

            var test = await db.Tests
                .Include(t => t.Topic)
                .Include(t => t.CuratorCreate)
                .Include(t => t.TestType)
                .Include(t => t.Questions)
                .AsSplitQuery()
                .FirstOrDefaultAsync(t => t.Id == testId);

            if (test == null)
                return null;

            var questionIds = test.Questions.Select(q => q.Id).ToList();

            var allOptions = await db.Options
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync();

            var latestOptions = allOptions
                .GroupBy(o => o.QuestionId)
                .SelectMany(g =>
                {
                    var maxVersion = g.Max(o => o.Version);
                    return g.Where(o => o.Version == maxVersion);
                })
                .ToList();

            var optionsByQuestion = latestOptions
                .GroupBy(o => o.QuestionId)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var question in test.Questions)
            {
                if (optionsByQuestion.TryGetValue(question.Id, out var options))
                {
                    question.Options = options;
                }
                else
                {
                    question.Options = new List<Option>();
                }
            }

            return test;
        }

        public async Task AddAsync(Test test, int i)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Test
            {
                Name = test.Name,
                TopicId = test.TopicId,
                CuratorCreateId = CurrentUser.Id,
                PenaltyMax = i,
                DateOfCreating = DateTime.Now,
                TestTypeId = 1,
                TimeLimitSecond = test.TimeLimitSecond,
                Description = test.Description,
                IsArchive = false,
                IsRandom = test.IsRandom,
                MaxNumPassing = test.MaxNumPassing,
                Questions = test.Questions
            };

            await db.Tests.AddAsync(entity);
            await db.SaveChangesAsync();

            test.Id = entity.Id;
            _tests.Add(entity);
        }

        public async Task GetAllAsync(int? subjectId = null, int? teacherId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Tests
                .Include(t => t.Topic)
                .Include(t => t.CuratorCreate)
                .Include(t => t.TestType)
                .Include(t => t.Questions)
                .AsQueryable();

            if (subjectId != null)
                query = query.Where(t => t.TopicId == subjectId);
            if (teacherId != null)
                query = query.Where(t => t.CuratorCreateId == teacherId);

            var tests = await query.ToListAsync();

            foreach (var test in tests)
            {
                test.PenaltyMax = test.Questions?.Count ?? 0;
            }

            _tests = tests;
        }

        public async Task GetAllForParticipantsAsync(int? partId = null)
        {
            using var db = _factory.CreateDbContext();
            var tests = await db.Tests
                .Include(t => t.ParticipantsPublicTests)
                .Where(o => o.ParticipantsPublicTests.Any(p => p.ParticipantId == partId))
                .ToListAsync();

            _tests = tests;
        }

        public async Task RemoveAsync(Test test)
        {
            using var db = _factory.CreateDbContext();

            var attempts = await db.Attempts
                .Where(a => a.TestId == test.Id)
                .ToListAsync();

            var attemptId = attempts.Select(a => a.Id).ToList();

            var participantAnswersByAttempts = await db.ParticipantAnswers
                .Where(sa => attemptId.Contains(sa.AttemptId))
                .ToListAsync();

            db.ParticipantAnswers.RemoveRange(participantAnswersByAttempts);

            var questions = await db.Questions
                .Where(q => q.TestId == test.Id)
                .ToListAsync();

            var questionIds = questions.Select(q => q.Id).ToList();

            var participantAnswersByQuestions = await db.ParticipantAnswers
                .Where(sa => questionIds.Contains(sa.QuestionId))
                .ToListAsync();

            db.ParticipantAnswers.RemoveRange(participantAnswersByQuestions);

            var options = await db.Options
                .Where(o => questionIds.Contains(o.QuestionId))
                .ToListAsync();

            db.Options.RemoveRange(options);
            db.Questions.RemoveRange(questions);
            db.Attempts.RemoveRange(attempts);
            db.Tests.Remove(test);

            await db.SaveChangesAsync();

            _tests.Remove(test);
        }

        public async Task UpdateAsync(Test test)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Tests.FindAsync(test.Id);
            if (existing != null)
            {
                existing.Name = test.Name;
                existing.Description = test.Description;
                existing.TopicId = test.TopicId;
                existing.TestTypeId = test.TestTypeId;
                existing.PenaltyMax = test.PenaltyMax;
                existing.MaxNumPassing = test.MaxNumPassing;
                existing.TimeLimitSecond = test.TimeLimitSecond;
                existing.IsArchive = test.IsArchive;
                existing.IsRandom = test.IsRandom;
                await db.SaveChangesAsync();

                var index = _tests.FindIndex(t => t.Id == test.Id);
                if (index != -1)
                {
                    _tests[index] = existing;
                }
            }
        }

        public async Task GetTestsByTopicIdAsync(int subjectId, int? teacherId = null)
        {
            using var db = _factory.CreateDbContext();
            List<Test> tests;

            if (teacherId != null)
            {
                tests = await db.Tests
                    .Where(t => t.CuratorCreateId == teacherId && t.TopicId == subjectId)
                    .ToListAsync();
            }
            else
            {
                tests = await db.Tests
                    .Where(t => t.TopicId == subjectId)
                    .ToListAsync();
            }

            _tests = tests;
        }

        public async Task<Test> GetTestById(int? testId)
        {
            using var db = _factory.CreateDbContext();

            var tests = db.Tests
                .Include(t => t.Topic)
                .Include(t => t.CuratorCreate)
                .Include(t => t.TestType)
                .Include(t => t.Questions)
                    .ThenInclude(s => s.Options)
                .Include(t => t.Attempts)
                .AsSplitQuery()
                .FirstOrDefault(t => t.Id == testId);

            return tests;
        }

        public async Task<Test?> GetLastTestAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Tests
                .OrderByDescending(a => a.DateOfCreating)
                .FirstOrDefaultAsync();
        }

        public async Task GetTestsByTopicNameAsync(string subjectName)
        {
            using var db = _factory.CreateDbContext();
            var subject = await db.Topics
                .FirstOrDefaultAsync(s => s.Name == subjectName);

            if (subject != null)
            {
                await GetTestsByTopicIdAsync(subject.Id);
            }
            else
            {
                _tests.Clear();
            }
        }

        public async Task ArchiveTestAsync(int testId)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var test = await db.Tests.FirstOrDefaultAsync(ug => ug.Id == testId);

                if (test != null)
                {
                    test.IsArchive = !test.IsArchive;
                    await db.SaveChangesAsync();

                    var localTest = _tests.FirstOrDefault(t => t.Id == testId);
                    if (localTest != null)
                    {
                        localTest.IsArchive = test.IsArchive;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateQuestionCountAsync(int testId, int count)
        {
            using var db = _factory.CreateDbContext();
            var test = await db.Tests.FindAsync(testId);
            if (test != null)
            {
                test.MaxNumPassing = count;
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdatePenaltyMaxAsync(int testId, int penaltyMax)
        {
            using var db = _factory.CreateDbContext();
            var test = await db.Tests.FindAsync(testId);
            if (test != null)
            {
                test.PenaltyMax = penaltyMax;
                await db.SaveChangesAsync();
            }
        }
    }
}