using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class ParticipantAnswerService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<ParticipantAnswer> ParticipantAnswers { get; } = new();

        public ParticipantAnswerService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(ParticipantAnswer participantAnswer)
        {
            using var db = _factory.CreateDbContext();

            var existingAnswer = await db.ParticipantAnswers
                .FirstOrDefaultAsync(a => a.AttemptId == participantAnswer.AttemptId &&
                                           a.QuestionId == participantAnswer.QuestionId &&
                                           a.OptionId == participantAnswer.OptionId &&
                                           a.ConstantOptionId == participantAnswer.ConstantOptionId);

            if (existingAnswer != null)
            {
                existingAnswer.OptionId = participantAnswer.OptionId;
                existingAnswer.IsCorrect = participantAnswer.IsCorrect;
                existingAnswer.AnsweredAt = participantAnswer.AnsweredAt;
                db.ParticipantAnswers.Update(existingAnswer);
            }
            else
            {
                var entity = new ParticipantAnswer
                {
                    AttemptId = participantAnswer.AttemptId,
                    QuestionId = participantAnswer.QuestionId,
                    OptionId = participantAnswer.OptionId,
                    ConstantOptionId = participantAnswer.ConstantOptionId,
                    IsCorrect = participantAnswer.IsCorrect,
                    TextAnswer = participantAnswer.TextAnswer,
                    AnsweredAt = participantAnswer.AnsweredAt,
                    Attempt = participantAnswer.Attempt,
                    Option = participantAnswer.Option,
                    Question = participantAnswer.Question
                };
                await db.ParticipantAnswers.AddAsync(entity);
            }
            
            await db.SaveChangesAsync();
        }

        public async Task<List<ParticipantAnswer>> GetAllAsync(decimal? attemptId = null, decimal? questionId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = db.ParticipantAnswers
                .Include(sa => sa.Attempt)
                .Include(sa => sa.Option)
                .Include(sa => sa.Question)
                .AsQueryable();

            if (attemptId != null)
                query = query.Where(sa => sa.AttemptId == attemptId);
            if (questionId != null)
                query = query.Where(sa => sa.QuestionId == questionId);

            var answers = await query.ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                ParticipantAnswers.Clear();
                foreach (var answer in answers)
                    ParticipantAnswers.Add(answer);
            });

            return answers;
        }


        public async Task RemoveByAttemptAndQuestionAsync(int attemptId, int questionId)
        {
            using var db = _factory.CreateDbContext();
            var answers = db.ParticipantAnswers
                .Where(a => a.AttemptId == attemptId && a.QuestionId == questionId);

            db.ParticipantAnswers.RemoveRange(answers);
            await db.SaveChangesAsync();
        }

        public async Task RemoveAsync(ParticipantAnswer participantAnswer)
        {
            using var db = _factory.CreateDbContext();
            db.ParticipantAnswers.Remove(participantAnswer);
            if (await db.SaveChangesAsync() > 0)
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (ParticipantAnswers.Contains(participantAnswer))
                        ParticipantAnswers.Remove(participantAnswer);
                });
        }

        public async Task UpdateAsync(ParticipantAnswer participantAnswer)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.ParticipantAnswers
                .FirstOrDefaultAsync(sa => sa.AttemptId == participantAnswer.AttemptId && sa.QuestionId == participantAnswer.QuestionId);
            if (existing != null)
            {
                existing.OptionId = participantAnswer.OptionId;
                existing.IsCorrect = participantAnswer.IsCorrect;
                existing.AnsweredAt = participantAnswer.AnsweredAt;
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> IsReadyAsync(Attempt attempt, Test test)
        {
            using var db = _factory.CreateDbContext();
            var testQuestions = await db.Questions
                .Where(q => q.TestId == test.Id)
                .ToListAsync();

            var participantAnswers = await db.ParticipantAnswers
                .Where(w => w.AttemptId == attempt.Id)
                .ToListAsync();

            foreach (var question in testQuestions)
            {
                var answersForQuestion = participantAnswers
                    .Where(a => a.QuestionId == question.Id)
                    .ToList();

                if (!answersForQuestion.Any())
                    return false;
            }
            return true;
        }

        public async Task<bool?> GetQuestionCorrectnessAsync(Attempt attempt, int questionId)
        {
            using var db = _factory.CreateDbContext();
            var participantOptions = await db.ParticipantAnswers
                .Where(sa => sa.AttemptId == attempt.Id && sa.QuestionId == questionId)
                .Select(sa => sa.OptionId)
                .ToListAsync();

            if (!participantOptions.Any())
                return null;

            var correctOptions = await db.Options
                .Where(o => o.QuestionId == questionId && o.IsCorrect == true)
                .Select(o => o.Id)
                .ToHashSetAsync();

            return correctOptions.SetEquals(participantOptions.ToHashSet());
        }

        public async Task<ParticipantAnswer> GetByQuestionAndAttemptAsync(int questionId, int attemptId)
        {
            using var db = _factory.CreateDbContext();
            return await db.ParticipantAnswers
                .Include(sa => sa.Attempt)
                .Include(sa => sa.Question)
                .Include(sa => sa.Option)
                .FirstOrDefaultAsync(sa => sa.QuestionId == questionId && sa.AttemptId == attemptId);
        }

        public async Task<Dictionary<int, bool?>> GetAllQuestionCorrectnessAsync(Attempt attempt, List<int> questionIds)
        {
            using var db = _factory.CreateDbContext();
            var result = new Dictionary<int, bool?>();

            var allParticipantAnswers = await db.ParticipantAnswers
                .Include(sa => sa.Option)
                .Where(sa => sa.AttemptId == attempt.Id && questionIds.Contains(sa.QuestionId))
                .ToListAsync();

            var groupedAnswers = allParticipantAnswers
                .GroupBy(sa => sa.QuestionId)
                .ToDictionary(g => g.Key, g => g.Select(sa => sa.OptionId).ToList());

            var allCorrectOptions = await db.Options
                .Where(o => questionIds.Contains(o.QuestionId) && o.IsCorrect == true)
                .ToListAsync();

            var groupedCorrect = allCorrectOptions
                .GroupBy(o => o.QuestionId)
                .ToDictionary(g => g.Key, g => g.Select(o => o.Id).ToHashSet());

            foreach (var questionId in questionIds)
            {
                if (!groupedAnswers.TryGetValue(questionId, out var participantOptions) || !participantOptions.Any())
                {
                    result[questionId] = null;
                    continue;
                }

                if (groupedCorrect.TryGetValue(questionId, out var correctOptions))
                    result[questionId] = correctOptions.SetEquals(participantOptions.ToHashSet());
                else
                    result[questionId] = false;
            }

            return result;
        }

        public async Task<List<Question>> GetQuestiosForCurrentTestAsync(int currentTest)
        {
            using var db = _factory.CreateDbContext();
            return await db.Questions
                .Include(q => q.Options)
                .Where(q => q.TestId == currentTest)
                .OrderBy(q => q.NumberActual)
                .ToListAsync();
        }

        public List<ParticipantAnswer> GetAnswersByQuestion(int questionId)
        {
            return ParticipantAnswers
                .Where(sa => sa.QuestionId == questionId)
                .ToList();
        }
    }
}