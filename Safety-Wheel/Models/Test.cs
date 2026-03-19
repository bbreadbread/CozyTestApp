using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Security.Cryptography;

namespace CozyTest.Models;

public partial class Test : ObservableObject
{
    private int _criteriaId;
    private int _maxNumPassing;
    private bool? _isArchive;
    private bool? _isPublic;
    private string _description;
    private string _name;
    private int _timeLimitSecond;
    private int _testTypeId;
    private int _penaltyMax;
    private int _curatorCreateId;
    private int _topicId;
    private int _id;
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

    public int TopicId
    {
        get => _topicId;
        set => SetProperty(ref _topicId, value);
    }

    public int CuratorCreateId
    {
        get => _curatorCreateId;
        set => SetProperty(ref _curatorCreateId, value);
    }
    DateTime _dateOfCreating;
    public int PenaltyMax
    {
        get => _penaltyMax;
        set => SetProperty(ref _penaltyMax, value);
    }
    public DateTime DateOfCreating
    {
        get => _dateOfCreating;
        set => SetProperty(ref _dateOfCreating, value);
    }
    public int TestTypeId
    {
        get => _testTypeId;
        set => SetProperty(ref _testTypeId, value);
    }
    public int TimeLimitSecond
    {
        get => _timeLimitSecond;
        set => SetProperty(ref _timeLimitSecond, value);
    }

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public bool? IsPublic
    {
        get => _isPublic;
        set => SetProperty(ref _isPublic, value);
    }

    public bool? IsArchive
    {
        get => _isArchive;
        set => SetProperty(ref _isArchive, value);
    }

    public int MaxNumPassing
    {
        get => _maxNumPassing;
        set => SetProperty(ref _maxNumPassing, value);
    }

    public int CriteriaId
    {
        get => _criteriaId;
        set => SetProperty(ref _criteriaId, value);
    }

    public virtual ICollection<Attempt> Attempts { get; set; } = new List<Attempt>();

    public virtual Criterion? Criteria { get; set; }

    public virtual Curator? CuratorCreate { get; set; }

    public virtual ICollection<ParticipantsAssignedTest> ParticipantsAssignedTests { get; set; } = new List<ParticipantsAssignedTest>();
    public virtual ICollection<ParticipantsPublicTest> ParticipantsPublicTests { get; set; } = new List<ParticipantsPublicTest>();
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual DTestType? TestType { get; set; }

    public virtual Topic? Topic { get; set; }

    public virtual ICollection<Curator> Curators { get; set; } = new List<Curator>();

    public virtual ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
