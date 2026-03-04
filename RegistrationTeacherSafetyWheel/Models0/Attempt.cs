using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RegistrationCuratorSafetyWheel.Models;

public class Attempt : ObservableObject
{
    private int _id;
    private int? _participantsId;
    private int? _testId;
    private DateTime? _startedAt;
    private DateTime? _finishedAt;
    private int? _score;
    private string? _status;
    private int? _testType;
    private ObservableCollection<ParticipantAnswer> _participantAnswers = new();
    private Participant? _participants;
    private DTestType? _testTypeNavigation;
    private Test? _test;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int? ParticipantId
    {
        get => _participantsId;
        set => SetProperty(ref _participantsId, value);
    }

    public int? TestId
    {
        get => _testId;
        set => SetProperty(ref _testId, value);
    }

    public DateTime? StartedAt
    {
        get => _startedAt;
        set => SetProperty(ref _startedAt, value);
    }

    public DateTime? FinishedAt
    {
        get => _finishedAt;
        set => SetProperty(ref _finishedAt, value);
    }

    public int? Score
    {
        get => _score;
        set => SetProperty(ref _score, value);
    }

    public string? Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public int? DTestType
    {
        get => _testType;
        set => SetProperty(ref _testType, value);
    }

    public virtual ObservableCollection<ParticipantAnswer> ParticipantAnswers
    {
        get => _participantAnswers;
        set => SetProperty(ref _participantAnswers, value);
    }

    public virtual Participant? Participants
    {
        get => _participants;
        set => SetProperty(ref _participants, value);
    }
    public virtual Test? Test
    {
        get => _test;
        set => SetProperty(ref _test, value);
    }
    public virtual DTestType? DTestTypeNavigation
    {
        get => _testTypeNavigation;
        set => SetProperty(ref _testTypeNavigation, value);
    }


}
