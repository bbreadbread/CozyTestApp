using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using RegistrationCuratorCozyTest.Models;

namespace RegistrationCuratorCozyTest.Service
{
    public class CuratorService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Curator> Curators { get; set; } = new();

        public CuratorService()
        {
            GetAll();
        }

        public void Add(Curator teacher)
        {
            var _teacher = new Curator
            {
                Login = teacher.Login,
                Password = teacher.Password,
                Name = teacher.Name
            };
            _db.Add(_teacher);
            Curators.Add(_teacher);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll()
        {
            var teachers = _db.Curators
                .ToList();
            Curators.Clear();

            foreach (var teacher in teachers)
            {
                Curators.Add(teacher);
            }
        }

        public void Remove(Curator teacher)
        {
            var dbCurator = _db.Curators
                .FirstOrDefault(t => t.Id == teacher.Id);

            if (dbCurator == null)
                return;

            var tests = _db.Tests
                .Where(t => t.CuratorCreateId == dbCurator.Id)
                .ToList();

            foreach (var test in tests)
            {
                var attempts = _db.Attempts
                    .Where(a => a.TestId == test.Id)
                    .ToList();

                var attemptIds = attempts.Select(a => a.Id).ToList();

                var answersByAttempts = _db.ParticipantAnswers
                    .Where(sa => attemptIds.Contains(sa.AttemptId))
                    .ToList();

                _db.ParticipantAnswers.RemoveRange(answersByAttempts);

                var questions = _db.Questions
                    .Where(q => q.TestId == test.Id)
                    .ToList();

                var questionIds = questions.Select(q => q.Id).ToList();

                var answersByQuestions = _db.ParticipantAnswers
                    .Where(sa => questionIds.Contains(sa.QuestionId))
                    .ToList();

                _db.ParticipantAnswers.RemoveRange(answersByQuestions);

                var options = _db.Options
                    .Where(o => questionIds.Contains((int)o.QuestionId))
                    .ToList();

                _db.Options.RemoveRange(options);
                _db.Questions.RemoveRange(questions);
                _db.Attempts.RemoveRange(attempts);
                _db.Tests.Remove(test);
            }

            var participants = _db.Participants
                .Where(s => s.CuratorCreateId == dbCurator.Id)
                .ToList();

            _db.Participants.RemoveRange(participants);

            _db.Curators.Remove(dbCurator);

            _db.SaveChanges();

            Curators.Remove(teacher); 
        }


        public void Update(Curator teacher)
        {
            var existing = _db.Curators.Find(teacher.Id);
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
            return _db.Curators.Any(t => t.Login == login)
                || _db.Participants.Any(s => s.Login == login);
        }
    }
}
