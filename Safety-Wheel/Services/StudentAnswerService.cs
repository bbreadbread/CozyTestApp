using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace CozyTest.Services
{
    public class ParticipantAnswerService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<ParticipantAnswer> ParticipantAnswers { get; set; } = new();

        public ParticipantAnswerService()
        {
            GetAll();
        }

        public void Add(ParticipantAnswer participantAnswer)
        {
            var _participantAnswer = new ParticipantAnswer
            {
                AttemptId = participantAnswer.AttemptId,
                QuestionId = participantAnswer.QuestionId,
                OptionId = participantAnswer.OptionId,
                IsCorrect = participantAnswer.IsCorrect,
                AnsweredAt = participantAnswer.AnsweredAt,
                Attempt = participantAnswer.Attempt,
                Option = participantAnswer.Option,
                Question = participantAnswer.Question
            };
            _db.Add(_participantAnswer);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll(decimal? attemptId = null, decimal? questionId = null)
        {
            IQueryable<ParticipantAnswer> query = _db.ParticipantAnswers
                .Include(sa => sa.Attempt)
                .Include(sa => sa.Option)
                .Include(sa => sa.Question);

            if (attemptId != null)
                query = query.Where(sa => sa.AttemptId == attemptId);
            if (questionId != null)
                query = query.Where(sa => sa.QuestionId == questionId);

            var answers = query.ToList();
            ParticipantAnswers.Clear();

            foreach (var answer in answers)
            {
                ParticipantAnswers.Add(answer);
            }
        }

        public void Remove(ParticipantAnswer participantAnswer)
        {
            _db.Remove(participantAnswer);
            if (Commit() > 0)
                if (ParticipantAnswers.Contains(participantAnswer))
                    ParticipantAnswers.Remove(participantAnswer);
        }

        public void Update(ParticipantAnswer participantAnswer)
        {
            var existing = _db.ParticipantAnswers
                .FirstOrDefault(sa => sa.AttemptId == participantAnswer.AttemptId && sa.QuestionId == participantAnswer.QuestionId);
            if (existing != null)
            {
                existing.OptionId = participantAnswer.OptionId;
                existing.IsCorrect = participantAnswer.IsCorrect;
                existing.AnsweredAt = participantAnswer.AnsweredAt;
                Commit();
            }
        }

        public bool IsReady(Attempt attempt, Test test)
        {
            var testQuestions = _db.Questions
                .Where(q => q.TestId == test.Id)
                .ToList();

            var participantAnswers = _db.ParticipantAnswers
                .Where(w => w.AttemptId == attempt.Id)
                .ToList();

            foreach (var question in testQuestions)
            {
                var answersForQuestion = participantAnswers
                    .Where(a => a.QuestionId == question.Id)
                    .ToList();

                if (!answersForQuestion.Any())
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool? GetQuestionCorrectness(Attempt attempt, int questionId)
        {
            var participantOptions = _db.ParticipantAnswers
                                    .Where(sa => sa.AttemptId == attempt.Id &&
                                                 sa.QuestionId == questionId)
                                    .Select(sa => sa.OptionId)
                                    .ToList();

            if (!participantOptions.Any())
                return null;

            var correctOptions = _db.Options
                                    .Where(o => o.QuestionId == questionId &&
                                                o.IsCorrect == true)
                                    .Select(o => o.Id)
                                    .ToHashSet();

            return correctOptions.SetEquals(participantOptions.ToHashSet());
        }


        public ParticipantAnswer GetByQuestionAndAttempt(int questionId, int attemptId)
        {
            return _db.ParticipantAnswers
                .Include(sa => sa.Attempt)
                .Include(sa => sa.Question)
                .Include(sa => sa.Option)
                .FirstOrDefault(sa => sa.QuestionId == questionId &&
                                     sa.AttemptId == attemptId);
        }

        public Dictionary<int, bool?> GetAllQuestionCorrectness(Attempt attempt, List<int> questionIds)
        {
            var result = new Dictionary<int, bool?>();

            var allParticipantAnswers = _db.ParticipantAnswers
                .Include(sa => sa.Option)
                .Where(sa => sa.AttemptId == attempt.Id && questionIds.Contains(sa.QuestionId))
                .ToList()
                .GroupBy(sa => sa.QuestionId)
                .ToDictionary(g => g.Key, g => g.Select(sa => sa.OptionId).ToList());

            var allCorrectOptions = _db.Options
                .Where(o => questionIds.Contains(o.QuestionId) && o.IsCorrect == true)
                .ToList()
                .GroupBy(o => o.QuestionId)
                .ToDictionary(g => g.Key, g => g.Select(o => o.Id).ToHashSet());

            foreach (var questionId in questionIds)
            {
                if (!allParticipantAnswers.TryGetValue(questionId, out var participantOptions) ||
                    !participantOptions.Any())
                {
                    result[questionId] = null;
                    continue;
                }

                if (allCorrectOptions.TryGetValue(questionId, out var correctOptions))
                {
                    result[questionId] = correctOptions.SetEquals(participantOptions.ToHashSet());
                }
                else
                {
                    result[questionId] = false;
                }
            }

            return result;

        }

        public List<Question> GetQuestiosForCurrentTest(int currentTest)
        {
            return _db.Questions
                  .Include(q => q.Options)
                  .Where(q => q.TestId == currentTest)
                  .OrderBy(q => q.Number)
                  .ToList();
        }

        public List<ParticipantAnswer> GetAnswersByQuestion(int questionId)
        {
                return ParticipantAnswers
                    .Where(sa => sa.QuestionId == questionId)
                    .ToList();
            
        }
    }
}
