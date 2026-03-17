using HarfBuzzSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Models
{
    public partial class BCuratorsParticipant : ObservableObject
    {
        private int _curatorId;
        private int _participantId;
        private Curator _curator = null!;
        private Participant _participant = null!;
        public int CuratorId
        {
            get => _curatorId;
            set => SetProperty(ref _curatorId, value);
        }

        public int ParticipantId
        {
            get => _participantId;
            set => SetProperty(ref _participantId, value);
        }
        public virtual Curator Curator
        {
            get => _curator;
            set => SetProperty(ref _curator, value);
        }

        public virtual Participant Participant
        {
            get => _participant;
            set => SetProperty(ref _participant, value);
        }
    }
}
