using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public partial class Curator
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public bool? IsAdmin { get; set; }

    public bool? IsArchive { get; set; }

    public virtual ICollection<Requests> Requests { get; set; } = new List<Requests>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Participant> ParticipantsNavigation { get; set; } = new List<Participant>();
    public virtual ICollection<BParticipantAssignedTest> BParticipantAssignedTest { get; set; } = new List<BParticipantAssignedTest>();
    public virtual ICollection<BCuratorsParticipant> BCuratorsParticipant { get; set; } = new List<BCuratorsParticipant>();


    public virtual ICollection<Test> TestsNavigation { get; set; } = new List<Test>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
