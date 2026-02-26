using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Safety_Wheel.Models;

public class Attempt : ObservableObject
{
    private int _id;
    private int? _studentsId;
    private int? _testId;
    private DateTime? _startedAt;
    private DateTime? _finishedAt;
    private int? _score;
    private string? _status;
    private int? _testType;
    private ObservableCollection<StudentAnswer> _studentAnswers = new();
    private Student? _students;
    private TestType? _testTypeNavigation;
    private Test? _test;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int? StudentsId
    {
        get => _studentsId;
        set => SetProperty(ref _studentsId, value);
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

    public int? TestType
    {
        get => _testType;
        set => SetProperty(ref _testType, value);
    }

    public virtual ObservableCollection<StudentAnswer> StudentAnswers
    {
        get => _studentAnswers;
        set => SetProperty(ref _studentAnswers, value);
    }

    public virtual Student? Students
    {
        get => _students;
        set => SetProperty(ref _students, value);
    }
    public virtual Test? Test
    {
        get => _test;
        set => SetProperty(ref _test, value);
    }
    public virtual TestType? TestTypeNavigation
    {
        get => _testTypeNavigation;
        set => SetProperty(ref _testTypeNavigation, value);
    }

    
}
