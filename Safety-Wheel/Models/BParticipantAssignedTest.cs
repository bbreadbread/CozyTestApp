using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Models
{
    public partial class BParticipantAssignedTest : ObservableObject
    {
        private int _testId;
        private int _participantId;
        private int _curatorId;
        private Test _test = null!;
        private Participant _participant = null!;
        private Curator _curator = null!;
        public int TestId
        {
            get => _testId;
            set => SetProperty(ref _testId, value);
        }

        public int ParticipantId
        {
            get => _participantId;
            set => SetProperty(ref _participantId, value);
        }
        public int CuratorId
        {
            get => _curatorId;
            set => SetProperty(ref _curatorId, value);
        }
        public virtual Test Test
        {
            get => _test;
            set => SetProperty(ref _test, value);
        }

        public virtual Participant Participant
        {
            get => _participant;
            set => SetProperty(ref _participant, value);
        }
        public virtual Curator Curator
        {
            get => _curator;
            set => SetProperty(ref _curator, value);
        }

        public DateTime? DateTimeAssigned { get; set; }
    }
}
