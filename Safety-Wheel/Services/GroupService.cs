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
            GetAllGroupsForUser();
        }
        public void GetAllGroupsForUser()
        {
            var userGroups = _db.Groups
                               .Include(ug => ug.Participants)
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
        //нашли доступные группы
        public ObservableCollection<Group> GetAllGroupsForCurator(int curatorId)
        {
            var groups = _db.Groups
                              .Include(ug => ug.Curator)
                              .Where(u => u.CuratorId == curatorId)
                              .ToList();
            Group.Clear();

            foreach (var userGroup in groups)
            {
                Group.Add(userGroup);
            }
            return Group;
        }
    }
}
