using CozyTest.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CozyTest.Services
{
    public class GroupService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Group> Group { get; set; } = new();
        public GroupService()
        {

        }
        public void GetAllGroupsForUser()
        {
            //var userGroups = _db.Groups
            //                   .Include(ug => ug.Participants)
            //                   .Where(u => u.Participants.Any(p => p.Id == CurrentUser.Id))
            //                   .ToList();

            //Group.Clear();

            //foreach (var userGroup in userGroups)
            //{
            //    Group.Add(userGroup);
            //}
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
