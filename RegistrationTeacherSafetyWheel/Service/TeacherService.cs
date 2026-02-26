using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RegistrationTeacherSafetyWheel.Models;

namespace RegistrationTeacherSafetyWheel.Service
{
    public class TeacherService
    {
        private readonly SafetyWheelContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Teacher> Teachers { get; set; } = new();

        public TeacherService()
        {
            GetAll();
        }

        public void Add(Teacher teacher)
        {
            var _teacher = new Teacher
            {
                Login = teacher.Login,
                Password = teacher.Password,
                Name = teacher.Name
            };
            _db.Add(_teacher);
            Teachers.Add(_teacher);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var teachers = _db.Teachers
                .ToList();
            Teachers.Clear();

            foreach (var teacher in teachers)
            {
                Teachers.Add(teacher);
            }
        }

        public void Remove(Teacher teacher)
        {
            var dbTeacher = _db.Teachers
                .FirstOrDefault(t => t.Id == teacher.Id);

            if (dbTeacher == null)
                return;

            var tests = _db.Tests
                .Where(t => t.TeacherId == dbTeacher.Id)
                .ToList();

            foreach (var test in tests)
            {
                var attempts = _db.Attempts
                    .Where(a => a.TestId == test.Id)
                    .ToList();

                var attemptIds = attempts.Select(a => a.Id).ToList();

                var answersByAttempts = _db.StudentAnswers
                    .Where(sa => attemptIds.Contains(sa.AttemptId))
                    .ToList();

                _db.StudentAnswers.RemoveRange(answersByAttempts);

                var questions = _db.Questions
                    .Where(q => q.TestId == test.Id)
                    .ToList();

                var questionIds = questions.Select(q => q.Id).ToList();

                var answersByQuestions = _db.StudentAnswers
                    .Where(sa => questionIds.Contains(sa.QuestionId))
                    .ToList();

                _db.StudentAnswers.RemoveRange(answersByQuestions);

                var options = _db.Options
                    .Where(o => questionIds.Contains((int)o.QuestionId))
                    .ToList();

                _db.Options.RemoveRange(options);
                _db.Questions.RemoveRange(questions);
                _db.Attempts.RemoveRange(attempts);
                _db.Tests.Remove(test);
            }

            var students = _db.Students
                .Where(s => s.TeachersId == dbTeacher.Id)
                .ToList();

            _db.Students.RemoveRange(students);

            _db.Teachers.Remove(dbTeacher);

            _db.SaveChanges();

            Teachers.Remove(teacher); 
        }


        public void Update(Teacher teacher)
        {
            var existing = _db.Teachers.Find(teacher.Id);
            if (existing != null)
            {
                existing.Login = teacher.Login;
                existing.Password = teacher.Password;
                existing.Name = teacher.Name;
                Commit();
            }
        }

        public bool UserExistsByLogin(string login)
        {
            return _db.Teachers.Any(t => t.Login == login)
                || _db.Students.Any(s => s.Login == login);
        }
    }
}
