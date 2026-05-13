using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class QuestionService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        private readonly CorrespondenceService _correspondenceService;
        public ObservableCollection<Question> Questions { get; } = new();

        public QuestionService(IDbContextFactory<CozyTestContext> factory, CorrespondenceService correspondenceService)
        {
            _factory = factory;
            _correspondenceService = correspondenceService;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task<Question> AddAsync(Question question, Test test, int number)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Question
            {
                TestId = test.Id,
                NumberActual = number,
                NumberCreate = number,
                TestQuest = question.TestQuest,
                PicturePath = question.PicturePath,
                Comments = question.Comments,
                QuestionType = question.QuestionType,
                QuestionTypeId = question.QuestionTypeId,
                Version = question.Version,
                IsRandom = question.IsRandom,
                TimeCreate = DateTime.Now,
            };
            await db.Questions.AddAsync(entity);
            await db.SaveChangesAsync();

            entity.Test = test;
            entity.Id = entity.Id; 

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Questions.Add(entity);
            });

            return entity;
        }

        public async Task GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            var questions = await db.Questions
                .Include(q => q.Test)
                .Include(q => q.Options)
                .Include(q => q.ParticipantAnswers)
                .ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Questions.Clear();
                foreach (var question in questions)
                    Questions.Add(question);
            });
        }

        public async Task RemoveAsync(Question question)
        {
            if (question == null) return;

            await DeleteQuestionAsync(question.Id);
            await GetAllAsync();
        }

        public async Task UpdateAsync(Question question)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Questions.FindAsync(question.Id);
            if (existing != null)
            {
                existing.TestId = question.TestId;
                existing.NumberCreate = existing.NumberCreate;
                existing.NumberActual = question.NumberActual;
                existing.TestQuest = question.TestQuest;
                existing.PicturePath = question.PicturePath;
                existing.Comments = question.Comments;
                existing.IsRandom = question.IsRandom;
                existing.IsArchive = question.IsArchive;
                existing.Version = question.Version;
                existing.TimeCreate = existing.TimeCreate;

                await db.SaveChangesAsync();
            }
        }

        public List<Question> GetQuestiosForCurrentTest(int currentTest)
        {
            using var db = _factory.CreateDbContext();

            return db.Questions
                .Where(q => q.TestId == currentTest)
                .OrderBy(q => q.NumberActual)
                .ToList();
        }

        public async Task<List<Question>> GetQuestiosForCurrentTestAsync(int currentTest)
        {
            using var db = _factory.CreateDbContext();

            return await db.Questions
                .Where(q => q.TestId == currentTest)
                .OrderBy(q => q.NumberActual)
                .ToListAsync();
        }

        public bool IsFirst(int currentQ)
        {
            using var db = _factory.CreateDbContext();

            var q = db.Questions.FirstOrDefault(q => q.NumberActual == currentQ);
            if (q != null) return true;
            return false;
        }

        public async Task DeleteQuestionAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();
            var question = await db.Questions
                .FirstOrDefaultAsync(q => q.Id == questionId);

            if (question == null) return;

            var participantAnswers = await db.ParticipantAnswers
                .Where(sa => sa.QuestionId == questionId)
                .ToListAsync();

            db.ParticipantAnswers.RemoveRange(participantAnswers);

            await _correspondenceService.RemoveByQuestionIdAsync(questionId);

            var options = await db.Options
                .Where(o => o.QuestionId == questionId)
                .ToListAsync();

            db.Options.RemoveRange(options);
            db.Questions.Remove(question);

            await db.SaveChangesAsync();
        }


    }
}