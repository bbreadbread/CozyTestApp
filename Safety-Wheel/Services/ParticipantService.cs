using Microsoft.EntityFrameworkCore;
using CozyTest.Models;
using System.Collections.ObjectModel;
using System.Windows;
using ControlzEx.Standard;
using System.Windows.Controls;

namespace CozyTest.Services
{
    public class ParticipantService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Participant> Participants { get; set; } = new();
        public ObservableCollection<ParticipantsPublicTest> ParticipantsPublicTests;
        public ParticipantService()
        {
            if (CurrentUser.TypeUser == 1)
            GetAllParticipants();
            else GetAllParticipants(CurrentUser.Id);
        }
        public void GetAllParticipants(int? teacherId = null)
        {
            var query = _db.Participants
                .Include(s => s.Curators)
                .Include(s => s.Attempts).AsEnumerable();

            if (teacherId != null)
                query = query.Where(s => s.CuratorCreateId == teacherId);

            var participants = query.ToList();
            Participants.Clear();

            foreach (var participant in participants)
            {
                Participants.Add(participant);
            }
        }
        public void Add(Participant participant)
        {
            var _participant = new Participant
            {
                Name = participant.Name,
                Login = participant.Login,
                Password = participant.Password,
                CuratorCreateId = participant.CuratorCreateId,
                IsArchive = participant.IsArchive,
            };
            _db.Add(_participant);
            Participants.Add(_participant);
            Commit();
        }

        public int Commit() => _db.SaveChanges();

        public Participant GetLast()
        {
            var query = _db.Participants
                .OrderByDescending(a => a.Id)
                      .FirstOrDefault();

            return query;
        }
        public ObservableCollection<Participant> GetAll(int? teacherId = null)
        {
            var query = _db.Participants
                .Include(s => s.Curators)
                .ToList();

            return new ObservableCollection<Participant>(query.ToList());
        }
        public ObservableCollection<Participant> GetAllActive(int? teacherId = null)
        {
            var query = _db.Participants
                .Include(s => s.Curators)
                .Where(p => p.IsArchive == false)
                .ToList();

            return new ObservableCollection<Participant>(query.ToList());
        }
        public ObservableCollection<Participant> GetAllBind(int teacherId)
        {
            var query = _db.Participants
                                .Include(s => s.Curators)
                                .Where(ug => ug.Curators.Any(ug => ug.Id == teacherId))
                                .ToList();

            return new ObservableCollection<Participant>(query.ToList());
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

        public Participant GetById(int? participantId = null)
        {
            if (!participantId.HasValue) return null;

            return _db.Participants
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
        public void UpdateParticipantBindForCurator(int userId, int curatorId, bool bind)//bind- надо ли привязывать. true - надо привязать, false - надо отвязать
        {
            try
            {
                var participant = _db.Participants
                    .Include(p => p.Curators)
                    .FirstOrDefault(p => p.Id == userId);

                if (participant == null)
                {
                    MessageBox.Show("Участник не найден");
                    return;
                }

                var curator = _db.Curators
                    .FirstOrDefault(c => c.Id == curatorId);

                if (curator == null)
                {
                    MessageBox.Show("Куратор не найден");
                    return;
                }

                if (bind) 
                {
                    if (!participant.Curators.Any(c => c.Id == curator.Id))
                    {
                        participant.Curators.Add(curator);
                        _db.SaveChanges();
                    }
                }
                else 
                {
                    var existingCurator = participant.Curators
                        .FirstOrDefault(c => c.Id == curator.Id);

                    if (existingCurator != null)
                    {
                        participant.Curators.Remove(existingCurator);
                        _db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        
        public void UpdateParticipantBindForGroup(Participant participant, Group group, bool bind)//bind- надо ли привязывать. true - надо привязать, false - надо отвязать
        {
            try
            {

                if (participant == null)
                {
                    MessageBox.Show("Участник не найден");
                    return;
                }


                if (group == null)
                {
                    MessageBox.Show("Группа не найдена");
                    return;
                }

                if (bind) 
                {
                    if (!participant.Groups.Any(c => c.Id == group.Id))
                    {
                        participant.Groups.Add(group);
                        _db.SaveChanges();
                    }
                }
                else 
                {
                    var existingCurator = participant.Groups
                        .FirstOrDefault(c => c.Id == group.Id);

                    if (existingCurator != null)
                    {
                        participant.Groups.Remove(existingCurator);
                        _db.SaveChanges();
                    }
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
