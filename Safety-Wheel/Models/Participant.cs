using CozyTest.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace CozyTest.Models;

public partial class Participant : ObservableObject
{
    ParticipantPublicTestService _participantService = new();
    
    private int _id;
    private string? _name;
    private string? _login;
    private string? _password;
    private int? _curatorCreateId;
    private bool? _isArchive;
    private Curator? _curatorCreate;

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

    public int? CuratorCreateId
    {
        get => _curatorCreateId;
        set => SetProperty(ref _curatorCreateId, value);
    }

    public bool? IsArchive
    {
        get => _isArchive;
        set => SetProperty(ref _isArchive, value);
    }

    public virtual Curator? CuratorCreate
    {
        get => _curatorCreate;
        set => SetProperty(ref _curatorCreate, value);
    }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();
    public virtual ICollection<ParticipantsAssignedTest> ParticipantsAssignedTests { get; set; } = new List<ParticipantsAssignedTest>();
    public virtual ICollection<ParticipantsPublicTest> ParticipantsPublicTests { get; set; } = new List<ParticipantsPublicTest>();
    public virtual ICollection<Curator> Curators { get; set; } = new List<Curator>();
    public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    public virtual ICollection<Test> Tests { get; set; } = new List<Test>();

    ///////////////////

    [NotMapped]
    private bool _isPublished;
    [NotMapped]
    public bool IsPublished
    {
        get 
        {
            IsPublished = _participantService.IsBindingUserTest(Id);
            return _isPublished;
        }

        set
        {
            SetProperty(ref _isPublished, value);
        }
    }
}
