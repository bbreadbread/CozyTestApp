using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class Student : ObservableObject
{
    private int _id;
    private string? _name;
    private string? _login;
    private string? _password;
    private int? _teachersId;
    private ObservableCollection<Attempt> _attempts = new();
    private Teacher? _teachers;

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

    public int? TeachersId
    {
        get => _teachersId;
        set => SetProperty(ref _teachersId, value);
    }

    public virtual ObservableCollection<Attempt> Attempts
    {
        get => _attempts;
        set => SetProperty(ref _attempts, value);
    }

    public virtual Teacher? Teachers
    {
        get => _teachers;
        set => SetProperty(ref _teachers, value);
    }
}
