using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class Subject : ObservableObject
{
    private int _id;
    private string? _name;
    private ObservableCollection<Test> _tests = new();

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

    public virtual ObservableCollection<Test> Tests
    {
        get => _tests;
        set => SetProperty(ref _tests, value);
    }
}
