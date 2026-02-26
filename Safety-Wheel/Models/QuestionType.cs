using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class QuestionType : ObservableObject
{
    private int _id;
    private string? _name;
    private ObservableCollection<Question> _questions = new ();
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

    public virtual ObservableCollection<Question> Questions
    {
        get => _questions;
        set => SetProperty(ref _questions, value);
    }
}
