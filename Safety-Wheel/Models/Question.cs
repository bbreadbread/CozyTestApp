using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class Question : ObservableObject
{
    private int _id;
    private int? _testId;
    private int? _number;
    private string? _testQuest;
    private string? _picturePath;
    private string? _comments;
    private int? _questionType;
    private ObservableCollection<Option> _options = new();
    private ObservableCollection<StudentAnswer> _studentAnswers = new();
    private Test? _test;

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int? TestId
    {
        get => _testId;
        set => SetProperty(ref _testId, value);
    }

    public int? Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string? TestQuest
    {
        get => _testQuest;
        set => SetProperty(ref _testQuest, value);
    }

    public string? PicturePath
    {
        get => _picturePath;
        set => SetProperty(ref _picturePath, value);
    }

    public string? Comments
    {
        get => _comments;
        set => SetProperty(ref _comments, value);
    }

    public int? QuestionType 
    { 
        get => _questionType; 
        set => SetProperty(ref _questionType, value);
    }

    public virtual ObservableCollection<Option> Options
    {
        get => _options;
        set => SetProperty(ref _options, value);
    }
    public virtual QuestionType? QuestionTypeNavigation { get; set; }

    public virtual ObservableCollection<StudentAnswer> StudentAnswers
    {
        get => _studentAnswers;
        set => SetProperty(ref _studentAnswers, value);
    }

    public virtual Test? Test
    {
        get => _test;
        set => SetProperty(ref _test, value);
    }
}
