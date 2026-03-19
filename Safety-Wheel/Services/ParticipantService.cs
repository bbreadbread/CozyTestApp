using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.Services
{
    public class ParticipantService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Participant> Participants { get; set; } = new();
        public ObservableCollection<ParticipantsPublicTest> ParticipantsPublicTests;

        public ParticipantService()
        {
            GetAllParticipants(CurrentUser.Id);
        }

        public void Add(Participant participant)
        {
            var _participant = new Participant
            {
                Name = participant.Name,
                Login = participant.Login,
                Password = participant.Password,
                CuratorCreateId = participant.CuratorCreateId
            };
            _db.Add(_participant);
            Participants.Add(_participant);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public void GetAllParticipants(int? teacherId = null)
        {
            IQueryable<Participant> query = _db.Participants
                .Include(s => s.Curators)
                .Include(s => s.Attempts);

            if (teacherId != null)
                query = query.Where(s => s.CuratorCreateId == teacherId);

            var participants = query.ToList();
            Participants.Clear();

            foreach (var participant in participants)
            {
                Participants.Add(participant);
            }
        }

        public void ReloadParticipants(int teacherId)
        {
            var stud  = _db.Participants
                         .Where(s => s.CuratorCreateId == teacherId)
                         .ToList();
            Participants.Clear();
            foreach (var participant in stud)
            {
                Participants.Add(participant);
            }
        }

        public Participant? GetCurrentParticipant(int? participantId = null)
        {
            if (!participantId.HasValue) return null;

            return Participants
                      .FirstOrDefault(s => s.Id == participantId.Value);
        }

        public void Remove(Participant participant)
        {
                _db.Remove(participant);
                if (Commit() > 0)
                    if (Participants.Contains(participant))
                        Participants.Remove(participant);
        }

        public void Update(Participant participant)
        {
            var existing = _db.Participants.Find(participant.Id);
            if (existing != null)
            {
                existing.Name = participant.Name;
                existing.Login = participant.Login;
                existing.Password = participant.Password;
                existing.CuratorCreateId = participant.CuratorCreateId;
                Commit();
            }
        }

        public void UpdateParticipantArchiveStatus(int userId)
        {
            try
            {
                var user = _db.Participants
                    .FirstOrDefault(ug => ug.Id == userId);

                if (user != null)
                {
                    user.IsArchive = !user.IsArchive;
                    _db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public ObservableCollection<Participant> GetAllParticipantForGroup(int groupId)
        {
            var users = _db.Participants
                              .Include(ug => ug.Groups)
                              .Where(u => u.Groups.Any(p => p.Id == groupId))
                              .ToList();

            Participants.Clear();

            foreach (var userGroup in users)
            {
                Participants.Add(userGroup);
            }
            return Participants;
        }

    }
}
