using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Safety_Wheel.Models;

public class Teacher : ObservableObject
{
    private int _id;
    private string? _login;
    private string? _password;
    private string? _name;
    private ObservableCollection<Student> _students = new();
    private ObservableCollection<Test> _tests = new();

    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
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

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public virtual ObservableCollection<Student> Students
    {
        get => _students;
        set => SetProperty(ref _students, value);
    }

    public virtual ObservableCollection<Test> Tests
    {
        get => _tests;
        set => SetProperty(ref _tests, value);
    }
}
