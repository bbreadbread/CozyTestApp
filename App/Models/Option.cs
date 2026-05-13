using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class Option : ObservableObject
{
    private int _id;
    private int _questionId;
    private int? _number;
    private string? _textAnswer;
    private bool? _isCorrect;
    private Question? _question;
    private ObservableCollection<StudentAnswer> _studentAnswers = new();

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    public int QuestionId
    {
        get => _questionId;
        set => SetProperty(ref _questionId, value);
    }
    public int? Number
    {
        get => _number;
        set => SetProperty(ref _number, value);
    }

    public string? TextAnswer
    {
        get => _textAnswer;
        set => SetProperty(ref _textAnswer, value);
    }

    public bool? IsCorrect
    {
        get => _isCorrect;
        set => SetProperty(ref _isCorrect, value);
    }

    public virtual Question? Question
    {
        get => _question;
        set => SetProperty(ref _question, value);
    }

    public virtual ObservableCollection<StudentAnswer> StudentAnswers
    {
        get => _studentAnswers;
        set => SetProperty(ref _studentAnswers, value);
    }
}
