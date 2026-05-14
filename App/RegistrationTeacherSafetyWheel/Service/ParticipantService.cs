using RegistrationCuratorCozyTest.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistrationCuratorCozyTest.Service
{
    internal class ParticipantService
    {
        private readonly CozyTestContext _db = BaseDbService.Instance.Context;
        public ObservableCollection<Participant> Participants { get; set; } = new();

        public ParticipantService()
        {
        }
        public int Commit() => _db.SaveChanges();
        public void Add(Participant part)
        {
            var _participant = new Participant
            {
                Login = part.Login,
                Password = part.Password,
                Name = part.Name + "(Т)",
                IsArchive = false,
            };
            _db.Add(_participant);
            Participants.Add(_participant);
            Commit();
        }

        public Participant GetLast() => _db.Participants
                .OrderByDescending(a => a.Id)
                .FirstOrDefault();
    }
}
