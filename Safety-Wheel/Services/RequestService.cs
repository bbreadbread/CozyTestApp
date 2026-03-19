using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class RequestService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Requests> Requests { get; set; } = new();
    
        public RequestService()
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
            Requests.Clear();
            foreach (var apl in ap)
            {
                Requests.Add(apl);
            }
        }
    }
}
