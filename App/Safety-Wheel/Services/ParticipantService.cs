using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class ParticipantService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        public ObservableCollection<Participant> Participants { get; } = new();
        public ObservableCollection<ParticipantsPublicTest> ParticipantsPublicTests { get; } = new();

        public ParticipantService(IDbContextFactory<CozyTestContext> factory)
        {
            _factory = factory;
        }

        public async Task InitializeAsync()
        {
            if (CurrentUser.TypeUser == 1 || CurrentUser.TypeUser == 0)
                await GetAllParticipantsForAdminAsync();
            else
                await GetAllParticipantsAsync(CurrentUser.Id);
        }

        public async Task InitializeForAdminAsync()
        {
            await GetAllParticipantsForAdminAsync();
        }

        public async Task GetAllParticipantsAsync(int? teacherId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Participants
                .Include(s => s.Curators)
                .Include(s => s.Attempts)
                .AsQueryable();

            if (teacherId != null)
                query = query.Where(s => s.CuratorCreateId == teacherId);

            var participants = await query.ToListAsync();

            if (Application.Current?.Dispatcher != null && Application.Current.Dispatcher.CheckAccess())
            {
                Participants.Clear();
                foreach (var participant in participants)
                    Participants.Add(participant);
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Participants.Clear();
                    foreach (var participant in participants)
                        Participants.Add(participant);
                });
            }
        }

        public async Task GetAllParticipantsForAdminAsync()
        {
            using var db = _factory.CreateDbContext();
            var participants = await db.Participants
                .Include(s => s.Curators)
                .Include(s => s.Attempts)
                .ToListAsync();

            if (Application.Current?.Dispatcher != null && Application.Current.Dispatcher.CheckAccess())
            {
                Participants.Clear();
                foreach (var participant in participants)
                    Participants.Add(participant);
            }
            else
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Participants.Clear();
                    foreach (var participant in participants)
                        Participants.Add(participant);
                });
            }
        }

        public async Task AddAsync(Participant participant)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Participant
            {
                Name = participant.Name + "(Т)",
                Login = participant.Login,
                Password = participant.Password,
                CuratorCreateId = participant.CuratorCreateId,
                IsArchive = participant.IsArchive
            };
            await db.Participants.AddAsync(entity);
            await db.SaveChangesAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
                Participants.Add(entity));
        }

        public async Task<Participant> GetLastAsync()
        {
            using var db = _factory.CreateDbContext();
            return await db.Participants
                .OrderByDescending(a => a.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<ObservableCollection<Participant>> GetAllAsync(int? teacherId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = await db.Participants
                .AsNoTracking()
                .Include(s => s.Curators)
                .ToListAsync();

            Participants.Clear();
            foreach (var participant in query)
                Participants.Add(participant);

            return new ObservableCollection<Participant>(query);
        }

        public async Task<ObservableCollection<Participant>> GetAllActiveAsync(int? teacherId = null)
        {
            using var db = _factory.CreateDbContext();
            var query = await db.Participants
                .Include(s => s.Curators)
                .Where(p => p.IsArchive == false)
                .ToListAsync();

            return new ObservableCollection<Participant>(query);
        }

        public async Task<ObservableCollection<Participant>> GetAllBindAsync(int teacherId)
        {
            using var db = _factory.CreateDbContext();
            var query = await db.Participants
                .Include(s => s.Curators)
                .Where(ug => ug.Curators.Any(ug => ug.Id == teacherId))
                .ToListAsync();

            return new ObservableCollection<Participant>(query);
        }

        public async Task ReloadParticipantsAsync(int teacherId)
        {
            using var db = _factory.CreateDbContext();
            var stud = await db.Participants
                .Where(s => s.CuratorCreateId == teacherId)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                Participants.Clear();
                foreach (var participant in stud)
                    Participants.Add(participant);
            });
        }

        public async Task<Participant> GetByIdAsync(int? participantId = null)
        {
            if (!participantId.HasValue) return null;

            using var db = _factory.CreateDbContext();

            var part = db.Participants
                .Include(o => o.Curators)
                .FirstOrDefaultAsync(s => s.Id == participantId.Value);
            return await part;
        }

        public async Task<Participant> GetBindAccPartByIdCurAsync(int? curatorid = null)
        {
            if (!curatorid.HasValue) return null;

            using var db = _factory.CreateDbContext();

            var curator = await db.Curators
                .FirstOrDefaultAsync(s => s.Id == curatorid.Value);

            if (curator == null || !curator.ParticipantProfileId.HasValue)
                return null;

            return await db.Participants
                .FirstOrDefaultAsync(s => s.Id == curator.ParticipantProfileId.Value);
        }

        public async Task RemoveAsync(Participant participant)
        {
            using var db = _factory.CreateDbContext();
            db.Participants.Remove(participant);
            if (await db.SaveChangesAsync() > 0)
            {
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Participants.Contains(participant))
                        Participants.Remove(participant);
                });
            }
        }

        public async Task UpdateAsync(Participant participant)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Participants.FindAsync(participant.Id);
            if (existing != null)
            {
                existing.Name = participant.Name;
                existing.Login = participant.Login;
                existing.Password = participant.Password;
                existing.CuratorCreateId = participant.CuratorCreateId;
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateParticipantArchiveStatusAsync(int userId)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var user = await db.Participants
                    .FirstOrDefaultAsync(ug => ug.Id == userId);

                if (user != null)
                {
                    user.IsArchive = !user.IsArchive;
                    await db.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateParticipantBindForCuratorAsync(int userId, int curatorId, bool bind)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var participant = await db.Participants
                    .Include(p => p.Curators)
                    .FirstOrDefaultAsync(p => p.Id == userId);

                if (participant == null)
                {
                    MessageBox.Show("Участник не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var curator = await db.Curators
                    .FirstOrDefaultAsync(c => c.Id == curatorId);

                if (curator == null)
                {
                    MessageBox.Show("Куратор не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (bind)
                {
                    if (!participant.Curators.Any(c => c.Id == curator.Id))
                    {
                        participant.Curators.Add(curator);
                        await db.SaveChangesAsync();
                    }
                }
                else
                {
                    var existingCurator = participant.Curators
                        .FirstOrDefault(c => c.Id == curator.Id);

                    if (existingCurator != null)
                    {
                        participant.Curators.Remove(existingCurator);
                        await db.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task UpdateParticipantBindForGroupAsync(Participant participant, Group group, bool bind)
        {
            try
            {
                if (participant == null)
                {
                    MessageBox.Show("Участник не найден", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (group == null)
                {
                    MessageBox.Show("Группа не найдена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                using var db = _factory.CreateDbContext();

                var dbParticipant = await db.Participants
                    .Include(p => p.Groups)
                    .FirstOrDefaultAsync(p => p.Id == participant.Id);

                if (dbParticipant == null)
                {
                    MessageBox.Show("Участник не найден в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var dbGroup = await db.Groups
                    .FirstOrDefaultAsync(g => g.Id == group.Id);

                if (dbGroup == null)
                {
                    MessageBox.Show("Группа не найдена в базе данных", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (bind)
                {
                    if (!dbParticipant.Groups.Any(c => c.Id == dbGroup.Id))
                    {
                        dbParticipant.Groups.Add(dbGroup);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Участник уже привязан к этой группе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var existingGroup = dbParticipant.Groups
                        .FirstOrDefault(c => c.Id == dbGroup.Id);

                    if (existingGroup != null)
                    {
                        dbParticipant.Groups.Remove(existingGroup);
                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        MessageBox.Show("Участник не привязан к этой группе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public async Task<ObservableCollection<Participant>> GetAllParticipantForGroupAsync(int groupId)
        {
            using var db = _factory.CreateDbContext();
            var users = await db.Participants
                .AsNoTracking()
                .Include(ug => ug.Groups)
                .Where(u => u.Groups.Any(p => p.Id == groupId))
                .ToListAsync();

            var result = new ObservableCollection<Participant>();
            foreach (var userGroup in users)
                result.Add(userGroup);

            return result;
        }

        public async Task<ObservableCollection<Test>> GetAllPublicTestsForParticipantAsync(int participantId)
        {
            var testList = new ObservableCollection<Test>();

            using var db = _factory.CreateDbContext();

            var publicTests = await db.ParticipantsPublicTests
                .Include(ppt => ppt.Test)
                    .ThenInclude(t => t.Topic)
                .Include(ppt => ppt.Test)
                    .ThenInclude(t => t.TestType)
                .Include(ppt => ppt.Test)
                    .ThenInclude(t => t.CuratorCreate)
                .Where(ppt => ppt.ParticipantId == participantId)
                .Select(ppt => ppt.Test)
                .Where(t => t.IsArchive == false)
                .ToListAsync();

            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                testList.Clear();
                foreach (var test in publicTests)
                {
                    testList.Add(test);
                }
            });

            return testList;
        }
    }
}