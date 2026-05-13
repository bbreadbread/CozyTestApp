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
    public class TestService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Test> Tests { get; set; } = new();

        public TestService(bool? empty = null)
        {
            if (empty != true) GetAll();
        }

        public void Add(Test test, int i)
        {
            var _test = new Test
            {
                Name = test.Name,
                SubjectId = test.SubjectId,
                TeacherId = CurrentUser.Id,
                PenaltyMax = i,
                MaxScore = i,
                DateOfCreating = DateTime.Now,
            };
            _db.Add(_test);
            Tests.Add(_test);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll(int? subjectId = null, int? teacherId = null)
        {
            IQueryable<Test> query = _db.Tests
                .Include(t => t.Subject)
                .Include(t => t.Teacher)
                .Include(t => t.Questions);

            if (subjectId != null)
                query = query.Where(t => t.SubjectId == subjectId);
            if (teacherId != null)
                query = query.Where(t => t.TeacherId == teacherId);

            var tests = query.ToList();
            Tests.Clear();

            foreach (var test in tests)
            {
                Tests.Add(test);
            }
        }

        public void Remove(Test test)
        {
            if (Tests.Contains(test))
            {
                var attempts = _db.Attempts
                    .Where(a => a.TestId == test.Id)
                    .ToList();

                var attemptId = attempts
                    .Select(a => a.Id)
                    .ToList();

                var studentAnswersByAttempts = _db.StudentAnswers
                    .Where(sa => attemptId.Contains(sa.AttemptId))
                    .ToList();

                _db.StudentAnswers.RemoveRange(studentAnswersByAttempts);

                var questions = _db.Questions
                    .Where(q => q.TestId == test.Id)
                    .ToList();

                var questionIds = questions
                    .Select(q => q.Id)
                    .ToList();

                var studentAnswersByQuestions = _db.StudentAnswers
                    .Where(sa => questionIds.Contains(sa.QuestionId))
                    .ToList();

                _db.StudentAnswers.RemoveRange(studentAnswersByQuestions);

                var options = _db.Options
                    .Where(o => questionIds.Contains(o.QuestionId))
                    .ToList();

                _db.Options.RemoveRange(options);
                _db.Questions.RemoveRange(questions);
                _db.Attempts.RemoveRange(attempts);
                _db.Remove(test);
                _db.Tests.Remove(test);
                _db.SaveChanges();
            }
        }

        public void Update(Test test)
        {
            var existing = _db.Tests.Find(test.Id);
            if (existing != null)
            {
                existing.Name = test.Name;
                existing.SubjectId = test.SubjectId;
                existing.TeacherId = test.TeacherId;
                existing.PenaltyMax = test.PenaltyMax;
                Commit();
            }
        }

        public void GetTestsBySubjectId(int subjectId, int? teacherId = null)
        {
            List<Test> tests;
            if (teacherId != null)
            {
                tests = Tests.Where(t => t.TeacherId == teacherId && t.SubjectId == subjectId).ToList();
            }
            else 
            tests = Tests
                .Where(t => t.SubjectId == subjectId)
                .ToList();

            Tests.Clear();

            foreach (var test in tests)
            {
                Tests.Add(test);
            }
        }

        public Test GetTestById(int? testId)
        {
            return Tests
                .Where(t => t.Id == testId)
                .First();
        }

        public Test GetLastTest()
        {
            return Tests
                 .OrderByDescending(a => a.DateOfCreating)
                      .FirstOrDefault();
        }

        public void GetTestsBySubjectName(string subjectName)
        {
            var subject = _db.Subjects
                .FirstOrDefault(s => s.Name.ToString() == subjectName);

            if (subject != null)
            {
                GetTestsBySubjectId(subject.Id);
            }
            else
            {
                Tests.Clear();
            }
        }


    }
}
