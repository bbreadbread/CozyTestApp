using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Services
{
    public class QuestionService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Question> Questions { get; set; } = new();

        public QuestionService()
        {
            GetAll();
        }

        public Question Add(Question question, Test test, int i)
        {
            var _question = new Question
            {
                TestId = test.Id,
                Number = i,
                TestQuest = question.TestQuest,
                PicturePath = question.PicturePath,
                Comments = question.Comments,
                QuestionType = question.QuestionType
            };
            _db.Add(_question);
            Commit();
            return _question;
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            IQueryable<Question> query = _db.Questions
                .Include(q => q.Test)
                .Include(q => q.Options)
                .Include(q => q.StudentAnswers);
            var questions = query.ToList();
            Questions.Clear();

            foreach (var question in questions)
            {
                Questions.Add(question);
            }
        }

        public void Remove(Question question)
        {
            if (question == null)
                return;

            DeleteQuestion(question.Id);

            Questions.Clear();
            GetAll();
        }

        public void Update(Question question)
        {
            var existing = _db.Questions.Find(question.Id);
            if (existing != null)
            {
                existing.TestId = question.TestId;
                existing.Number = question.Number;
                existing.TestQuest = question.TestQuest;
                existing.PicturePath = question.PicturePath;
                existing.Comments = question.Comments;
                Commit();
            }
        }

        public List<Question> GetQuestiosForCurrentTest(int currentTest)
        {
            return Questions
                  .Where(q => q.TestId == currentTest)
                  .OrderBy(q => q.Number)
                  .ToList();
        }

        public void DeleteQuestion(int questionId)
        {
            var question = _db.Questions
                .FirstOrDefault(q => q.Id == questionId);

            if (question == null)
                return;

            var studentAnswers = _db.StudentAnswers
                .Where(sa => sa.QuestionId == questionId)
                .ToList();

            _db.StudentAnswers.RemoveRange(studentAnswers);

            var options = _db.Options
                .Where(o => o.QuestionId == questionId)
                .ToList();

            _db.Options.RemoveRange(options);

            _db.Questions.Remove(question);

            _db.SaveChanges();
        }

    }
}
