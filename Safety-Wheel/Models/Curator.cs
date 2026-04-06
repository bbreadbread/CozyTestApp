using System;
using System.Collections.Generic;

namespace CozyTest.Models;

public partial class Curator : ObservableObject
{
    private int _id;
    private string? _name;
    private string? _login;
    private string? _password;
    private bool? _isAdmin;
    private bool? _isArchive;
    private int? _participantProfileId;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string? Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    public string? Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public bool? IsAdmin
    {
        get => _isAdmin;
        set => SetProperty(ref _isAdmin, value);
    }

    public bool? IsArchive
    {
        get => _isArchive;
        set => SetProperty(ref _isArchive, value);
    }
    public int? ParticipantProfileId
    {
        get => _participantProfileId;
        set => SetProperty(ref _participantProfileId, value);
    }
    public virtual ICollection<Requests> Requests { get; set; } = new List<Requests>();
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    public virtual ICollection<Participant> ParticipantsNavigation { get; set; } = new List<Participant>();
    public virtual ICollection<ParticipantsAssignedTest> BParticipantAssignedTest { get; set; } = new List<ParticipantsAssignedTest>();
    public virtual ICollection<ParticipantsPublicTest> ParticipantsPublicTests { get; set; } = new List<ParticipantsPublicTest>();
    public virtual ICollection<Test> TestsNavigation { get; set; } = new List<Test>();
    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();
}
