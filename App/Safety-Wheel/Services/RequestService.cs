using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Internal;

namespace CozyTest.Services
{
    public class RequestService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Requests> Requests { get; } = new();

        public RequestService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task GetAllAsync()
        {
            using var db = _factory.CreateDbContext();
            var list = await db.Requests.AsNoTracking().ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Requests.Clear();
                foreach (var item in list)
                    Requests.Add(item);
            });
        }
        public async Task<bool> CheckLoginExistsAsync(string login)
        {
            using var context = _factory.CreateDbContext();

            bool existsInRequests = await context.Requests
                .AnyAsync(r => r.Login == login && r.Status != "Отклонена");

            bool existsInParticipants = await context.Participants
                .AnyAsync(p => p.Login == login);

            bool existsInCurators = await context.Curators
                .AnyAsync(c => c.Login == login);

            return existsInRequests || existsInParticipants || existsInCurators;
        }

        public async Task AddAsync(Requests request)
        {
            using var context = _factory.CreateDbContext();

            context.Requests.Add(request);
            await context.SaveChangesAsync();

            Requests.Add(request);
        }

        //public void Add(Requests request)
        //{
        //    using var db = _factory.CreateDbContext();

        //    var entity = new Requests
        //    {
        //        Name = request.Name,
        //        Login = request.Login,
        //        Password = request.Password,
        //        Status = request.Status,
        //        DateTimeApplication = DateTime.Now,
        //        Reviewer = null,
        //        ReviewerId = null
        //    };

        //    db.Requests.Add(entity);
        //    db.SaveChanges();
        //}

        public async Task UpdateAsync(Requests request)
        {
            using var db = _factory.CreateDbContext();

            var existing = await db.Requests.FindAsync(request.Id);
            if (existing != null)
            {
                existing.Status = request.Status;
                existing.ReviewerId = request.ReviewerId;
                await db.SaveChangesAsync();
            }
        }

        public List<Requests> GetAllActive()
        {
            return Requests.Where(p => p.Status == "Ожидает подтверждения").ToList();
        }
    }
}