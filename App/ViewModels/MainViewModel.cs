using MahApps.Metro.IconPacks;
using Safety_Wheel.Models;
using Safety_Wheel.Pages.Teacher;
using Safety_Wheel.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Safety_Wheel.ViewModels.StatisticsVM;

namespace Safety_Wheel.ViewModels
{
    public class MainViewModel : ObservableObject
    {

        private ObservableCollection<MenuItemViewModel> _mainMenuItems;
        private ObservableCollection<MenuItemViewModel> _menuOptionItems;
        private ObservableCollection<MenuItemViewModel> _menuOptionDateItems;

        private ObservableCollection<MenuItemViewModel> _menuItems;
        private ObservableCollection<MenuItemViewModel> _menuDatesItems;
        private ObservableCollection<MenuItemViewModel> _menuAttemptsItems;
        private ObservableCollection<MenuItemViewModel> _menuTestsItems;

        private StudentService _studentService = new();
        private AttemptService _attemptService = new();
        private SubjectService _subjectService = new();
        private TestService _testService = new();

        private MenuItemViewModel _selectedMainMenuItem;
        private MenuItemViewModel _selectedStudent;
        private MenuItemViewModel _selectedDate;
        private MenuItemViewModel _selectedAttempt;
        private MenuItemViewModel _selectedTest;

        private Student _currentStudent;
        private DateTime? _selectedDateValue;
        private object _currentContent;

        public enum MainMenuType
        {
            TestResults,
            Statistics,
            EditCreateTests,
            TeacherManager,
            MonthFilter
        }

        public MainViewModel()
        {
            ApplyMonthFilterCommand = new RelayCommand(ApplyMonthFilter);
            ResetMonthFilterCommand = new RelayCommand(ResetMonthFilter);

            CreateMainMenuItems();
            CreateMenuItems();
        }

        public void CreateMainMenuItems()
        {
            MainMenuItems = new ObservableCollection<MenuItemViewModel>
            {
                 new MenuItemViewModel(this)
                 {
                    Icon = new PackIconMaterial { Kind = PackIconMaterialKind.ChartLine, Width= 30, Height = 30 },
                    Label = "Статистика",
                    ToolTip = "Статистика по тестам и студентам",
                    Tag = MainMenuType.Statistics
                 },
                new MenuItemViewModel(this)
                {
                    Icon = new PackIconMaterial { Kind = PackIconMaterialKind.ChartBar, Width= 30, Height = 30 },
                    Label = "Результаты тестирования",
                    ToolTip = "Просмотр результатов тестирования студентов",
                    Tag = MainMenuType.TestResults
                },
                new MenuItemViewModel(this)
                {
                    Icon = new PackIconMaterial { Kind = PackIconMaterialKind.PlusBox, Width= 30, Height = 30 },
                    Label = "Создание тестов",
                    ToolTip = "Создание и редактирование тестов",
                    Tag = MainMenuType.EditCreateTests
                }

            };

            MenuOptionItems = new ObservableCollection<MenuItemViewModel>
            {
                new MenuItemViewModel(this)
                {
                    Icon = new PackIconMaterial {Kind = PackIconMaterialKind.AccountCog, Width= 30, Height = 30 },
                    Label = "Управление учениками",
                    ToolTip = "Ученики преподавателя",
                    Tag = MainMenuType.TeacherManager
                }
            };
        }

        public void CreateMenuItems()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel> { };
            MenuDatesItems = new ObservableCollection<MenuItemViewModel> { };
            MenuAttemptsItems = new ObservableCollection<MenuItemViewModel> { };
            MenuTestsItems = new ObservableCollection<MenuItemViewModel> { };
            MenuOptionDateItems = new ObservableCollection<MenuItemViewModel> { };
        }

        public void InitAfterLogin()
        {
            _studentService.GetAllStudents(CurrentUser.Id);
        }

        public void ReloadStudents()
        {
            _studentService.ReloadStudents(CurrentUser.Id);

            if (SelectedMainMenuItem?.Tag is MainMenuType.TestResults)
                LoadStudentsForResults();

            if (SelectedMainMenuItem?.Tag is MainMenuType.Statistics)
                LoadStudentsForStatistics();
        }

        public void ResetApplicationState()
        {
            SelectedStudent = null;
            SelectedDate = null;
            SelectedMainMenuItem = null;

            MenuDatesItems.Clear();
            MenuAttemptsItems.Clear();
            MenuOptionDateItems.Clear();

            CurrentContent = null;

            AttemptsTableVisible = false;
            StatisticTableVisible = false;
            SecondMenuVisible = false;
        }

        private void LoadStudentDates(Student student)
        {
            if (student == null) return;

            MenuDatesItems.Clear();
            MenuAttemptsItems.Clear();
            CurrentContent = null;
            MenuOptionDateItems.Clear();

            List<DateTime> list = _attemptService.GetUniqueAttemptDates(student.Id);

            foreach (var date in list)
            {
                var dateItem = new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{date:dd.MM}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = $"Дата: {date:dd.MM.yyyy}",
                    ToolTip = $"Кликните, чтобы увидеть попытки за {date:dd.MM.yyyy}",
                    Tag = new DateInfo
                    {
                        Date = date,
                        Student = student
                    }
                };

                MenuDatesItems.Add(dateItem);
            }

            var q = new MenuItemViewModel(this)
            {
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.FilterMenu,
                    Foreground = Brushes.White,
                    Width = 30,
                    Height = 30
                },
                Label = "Фильтрация дат",
                ToolTip = "Фильтрация дат",
                Tag = MainMenuType.MonthFilter
            };

            MenuOptionDateItems.Add(q);

            SelectedDate = null;

        }

        private void LoadStudentDates(Student student, int? year, int? month)
        {
            if (student == null) return;

            MenuDatesItems.Clear();
            MenuAttemptsItems.Clear();
            MenuOptionDateItems.Clear();
            CurrentContent = null;

            var dates = _attemptService
                .GetUniqueAttemptDates(student.Id)
                .AsEnumerable();

            if (year.HasValue)
                dates = dates.Where(d => d.Year == year.Value);

            if (month.HasValue)
                dates = dates.Where(d => d.Month == month.Value);

            foreach (var date in dates.OrderBy(d => d))
            {
                MenuDatesItems.Add(new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{date:dd.MM}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White
                    },
                    Label = $"Дата: {date:dd.MM.yyyy}",
                    Tag = new DateInfo
                    {
                        Date = date,
                        Student = student
                    }
                });
            }

            var q = new MenuItemViewModel(this)
            {
                Icon = new PackIconMaterial
                {
                    Kind = PackIconMaterialKind.FilterMenu,
                    Foreground = Brushes.White,
                    Width = 30,
                    Height = 30
                },
                Label = "Фильтрация дат",
                ToolTip = "Фильтрация дат",
                Tag = MainMenuType.MonthFilter
            };

            if (dates.Count() == 0)
            {
                MenuDatesItems.Add(q);
                return;
            }

            MenuOptionDateItems.Add(q);
        }

        private void LoadDateAttempts(DateInfo dateInfo)
        {
            if (dateInfo == null) return;

            MenuAttemptsItems.Clear();
            CurrentContent = null;

            _attemptService.GetAll(studentId: dateInfo.Student.Id, date: dateInfo.Date);

            foreach (var attempt in _attemptService.Attempts.OrderByDescending(a => a.StartedAt))
            {
                var attemptItem = new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{attempt.StartedAt:HH:mm}",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = $"+ : {attempt.Score?.ToString() ?? "Не оценено"}",
                    ToolTip = $"Нажмите для просмотра теста\nВремя: {attempt.StartedAt:HH:mm}\nРезультат: {attempt.Score}",
                    Tag = attempt
                };
                MenuAttemptsItems.Add(attemptItem);
            }
            SelectedAttempt = null;
        }

        private void LoadTests()
        {
            MenuTestsItems.Clear();
            CurrentContent = null;

            _testService.GetAll(null, CurrentUser.Id);

            var attemptItemAll = new MenuItemViewModel(this)
            {
                Icon = new TextBlock
                {
                    Text = $"Все тесты",
                    FontSize = 16,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Label = $"Нажмите для просмотра всех тестов",
                ToolTip = $"Нажмите для просмотра всех тестов",
                Tag = null
            };
            MenuTestsItems.Add(attemptItemAll);

            foreach (var test in _testService.Tests)
            {
                var attemptItem = new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{test.Name}",
                        FontSize = 16,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = $"лабел",
                    ToolTip = $"тултип",
                    Tag = test
                };
                MenuTestsItems.Add(attemptItem);
            }
        }        

        private void LoadStudentsForResults()
        {
            MenuItems.Clear();

            foreach (var st in _studentService.Students)
            {
                var view = new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{st.Name}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = $"Попыток: {st.Attempts?.Count ?? 0}",
                    ToolTip = $"Студент: {st.Name}",
                    Tag = st
                };

                MenuItems.Add(view);
            }
        }
        private void LoadStudentsForStatistics()
        {
            MenuItems.Clear();

            MenuItems.Add(new MenuItemViewModel(this)
            {
                Icon = new TextBlock
                {
                    Text = "Все",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Label = "Общая статистика",
                ToolTip = "Показать статистику по всем студентам",
                Tag = null
            });

            foreach (var st in _studentService.Students)
            {
                MenuItems.Add(new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{st.Name}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = "Статистика",
                    ToolTip = $"Статистика по студенту: {st.Name}",
                    Tag = st
                });
            }
        }

        private void LoadSubjectForEdit()
        {
            MenuItems.Clear();
            CurrentContent = null;
            _subjectService.GetAll();

            MenuItems.Add(new MenuItemViewModel(this)
            {
                Icon = new TextBlock
                {
                    Text = "Все",
                    FontSize = 20,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                },
                Label = "Общая статистика",
                ToolTip = "Показать статистику по всем студентам",
                Tag = null
            });

            foreach (var sub in _subjectService.Subjects)
            {
                MenuItems.Add(new MenuItemViewModel(this)
                {
                    Icon = new TextBlock
                    {
                        Text = $"{sub.Name}",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    },
                    Label = sub.Name,
                    ToolTip = $"Показать все тесты для {sub.Name}",
                    Tag = sub
                });
            }
        }
        public MenuItemViewModel SelectedMainMenuItem
        {
            get => _selectedMainMenuItem;
            set
            {
                if (value == null)
                    return;

                if (value.Tag is MainMenuType.TeacherManager)
                    return;

                if (!SetProperty(ref _selectedMainMenuItem, value))
                    return;

                TeacherMainPage.GlobalFrame?.Navigate(new Page());

                MenuItems.Clear();
                MenuDatesItems.Clear();
                MenuAttemptsItems.Clear();
                CurrentContent = null;
                SelectedStudent = null;
                SelectedDate = null;
                SelectedAttempt = null;

                if (value.Tag is MainMenuType menuType)
                {
                    switch (menuType)
                    {
                        case MainMenuType.TestResults:
                            AttemptsTableVisible = true;
                            StatisticTableVisible = false;
                            SecondMenuVisible = true;
                            LoadStudentsForResults();
                            TeacherMainPage.GlobalFrame?.Navigate(new TeacherWelcomePage(true));
                            break;

                        case MainMenuType.Statistics:
                            LoadTests();
                            AttemptsTableVisible = false;
                            StatisticTableVisible = true;
                            SecondMenuVisible = true;
                            LoadStudentsForStatistics();
                            TeacherMainPage.GlobalFrame?.Navigate(new TeacherStatisticsPage());
                            break;

                        case MainMenuType.EditCreateTests:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = true;
                            LoadSubjectForEdit();
                            TeacherMainPage.GlobalFrame?.Navigate(new TeacherAllTests(null));
                            break;
                    }
                }
            }
        }

        public MenuItemViewModel SelectedStudent
        {
            get => _selectedStudent;
            set
            {
                if (!SetProperty(ref _selectedStudent, value))
                    return;

                if (SelectedMainMenuItem?.Tag is not MainMenuType menuType)
                    return;

                if (menuType == MainMenuType.Statistics && value?.Tag == null)
                {
                    CurrentStudent = null;
                    CurrentContent = new StatisticsViewModel(null);


                    TeacherStatisticsPage.DataPageTeacher?.LoadStatistics(null, SelectedTest?.Tag as Test);
                    return;
                }
                else if (menuType == MainMenuType.EditCreateTests)
                {
                    if (value?.Tag == null)
                    {
                        TeacherMainPage.GlobalFrame
                            ?.Navigate(new TeacherAllTests(null));
                    }
                    else if (value?.Tag is Subject subject)
                    {
                        TeacherMainPage.GlobalFrame
                            ?.Navigate(new TeacherAllTests(subject));
                    }
                }
                else if (menuType == MainMenuType.TestResults)
                {
                    TeacherMainPage.GlobalFrame?.Navigate(new TeacherWelcomePage(true));
                }


                if (value?.Tag is Student student)
                {

                    switch (menuType)
                    {
                        case MainMenuType.TestResults:
                            LoadStudentDates(student);
                            CurrentStudent = student;
                            break;

                        case MainMenuType.Statistics:
                            CurrentStudent = student;
                            TeacherStatisticsPage.DataPageTeacher?.LoadStatistics(student, SelectedTest?.Tag as Test);
                            break;
                    }
                }
            }
        }


        public MenuItemViewModel SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (!SetProperty(ref _selectedDate, value))
                    return;

                if (value?.Tag is MainMenuType.MonthFilter)
                {

                    return;
                }

                if (value?.Tag is DateInfo dateInfo)
                {
                    LoadDateAttempts(dateInfo);
                    SelectedDateValue = dateInfo.Date;
                }
            }
        }

        public MenuItemViewModel SelectedAttempt
        {
            get => _selectedAttempt;
            set
            {
                if (SetProperty(ref _selectedAttempt, value))
                {
                    if (value?.Tag is Attempt attempt && CurrentStudent != null)
                    {
                        var test = _testService.GetTestById(attempt.TestId);
                        int? sec = attempt.FinishedAt == null ? null : (int)(attempt.FinishedAt - attempt.StartedAt)?.TotalSeconds;
                        int? type = attempt.TestType;

                        var studPage = new Safety_Wheel.Pages.Student.StudTest(test, sec, type, true, attempt);
                        TeacherMainPage.GlobalFrame?.Navigate(studPage);
                    }
                }
            }
        }

        //таблица
        public MenuItemViewModel SelectedTest
        {
            get => _selectedTest;
            set
            {
                if (!SetProperty(ref _selectedTest, value))
                    return;

                if (SelectedMainMenuItem?.Tag is not MainMenuType.Statistics)
                    return;

                var test = value?.Tag as Test;
                var student = SelectedStudent?.Tag as Student;

                TeacherStatisticsPage.DataPageTeacher
                    ?.LoadStatistics(student, test);
            }
        }


        public ObservableCollection<MenuItemViewModel> MenuOptionItems
        {
            get => _menuOptionItems;
            set => SetProperty(ref _menuOptionItems, value);
        }
        public ObservableCollection<MenuItemViewModel> MenuOptionDateItems
        {
            get => _menuOptionDateItems;
            set => SetProperty(ref _menuOptionDateItems, value);
        }
        public ObservableCollection<MenuItemViewModel> MainMenuItems
        {
            get => _mainMenuItems;
            set => SetProperty(ref _mainMenuItems, value);
        }

        public ObservableCollection<MenuItemViewModel> MenuItems
        {
            get => _menuItems;
            set => SetProperty(ref _menuItems, value);
        }

        public ObservableCollection<MenuItemViewModel> MenuDatesItems
        {
            get => _menuDatesItems;
            set => SetProperty(ref _menuDatesItems, value);
        }

        public ObservableCollection<MenuItemViewModel> MenuAttemptsItems
        {
            get => _menuAttemptsItems;
            set => SetProperty(ref _menuAttemptsItems, value);
        }
        public ObservableCollection<MenuItemViewModel> MenuTestsItems
        {
            get => _menuTestsItems;
            set => SetProperty(ref _menuTestsItems, value);
        }


        public object CurrentContent
        {
            get => _currentContent;
            set => SetProperty(ref _currentContent, value);
        }
        public Student CurrentStudent
        {
            get => _currentStudent;
            set => SetProperty(ref _currentStudent, value);
        }

        public DateTime? SelectedDateValue
        {
            get => _selectedDateValue;
            set => SetProperty(ref _selectedDateValue, value);
        }

        //видимость
        private bool _attemptsTableVisible = false;
        private bool _statisticTableVisible = false;

        public bool AttemptsTableVisible
        {
            get => _attemptsTableVisible;
            set => SetProperty(ref _attemptsTableVisible, value);
        }
        public bool StatisticTableVisible
        {
            get => _statisticTableVisible;
            set => SetProperty(ref _statisticTableVisible, value);
        }

        private bool _secondMenuVisible = false;

        public bool SecondMenuVisible
        {
            get => _secondMenuVisible;
            set => SetProperty(ref _secondMenuVisible, value);
        }

        //месяцы
        private DateTime? _selectedMonthDate;
        public DateTime? SelectedMonthDate
        {
            get => _selectedMonthDate;
            set => SetProperty(ref _selectedMonthDate, value);
        }
        public ICommand ApplyMonthFilterCommand { get; }
        public ICommand ResetMonthFilterCommand { get; }
        public void ApplyMonthFilter()
        {
            if (_currentStudent == null) return;
            if (SelectedMonthDate == null) return;

            LoadStudentDates( _currentStudent, SelectedMonthDate.Value.Year, SelectedMonthDate.Value.Month );
        }

        public void ResetMonthFilter()
        {
            if (_currentStudent == null) return;

            SelectedMonthDate = null;
            LoadStudentDates(_currentStudent);
        }
        //глобальное имя
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _userFullName;
        public string UserFullName
        {
            get => _userFullName;
            set => SetProperty(ref _userFullName, value);
        }

        //измен        
        private string _studentName, _studentLogin, _studentPassword;
        public string StudentName
        {
            get => _studentName;
            set { _studentName = value; OnPropertyChanged(); }
        }
        public string StudentLogin
        {
            get => _studentLogin;
            set { _studentLogin = value; OnPropertyChanged(); }
        }
        public string StudentPassword
        {
            get => _studentPassword;
            set { _studentPassword = value; OnPropertyChanged(); }
        }

        private string _teacherName = "";
        private string _teacherLogin = "";
        private string _teacherPassword = "";

        public string TeacherName
        {
            get => _teacherName;
            set { _teacherName = value; OnPropertyChanged(); }
        }
        public string TeacherLogin
        {
            get => _teacherLogin;
            set { _teacherLogin = value; OnPropertyChanged(); }
        }
        public string TeacherPassword
        {
            get => _teacherPassword;
            set { _teacherPassword = value; OnPropertyChanged(); }
        }
    }
    public class DateInfo
    {
        public DateTime Date { get; set; }
        public Student Student { get; set; }
    }
}