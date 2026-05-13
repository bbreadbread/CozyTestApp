namespace RegistrationCuratorCozyTest.Models
{
    public partial class ParticipantsPublicTest: ObservableObject
    {
        public int ParticipantId { get; set; }

        public int TestId { get; set; }

        public int ResponsibleId { get; set; }

        public virtual Participant Participant { get; set; } = null!;

        public virtual Curator Responsible { get; set; } = null!;

        public virtual Test Test { get; set; } = null!;
    }
}
