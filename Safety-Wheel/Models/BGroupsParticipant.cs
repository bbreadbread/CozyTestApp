using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Models
{
    public partial class BGroupsParticipant : ObservableObject
    {
        private int _groupId;
        private int _participantId;
        private Group _group = null!;
        private Participant _participant = null!;
        public int GroupId
        {
            get => _groupId;
            set => SetProperty(ref _groupId, value);
        }

        public int ParticipantId
        {
            get => _participantId;
            set => SetProperty(ref _participantId, value);
        }
        public virtual Group Group
        {
            get => _group;
            set => SetProperty(ref _group, value);
        }

        public virtual Participant Participant
        {
            get => _participant;
            set => SetProperty(ref _participant, value);
        }
    }
}
