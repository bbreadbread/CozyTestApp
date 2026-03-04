using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Participant
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public int? CuratorCreateId { get; set; }

    public bool? IsArchive { get; set; }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Curator? CuratorCreate { get; set; }

    public virtual ICollection<ParticipantsAssignedTest> ParticipantsAssignedTests { get; set; } = new List<ParticipantsAssignedTest>();

    public virtual ICollection<Curator> Curators { get; set; } = new List<Curator>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
