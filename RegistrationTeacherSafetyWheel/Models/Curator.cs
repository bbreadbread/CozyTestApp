using System;
using System.Collections.Generic;

namespace RegistrationCuratorCozyTest.Models;

public partial class Curator
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Login { get; set; }

    public string? Password { get; set; }

    public bool? IsAdmin { get; set; }

    public bool? IsArchive { get; set; }

    public virtual ICollection<Application> Applications { get; set; } = new List<Application>();

    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();

    public virtual ICollection<Participant> ParticipantsNavigation { get; set; } = new List<Participant>();

    public virtual ICollection<Test> TestsNavigation { get; set; } = new List<Test>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();

    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
