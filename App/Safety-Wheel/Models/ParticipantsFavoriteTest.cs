namespace CozyTest.Models
{
    public class ParticipantsFavoriteTest
    {
        public int ParticipantId { get; set; }

        public int TestId { get; set; }

        public virtual Participant Participant { get; set; } = null!;

        public virtual Test Test { get; set; } = null!;
    }
}
