using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class GroupService
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        private readonly ILoggingService _logger;

        public ObservableCollection<Group> Groups { get; } = new();

        public GroupService(IDbContextFactory<CozyTestContext> factory, ILoggingService logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public async Task InitializeAsync() => await GetAllGroupsForUserAsync();

        public async Task AddAsync(Group group)
        {
            using var db = _factory.CreateDbContext();
            var entity = new Group
            {
                Name = group.Name,
                Description = group.Description,
                CuratorId = group.CuratorId,
                IsPublic = false,
            };
            await db.Groups.AddAsync(entity);
            await db.SaveChangesAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                entity.Id = entity.Id;
                Groups.Add(entity);
            });

            await _logger.LogAsync(
                whoMade: CurrentUser.Name,
                whoRole: CurrentUser.ClassUser.ToString(),
                action: LogActionType.Create,
                objectType: LogObjectType.Group,
                objectName: entity.Name
            );
        }

        public async Task DeleteAsync(Group group)
        {
            using var db = _factory.CreateDbContext();
            db.Groups.Remove(group);
            if (await db.SaveChangesAsync() > 0)
                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    if (Groups.Contains(group))
                        Groups.Remove(group);
                });

            await _logger.LogAsync(
                    whoMade: CurrentUser.Name,
                    whoRole: CurrentUser.ClassUser.ToString(),
                    action: LogActionType.Delete,
                    objectType: LogObjectType.Group,
                    objectName: group.Name
                );
        }

        public async Task UpdateAsync(Group group)
        {
            using var db = _factory.CreateDbContext();
            var existing = await db.Groups.FindAsync(group.Id);
            if (existing != null)
            {
                existing.Name = group.Name;
                existing.Description = group.Description;
                existing.CuratorId = group.CuratorId;
                existing.IsPublic = group.IsPublic;
                await db.SaveChangesAsync();

                await App.Current.Dispatcher.InvokeAsync(() =>
                {
                    var localGroup = Groups.FirstOrDefault(g => g.Id == group.Id);
                    if (localGroup != null)
                    {
                        localGroup.Name = group.Name;
                        localGroup.Description = group.Description;
                        localGroup.CuratorId = group.CuratorId;
                        localGroup.IsPublic = group.IsPublic;
                    }
                });
            }

            await _logger.LogAsync(
                    whoMade: CurrentUser.Name,
                    whoRole: CurrentUser.ClassUser.ToString(),
                    action: LogActionType.Edit,
                    objectType: LogObjectType.Group,
                    objectName: existing.Name
                );
        }

        public async Task GetAllGroupsForUserAsync()
        {
            using var db = _factory.CreateDbContext();
            var userGroups = await db.Groups
                .Include(g => g.Curator)
                .ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Groups.Clear();
                foreach (var userGroup in userGroups)
                {
                    userGroup.CountPart = userGroup.Participants.Count;
                    Groups.Add(userGroup);
                }
            });
        }

        public async Task GetAllGroupsForUserAsync(int userId)
        {
            using var db = _factory.CreateDbContext();
            var groups = await db.Groups
                .Include(ug => ug.Participants)
                .Where(u => u.Participants.Any(p => p.Id == userId))
                .ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Groups.Clear();
                foreach (var userGroup in groups)
                {
                    userGroup.CountPart = userGroup.Participants.Count;
                    Groups.Add(userGroup);
                }
            });
        }

        public async Task GetAllGroupsForCuratorAsync(bool isAdmin, int userId)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Groups
                .Include(ug => ug.Curator)
                .Include(ug => ug.Participants)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(u => u.CuratorId == userId || u.IsPublic == true);

            var groups = await query.ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Groups.Clear();
                foreach (var userGroup in groups)
                {
                    userGroup.CountPart = userGroup.Participants.Count;
                    Groups.Add(userGroup);
                }
            });
        }

        public async Task<ObservableCollection<Group>> GetAllGroupsForCuratorAsync(bool isAdmin, int curatorId, int testId)
        {
            using var db = _factory.CreateDbContext();
            var query = db.Groups
                .Include(ug => ug.Curator)
                .Include(ug => ug.Participants)
                .AsQueryable();

            if (!isAdmin)
                query = query.Where(u => u.CuratorId == curatorId || u.IsPublic == true);

            var groups = await query.ToListAsync();

            await App.Current.Dispatcher.InvokeAsync(async () =>
            {
                Groups.Clear();
                foreach (var userGroup in groups)
                {
                    userGroup.CountPart = userGroup.Participants.Count;
                    userGroup.IsPublished = await IsTestPublishedForAllParticipants(userGroup.Id, testId);
                    Groups.Add(userGroup);
                }
            });

            return Groups;
        }

        public async Task PublicGroupAsync(int groupId)
        {
            try
            {
                using var db = _factory.CreateDbContext();
                var test = await db.Groups.FirstOrDefaultAsync(ug => ug.Id == groupId);

                if (test != null)
                {
                    test.IsPublic = !test.IsPublic;
                    await db.SaveChangesAsync();

                    var localTest = Groups.FirstOrDefault(t => t.Id == groupId);
                    if (localTest != null)
                    {
                        localTest.IsPublic = test.IsPublic;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async Task<bool> IsTestPublishedForAllParticipants(int groupId, int testId)
        {
            using var db = _factory.CreateDbContext();
            var participants = await GetAllParticipantForGroup(groupId);

            if (!participants.Any())
                return false;

            foreach (var participant in participants)
            {
                var isPublished = db.ParticipantsPublicTests
                    .Any(ppt => ppt.ParticipantId == participant.Id && ppt.TestId == testId);

                if (!isPublished)
                    return false;
            }
            return true;
        }

        public async Task<List<Participant>> GetAllParticipantForGroup(int groupId)
        {
            using var db = _factory.CreateDbContext();
            return db.Participants
                .Include(p => p.Groups)
                .Where(u => u.Groups.Any(p => p.Id == groupId))
                .ToList();
        }
    }
}