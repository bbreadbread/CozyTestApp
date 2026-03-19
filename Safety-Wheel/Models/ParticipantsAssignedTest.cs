using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CozyTest.Models
{
    public partial class ParticipantsAssignedTest : ObservableObject
    {
        public int ParticipantId { get; set; }

        public int TestId { get; set; }

        public DateTime? DateTimeAssigned { get; set; }

        public virtual Participant Participant { get; set; } = null!;

        public virtual Test Test { get; set; } = null!;
    }
}

