using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class OptionService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Option> Options { get; } = new();

        public OptionService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Option option, int j)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Option
            {
                QuestionId = option.QuestionId,
                Number = j,
                TextAnswer = option.TextAnswer,
                IsCorrect = option.IsCorrect,
                IsImage = option.IsImage,
                Version = option.Version,
                Question = option.Question
            };
            await db.Options.AddAsync(entity);
            await db.SaveChangesAsync();

            option.Id = entity.Id;
        }

        public async Task<Option> AddReturnAsync(Option option, int j)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Option
            {
                QuestionId = option.QuestionId,
                Number = j,
                TextAnswer = option.TextAnswer,
                IsCorrect = option.IsCorrect,
                IsImage = option.IsImage,
                Version = option.Version,
                Question = option.Question
            };
            await db.Options.AddAsync(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        public async Task GetAllAsync(int? questionId = null)
        {
            using var db = _factory.CreateDbContext();
            List<Option> options;

            if (questionId != null)
            {
                options = await db.Options
                    .Include(o => o.Question)
                    .Include(o => o.ParticipantAnswers)
                    .Where(o => o.QuestionId == questionId)
                    .ToListAsync();
            }
            else
            {
                options = await db.Options
                    .Include(o => o.Question)
                    .Include(o => o.ParticipantAnswers)
                    .ToListAsync();
            }

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Options.Clear();
                foreach (var option in options)
                    Options.Add(option);
            });
        }

        public async Task RemoveAsync(Option option)
        {
            if (option == null) return;

            using var db = _factory.CreateDbContext();
            var answers = await db.ParticipantAnswers
                .Where(sa => sa.OptionId == option.Id)
                .ToListAsync();

            db.ParticipantAnswers.RemoveRange(answers);
            db.Options.Remove(option);

            if (await db.SaveChangesAsync() > 0)
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Options.Contains(option))
                        Options.Remove(option);
                });
        }

        public async Task UpdateAsync(Option option)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Options.FindAsync(option.Id);
            if (existing != null)
            {
                existing.QuestionId = option.QuestionId;
                existing.TextAnswer = option.TextAnswer;
                existing.IsCorrect = option.IsCorrect;
                existing.Version = option.Version;
                await db.SaveChangesAsync();
            }
        }

        public  List<Option> GetOptionsByQuestion(int questionId)
        {
            return Options
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.Number)
                .ToList();
        }

        public async Task<List<Option>> GetOptionsByQuestionAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();
            return await db.Options
                .Where(o => o.QuestionId == questionId)
                .OrderBy(o => o.Number)
                .ToListAsync();
        }

        public async Task<Option?> GetByTextAndQuestionAsync(int questionId, string textAnswer)
        {
            try
            {
                using var db = _factory.CreateDbContext();

                return await db.Options
                    .FirstOrDefaultAsync(o => o.QuestionId == questionId && o.TextAnswer == textAnswer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении варианта ответа: {ex.Message}");
                return null;
            }
        }

        public async Task<List<Option>> GetOptionsThatHaveAnswersAsync(int questionId)
        {
            using var db = _factory.CreateDbContext();

            return await db.ParticipantAnswers
                .Where(a => a.Option.QuestionId == questionId)
                .Select(a => a.Option)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<int>> GetVersionsByNumberAsync(int questionId, int optionNumber)
        {
            using var db = _factory.CreateDbContext();

            return await Task.Run(() =>
                 db.Options.Where(o => o.QuestionId == questionId && o.Number == optionNumber)
                       .Select(o => o.Version)
                       .ToList()
            );
        }
    }
}