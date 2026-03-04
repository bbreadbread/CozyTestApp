using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class TestType : ObservableObject
{
    private int _id;
    private string? _name;
    private string? _description;
    private int? _timeLimitSecond;
    private ObservableCollection<Attempt> _attempts = new();

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

    public string? Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public int? TimeLimitSecond
    {
        get => _timeLimitSecond;
        set => SetProperty(ref _timeLimitSecond, value);
    }

    public virtual ObservableCollection<Attempt> Attempts
    {
        get => _attempts;
        set => SetProperty(ref _attempts, value);
    }
}