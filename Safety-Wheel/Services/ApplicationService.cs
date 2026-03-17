using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Services
{
    public class ApplicationService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Requests> Applications { get; set; } = new();
    
        public ApplicationService()
        {
            GetAll();
        }

        public int Commit() => _db.SaveChanges();

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
        public void GetAll()
        {
            var query = _db.Requests;
            var ap = query.ToList();
            Applications.Clear();
            foreach (var apl in ap)
            {
                Applications.Add(apl);
            }
        }
    }
}
