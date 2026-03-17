using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Safety_Wheel.Models
{
    public partial class BParticipantFavoriteTest : ObservableObject
    {
        private int _testId;
        private int _participantId;
        private Test _test = null!;
        private Participant _participant = null!;
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
    }
}
