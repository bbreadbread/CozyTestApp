using Microsoft.EntityFrameworkCore;
using Safety_Wheel.Models;
using System.Collections.ObjectModel;


namespace Safety_Wheel.Services
{
    public class BGroupsParticipantService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<BGroupsParticipant> BGroupsParticipant { get; set; } = new();
        public void GetAllGroupsForUser(int userId)
        {
            //var userGroups = _db.BGroupsParticipant
            //                   .Include(ug => ug.Group)
            //                   .Where(u => u.ParticipantId == userId)
            //                   .ToList();

            //BGroupsParticipant.Clear();

            //foreach (var userGroup in userGroups)
            //{
            //    BGroupsParticipant.Add(userGroup);
            //}
        }

    }
}
