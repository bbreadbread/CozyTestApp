using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Safety_Wheel.Services
{
    public class AttemptService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Attempt> Attempts { get; set; } = new();

        public AttemptService()
        {
            GetAll();
        }

        public void Add(Attempt attempt)
        {
            var _attempt = new Attempt
            {
                ParticipantId = attempt.ParticipantId,
                TestId = attempt.TestId,
                StartedAt = attempt.StartedAt,
                FinishedAt = attempt.FinishedAt,
                Score = attempt.Score,
                Status = attempt.Status
            };
            _db.Add(_attempt);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAll(decimal? participantId = null, decimal? testId = null, DateTime? date = null)
        {
            IQueryable<Attempt> query = _db.Attempts
                .Include(a => a.Participant)
                .Include(a => a.ParticipantAnswers);

            if (participantId != null)
                query = query.Where(a => a.ParticipantId == participantId);

            if (testId != null)
                query = query.Where(a => a.TestId == testId);

            if (date.HasValue)
                query = query.Where(a => a.StartedAt.Value.Date == date.Value.Date);

            var attempts = query.ToList();
            Attempts.Clear();

            foreach (var attempt in attempts)
            {
                Attempts.Add(attempt);
            }
        }
        public List<DateTime> GetUniqueAttemptDates(int participantId)
        {
            return _db.Attempts
                .Where(a => a.ParticipantId == participantId && a.StartedAt.HasValue)
                .Select(a => a.StartedAt.Value.Date)
                .Distinct()
                .OrderByDescending(d => d)
                .ToList();
        }

        public Attempt GetLastByType(int participantId, int testId, int typeId)
        {
            return Attempts
                      .Where(a => a.ParticipantId == participantId &&
                                  a.TestId == testId)
                      .OrderByDescending(a => a.StartedAt)  
                      .FirstOrDefault();                    
        }
        public void Remove(Attempt attempt)
        {
            if (attempt.Id == 0) return;

            _db.Remove(attempt);
            if (Commit() > 0)
                if (Attempts.Contains(attempt))
                    Attempts.Remove(attempt);
        }

        public void Update(Attempt attempt)
        {
            var existing = _db.Attempts.Find(attempt.Id);
            if (existing != null)
            {
                existing.ParticipantId = attempt.ParticipantId;
                existing.TestId = attempt.TestId;
                existing.StartedAt = attempt.StartedAt;
                existing.FinishedAt = attempt.FinishedAt;
                existing.Score = attempt.Score;
                existing.Status = attempt.Status;
                Commit();
            }
        }

        public List<Attempt> GetAttemptsByTest(int testId)
        {
                return Attempts
                    .Where(a => a.TestId == testId)
                    .OrderBy(a => a.StartedAt)
                    .ToList();
           
        }
    }
}
