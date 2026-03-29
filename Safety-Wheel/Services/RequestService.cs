using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

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

        

        public void Add(Requests request)
        {
            var _req = new Requests
            {
                Name = request.Name,
                Login = request.Login,
                Password = request.Password,
                Status = request.Status,
                DateTimeApplication = DateTime.Now,
                Reviewer = null,
                ReviewerId = null,
            };
            _db.Add(_req);
            Commit();
        }
        public void GetAll()
        {
            var query = _db.Requests.ToList();
            Requests.Clear();
            foreach (var apl in query)
            {
                Requests.Add(apl);
            }
        }


        public void Update(Requests oldRequest)
        {
            var existing = _db.Requests.Find(oldRequest.Id);
            if (existing != null)
            {
                existing.Status = oldRequest.Status;
                existing.ReviewerId = oldRequest.ReviewerId;
                Commit();
            }


        }
    }
}
