using System;
using System.Collections.Generic;

namespace Safety_Wheel.Models;

public class StudentAnswer : ObservableObject
{
    private int _attemptId;
    private int _questionId;
    private int _optionId;
    private bool? _isCorrect;
    private DateTime? _answeredAt;
    private Attempt _attempt = null!;
    private Option _option = null!;
    private Question _question = null!;

    public int AttemptId
    {
        get => _attemptId;
        set => SetProperty(ref _attemptId, value);
    }

    public int QuestionId
    {
        get => _questionId;
        set => SetProperty(ref _questionId, value);
    }

    public int OptionId
    {
        get => _optionId;
        set => SetProperty(ref _optionId, value);
    }

    public bool? IsCorrect
    {
        get => _isCorrect;
        set => SetProperty(ref _isCorrect, value);
    }

    public DateTime? AnsweredAt
    {
        get => _answeredAt;
        set => SetProperty(ref _answeredAt, value);
    }

    public virtual Attempt Attempt
    {
        get => _attempt;
        set => SetProperty(ref _attempt, value);
    }

    public virtual Option Option
    {
        get => _option;
        set => SetProperty(ref _option, value);
    }

    public virtual Question Question
    {
        get => _question;
        set => SetProperty(ref _question, value);
    }
}
