using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace CozyTest.Services
{
    public class GroupService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Group> Group { get; set; } = new();
        public GroupService()
        {

        }

        public void Add(Group group)
        {
            var _group = new Group
            {
                Name = group.Name,
                Description = group.Description,
                CuratorId = group.CuratorId,
            };
            _db.Add(_group);
            Commit();
            Group.Add(group);
        }

        public void Delete(Group group)
        {
            _db.Remove(group);
            if (Commit() > 0)
                if (Group.Contains(group))
                    Group.Remove(group);
        }

        public void Update(Group group)
        {
            var existing = _db.Groups.Find(group.Id);
            if (existing != null)
            {
                existing.Name = group.Name;
                existing.Description = group.Description;
                existing.CuratorId = group.CuratorId;
                Commit();
            }
        }

        public int Commit() => _db.SaveChanges();

        public void GetAllGroupsForUser()
        {
            var userGroups = _db.Groups
                               .ToList();

            Group.Clear();

            foreach (var userGroup in userGroups)
            {
                Group.Add(userGroup);
            }
        }
        public void GetAllGroupsForUser(int userId)
        {
            var groups = _db.Groups
                               .Include(ug => ug.Participants)
                               .Where(u => u.Participants.Any(p => p.Id == userId))
                               .ToList();

            Group.Clear();

            foreach (var userGroup in groups)
            {
                Group.Add(userGroup);
            }
        }
        public void GetAllGroupsForCurator(int userId)
        {
            var groups = _db.Groups
                               .Where(p => p.CuratorId == userId)
                               .ToList();

            Group.Clear();

            foreach (var userGroup in groups)
            {
                Group.Add(userGroup);
            }
        }
        //нашли доступные группы
        public ObservableCollection<Group> GetAllGroupsForCurator(int curatorId, int testId)
        {
            var groups = _db.Groups
                              .Include(ug => ug.Curator)
                              .Include(ug => ug.Participants)
                              .Where(u => u.CuratorId == curatorId)
                              .ToList();
            Group.Clear();

            foreach (var userGroup in groups)
            {
                userGroup.IsPublished = IsTestPublishedForAllParticipants(userGroup.Id, testId);
                Group.Add(userGroup);
            }
            return Group;
        }

        private bool IsTestPublishedForAllParticipants(int groupId, int testId)
        {
            var participants = GetAllParticipantForGroup(groupId);

            if (!participants.Any())
                return false;

            foreach (var participant in participants)
            {
                var isPublished = _db.ParticipantsPublicTests
                                    .Any(ppt => ppt.ParticipantId == participant.Id && ppt.TestId == testId);

                if (!isPublished)
                    return false;
            }
            return true;
        }

        public List<Participant> GetAllParticipantForGroup(int groupId)
        {
            var part = _db.Participants
                .Include(p => p.Groups)
                .Where(u => u.Groups.Any(p => p.Id == groupId))
                .ToList();

            return part;
        }
    }
}
