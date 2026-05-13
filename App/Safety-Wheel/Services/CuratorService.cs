using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class CuratorService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Curator> Curators { get; } = new();

        public CuratorService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync() => await GetAllAsync();

        public async Task AddAsync(Curator cur)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Curator
            {
                Login = cur.Login,
                Password = cur.Password,
                Name = cur.Name,
                IsAdmin = cur.IsAdmin,
                IsArchive = cur.IsArchive,
                ParticipantProfileId = cur.ParticipantProfileId,                               
            };
            await db.Curators.AddAsync(entity);
            await db.SaveChangesAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Curators.Add(entity);
            });
        }

        public async Task GetAllAsync()
        {
            if (Application.Current?.Dispatcher == null) return;
            using var db = _factory.CreateDbContext();
            var teachers = await db.Curators
                .Include(t => t.Participants)
                .Include(t => t.Tests)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Curators.Clear();
                foreach (var teacher in teachers)
                    Curators.Add(teacher);
            });
        }

        public async Task GetAllByUserAsync(int partId)
        {
            using var db = _factory.CreateDbContext();
            var teachers = await db.Curators
                .Include(t => t.Participants)
                .Where(o => o.Participants.Any(p => p.Id == partId))
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Curators.Clear();
                foreach (var teacher in teachers)
                    Curators.Add(teacher);
            });
        }

        public async Task<Curator> GetLastAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Curators
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Curator> GetById(int id)
        {
            using var db = _factory.CreateDbContext();
            return await db.Curators.FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task RemoveAsync(Curator teacher)
        {
            using var db = _factory.CreateDbContext();
            db.Curators.Remove(teacher);
            if (await db.SaveChangesAsync() > 0)
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Curators.Contains(teacher))
                        Curators.Remove(teacher);
                });
        }

        public async Task UpdateAsync(Curator teacher)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Curators.FindAsync(teacher.Id);
            if (existing != null)
            {
                existing.Login = teacher.Login;
                existing.Password = teacher.Password;
                existing.Name = teacher.Name;
                await db.SaveChangesAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    var localTeacher = Curators.FirstOrDefault(t => t.Id == teacher.Id);
                    if (localTeacher != null)
                    {
                        localTeacher.Login = teacher.Login;
                        localTeacher.Password = teacher.Password;
                        localTeacher.Name = teacher.Name;
                    }
                });
            }
        }

        public bool UserExistsByLogin(string login)
        {
            using var db = _factory.CreateDbContext();
            return db.Curators.Any(t => t.Login == login)
                || db.Participants.Any(s => s.Login == login);
        }

        public async Task UpdateCuratorArchiveStatusAsync(int curatorId)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var user = await db.Curators.FirstOrDefaultAsync(ug => ug.Id == curatorId);

                if (user != null)
                {
                    bool newArchiveStatus = user.IsArchive == true ? false : true;
                    user.IsArchive = newArchiveStatus;
                    await db.SaveChangesAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var localCurator = Curators.FirstOrDefault(c => c.Id == curatorId);
                        if (localCurator != null)
                        {
                            localCurator.IsArchive = newArchiveStatus;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateCuratorAdminStatusAsync(int curatorId)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var user = await db.Curators.FirstOrDefaultAsync(ug => ug.Id == curatorId);

                if (user != null)
                {
                    bool newAdminStatus = user.IsAdmin == true ? false : true;
                    user.IsAdmin = newAdminStatus;
                    await db.SaveChangesAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var localCurator = Curators.FirstOrDefault(c => c.Id == curatorId);
                        if (localCurator != null)
                        {
                            localCurator.IsAdmin = newAdminStatus;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateCuratorAsync(Curator updatedCurator)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var existing = await db.Curators.FindAsync(updatedCurator.Id);
                if (existing != null)
                {
                    existing.Login = updatedCurator.Login;
                    existing.Password = updatedCurator.Password;
                    existing.Name = updatedCurator.Name;
                    existing.IsAdmin = updatedCurator.IsAdmin;
                    existing.IsArchive = updatedCurator.IsArchive;
                    await db.SaveChangesAsync();

                    await Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        var localCurator = Curators.FirstOrDefault(c => c.Id == updatedCurator.Id);
                        if (localCurator != null)
                        {
                            localCurator.Login = updatedCurator.Login;
                            localCurator.Password = updatedCurator.Password;
                            localCurator.Name = updatedCurator.Name;
                            localCurator.IsAdmin = updatedCurator.IsAdmin;
                            localCurator.IsArchive = updatedCurator.IsArchive;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления куратора: {ex.Message}");
            }
        }

        public async Task<List<Curator>> GetCuratorsForParticipantAsync(int participantId)
        {
            using var context = _factory.CreateDbContext();

            var curators = await context.Curators
                .Where(c => c.Participants.Any(p => p.Id == participantId))
                .ToListAsync();

            return curators;
        }
    }
}