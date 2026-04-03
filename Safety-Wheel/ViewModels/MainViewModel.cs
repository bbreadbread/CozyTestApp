using CozyTest.Models;
using CozyTest.Pages.Curator;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using CozyTest.ViewModels.StatisticsVM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CozyTest.ViewModels.CreateTestsVM;
using Microsoft.Extensions.DependencyInjection;

namespace CozyTest.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;    
        private readonly INavigationService _navigationService;

        private ObservableCollection<MenuItemViewModel> _mainMenuItems;
        private ObservableCollection<MenuItemViewModel> _menuOptionItems;
        private ObservableCollection<MenuItemViewModel> _menuOptionDateItems;

        private ObservableCollection<MenuItemViewModel> _menuItems;
        private ObservableCollection<MenuItemViewModel> _menuDatesItems;
        private ObservableCollection<MenuItemViewModel> _menuAttemptsItems;
        private ObservableCollection<MenuItemViewModel> _menuTestsItems;

        private ParticipantService _participantService = new();
        private AttemptService _attemptService = new();
        private TopicService _subjectService = new();
        private TestService _testService = new();

        private MenuItemViewModel _selectedMainMenuItem;
        private MenuItemViewModel _selectedParticipant;
        private MenuItemViewModel _selectedDate;
        private MenuItemViewModel _selectedAttempt;
        private MenuItemViewModel _selectedTest;

        private Participant _currentParticipant;
        private DateTime? _selectedDateValue;
        private object _currentContent;
        public enum MainMenuType
        {
            TestResults,
            Statistics,
            EditCreateTests,
            CuratorManager,
            MonthFilter,

            Home,
            Profile,

            Participants,
            Curators,
            Requests,
            Groups,
        }

        public MainViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;

            GoBackCommand = new RelayCommand(_ => ExecuteGoBack());
            ExitCommand = new RelayCommand(_ => ExecuteExit());

            CurrentContent = App.Services.GetRequiredService<CuratorWelcomePageViewModel>();

        }

        public void CreateMainMenuItems()
        {
            switch (CurrentUser.TypeUser)
            {
                case 1:
                    MainMenuItems = new ObservableCollection<MenuItemViewModel> {
                        new MenuItemViewModel(this)
                        {
                             Icon = new Image
                             {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/statistic_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                             },
                            Label = "Статистика",
                            ToolTip = "Статистика по тестам и студентам",
                            Tag = MainMenuType.Statistics
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/result_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Результаты тестирования",
                            ToolTip = "Просмотр результатов тестирования студентов",
                            Tag = MainMenuType.TestResults
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/test_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Создание тестов",
                            ToolTip = "Создание и редактирование тестов",
                            Tag = MainMenuType.EditCreateTests
                        }
                    };

                    MenuOptionItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/participant_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Тестируемые",
                            ToolTip = "Пользователи",
                            Tag = MainMenuType.Participants
                        },
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/group_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Группы",
                            ToolTip = "Группы",
                            Tag = MainMenuType.Groups
                        },
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/curator_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Экзаменаторы",
                            ToolTip = "Пользователи",
                            Tag = MainMenuType.Curators
                        },
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/requests_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Заявки",
                            ToolTip = "Заявки",
                            Tag = MainMenuType.Requests
                        }
                        
                    };
                    break;

                case 2:
                    MainMenuItems = new ObservableCollection<MenuItemViewModel> {
                        new MenuItemViewModel(this)
                        {
                             Icon = new Image
                             {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/statistic_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                             },
                            Label = "Статистика",
                            ToolTip = "Статистика по тестам и студентам",
                            Tag = MainMenuType.Statistics
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/result_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Результаты тестирования",
                            ToolTip = "Просмотр результатов тестирования студентов",
                            Tag = MainMenuType.TestResults
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/test_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Создание тестов",
                            ToolTip = "Создание и редактирование тестов",
                            Tag = MainMenuType.EditCreateTests
                        }
                    };

                    MenuOptionItems = new ObservableCollection<MenuItemViewModel>
                    {
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/participant_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Тестируемые",
                            ToolTip = "Пользователи",
                            Tag = MainMenuType.Participants
                        },
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/group_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Группы",
                            ToolTip = "Группы",
                            Tag = MainMenuType.Groups
                        },
                        new MenuItemViewModel(this)
                        {
                           Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/curator_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Экзаменаторы",
                            ToolTip = "Пользователи",
                            Tag = MainMenuType.Curators
                        }
                        
                    };
                    break;

                case 3:
                    MainMenuItems = new ObservableCollection<MenuItemViewModel> {
                        new MenuItemViewModel(this)
                        {
                             Icon = new Image
                             {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/home_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                             },
                            Label = "Статистика",
                            ToolTip = "Статистика по тестам и студентам",
                            Tag = MainMenuType.Home
                        },
                        new MenuItemViewModel(this)
                        {
                            Icon = new Image
                            {
                                Source = new BitmapImage(new Uri("pack://application:,,,/Images/profile_icon.png")),
                                Width = 45,
                                Height = 45,
                                Stretch = Stretch.Uniform
                            },
                            Label = "Результаты тестирования",
                            ToolTip = "Просмотр результатов тестирования студентов",
                            Tag = MainMenuType.Profile
                        }
                    };

                    MenuOptionItems = new ObservableCollection<MenuItemViewModel>
                    { };

                    break;

                default:
                    break;
            }
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
            switch (CurrentUser.TypeUser)
            {
                case 1:

                    CreateMainMenuItems();
                    CreateMenuItems();

                    _participantService.GetAllParticipants(CurrentUser.Id);
                    CurrentContent = App.Services.GetRequiredService<CuratorWelcomePageViewModel>();


                    break;

                case 2:

                    CreateMainMenuItems();
                    CreateMenuItems();

                    _participantService.GetAllParticipants(CurrentUser.Id);

                    CurrentContent = App.Services.GetRequiredService<CuratorWelcomePageViewModel>();

                    break;

                case 3:

                    CreateMainMenuItems();
                    CreateMenuItems();

                    break;

                default:
                    break;
            }
        }

        public void ReloadParticipants()
        {
            _participantService.ReloadParticipants(CurrentUser.Id);

            if (SelectedMainMenuItem?.Tag is MainMenuType.TestResults)
                LoadParticipantsForResultsAsync();

            if (SelectedMainMenuItem?.Tag is MainMenuType.Statistics)
                LoadParticipantsForStatisticsAsync();
        }

        public void ResetApplicationState()
        {
            SelectedParticipant = null;
            SelectedMainMenuItem = null;

            MenuDatesItems.Clear();
            MenuAttemptsItems.Clear();
            MenuOptionDateItems.Clear();

            CurrentContent = null;

            AttemptsTableVisible = false;
            StatisticTableVisible = false;
            SecondMenuVisible = false;
        }

        //private async void LoadParticipantDatesAsync(Participant participant)
        //{
        //    if (participant == null) return;

        //    MenuDatesItems.Clear();
        //    MenuAttemptsItems.Clear();
        //    CurrentContent = null;
        //    MenuOptionDateItems.Clear();

        //    List<DateTime> list = _attemptService.GetUniqueAttemptDates(participant.Id);

        //    foreach (var date in list)
        //    {
        //        var dateItem = new MenuItemViewModel(this)
        //        {
        //            Icon = new TextBlock
        //            {
        //                Text = $"{date:dd.MM}",
        //                FontSize = 20,
        //                FontWeight = FontWeights.Bold,
        //                Foreground = Brushes.White,
        //                HorizontalAlignment = HorizontalAlignment.Center,
        //                VerticalAlignment = VerticalAlignment.Center
        //            },
        //            Label = $"Дата: {date:dd.MM.yyyy}",
        //            ToolTip = $"Кликните, чтобы увидеть попытки за {date:dd.MM.yyyy}",
        //            Tag = new DateInfo
        //            {
        //                Date = date,
        //                Participant = participant
        //            }
        //        };

        //        MenuDatesItems.Add(dateItem);
        //    }

        //    var q = new MenuItemViewModel(this)
        //    {
        //        Icon = new PackIconMaterial
        //        {
        //            Kind = PackIconMaterialKind.FilterMenu,
        //            Foreground = Brushes.White,
        //            Width = 30,
        //            Height = 30
        //        },
        //        Label = "Фильтрация дат",
        //        ToolTip = "Фильтрация дат",
        //        Tag = MainMenuType.MonthFilter
        //    };

        //    MenuOptionDateItems.Add(q);

        //    SelectedDate = null;

        //}

        //private async void LoadParticipantDatesAsync(Participant participant, int? year, int? month)
        //{
        //    if (participant == null) return;

        //    MenuDatesItems.Clear();
        //    MenuAttemptsItems.Clear();
        //    MenuOptionDateItems.Clear();
        //    CurrentContent = null;

        //    var dates = _attemptService
        //        .GetUniqueAttemptDates(participant.Id)
        //        .AsEnumerable();

        //    if (year.HasValue)
        //        dates = dates.Where(d => d.Year == year.Value);

        //    if (month.HasValue)
        //        dates = dates.Where(d => d.Month == month.Value);

        //    foreach (var date in dates.OrderBy(d => d))
        //    {
        //        MenuDatesItems.Add(new MenuItemViewModel(this)
        //        {
        //            Icon = new TextBlock
        //            {
        //                Text = $"{date:dd.MM}",
        //                FontSize = 20,
        //                FontWeight = FontWeights.Bold,
        //                Foreground = Brushes.White
        //            },
        //            Label = $"Дата: {date:dd.MM.yyyy}",
        //            Tag = new DateInfo
        //            {
        //                Date = date,
        //                Participant = participant
        //            }
        //        });
        //    }

        //    var q = new MenuItemViewModel(this)
        //    {
        //        Icon = new PackIconMaterial
        //        {
        //            Kind = PackIconMaterialKind.FilterMenu,
        //            Foreground = Brushes.White,
        //            Width = 30,
        //            Height = 30
        //        },
        //        Label = "Фильтрация дат",
        //        ToolTip = "Фильтрация дат",
        //        Tag = MainMenuType.MonthFilter
        //    };

        //    if (dates.Count() == 0)
        //    {
        //        MenuDatesItems.Add(q);
        //        return;
        //    }

        //    MenuOptionDateItems.Add(q);
        //}


        private async Task LoadTestsAsync()
        {
            MenuTestsItems.Clear();
            CurrentContent = null;

            await Task.Run(() =>
            {
                _testService.GetAll(null, CurrentUser.Id);
            });


            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
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
            });
        }

        private async Task LoadParticipantsForResultsAsync()
        {
            MenuItems.Clear();


            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                foreach (var st in _participantService.Participants)
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
            });
        }
        private async Task LoadParticipantsForStatisticsAsync()
        {
            MenuItems.Clear();

            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
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

                foreach (var st in _participantService.Participants)
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
            });
        }

        private async Task LoadTopicForEditAsync()
        {
            MenuItems.Clear();
            CurrentContent = null;
            _subjectService.GetAll();

            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
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

                foreach (var sub in _subjectService.Topics)
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
            });
        }

        private bool _isNavigating;
        public MenuItemViewModel SelectedMainMenuItem
        {
            get => _selectedMainMenuItem;
            set
            {
                if (value == null || _isNavigating) return;
                if (!SetProperty(ref _selectedMainMenuItem, value)) return;

                _ = LoadContentAsync(value).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Dispatcher.CurrentDispatcher.Invoke(() =>
                            MessageBox.Show($"Ошибка: {t.Exception?.InnerException?.Message}"));
                    }
                }, TaskScheduler.Current);
            }
        }

        private async Task LoadContentAsync(MenuItemViewModel value)
        {
            _isNavigating = true;

            try
            {
                MenuItems.Clear();
                MenuDatesItems.Clear();
                MenuAttemptsItems.Clear();
                CurrentContent = null;
                SelectedParticipant = null;
                SelectedAttempt = null;

                if (value.Tag is MainMenuType menuType)
                {
                    switch (menuType)
                    {
                        case MainMenuType.TestResults:
                            AttemptsTableVisible = true;
                            StatisticTableVisible = false;
                            SecondMenuVisible = true;
                            await LoadParticipantsForResultsAsync();
                            CurrentContent = null;
                            break;

                        case MainMenuType.Statistics:
                            await LoadTestsAsync();
                            AttemptsTableVisible = false;
                            StatisticTableVisible = true;
                            SecondMenuVisible = true;
                            await LoadParticipantsForStatisticsAsync();
                            CurrentContent = new StatisticsViewModel(null);
                            break;

                        case MainMenuType.EditCreateTests:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = true;
                            await LoadTopicForEditAsync();
                            CurrentContent = App.Services.GetRequiredService<CuratorAllTestViewModel>(); 
                            break;

                        case MainMenuType.Participants:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = App.Services.GetRequiredService<ParticipantsViewModel>();

                            break;

                        case MainMenuType.Curators:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = App.Services.GetRequiredService<CuratorsViewModel>();
                            break;

                        case MainMenuType.Requests:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = App.Services.GetRequiredService<RequestsViewModel>();
                            break;

                        case MainMenuType.Groups:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = App.Services.GetRequiredService<GroupsViewModel>();
                            break;

                        case MainMenuType.Home:
                            await LoadTestsAsync();
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = null; 
                            break;

                        case MainMenuType.Profile:
                            AttemptsTableVisible = false;
                            StatisticTableVisible = false;
                            SecondMenuVisible = false;
                            CurrentContent = null;
                            break;
                    }
                }
            }
            finally
            {
                _isNavigating = false;
            }
        }

        public MenuItemViewModel SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                if (!SetProperty(ref _selectedParticipant, value))
                    return;

                if (SelectedMainMenuItem?.Tag is not MainMenuType menuType)
                    return;

                if (menuType == MainMenuType.Statistics && value?.Tag == null)
                {
                    CurrentParticipant = null;
                    CurrentContent = new StatisticsViewModel(null);


                    CuratorStatisticsPage.DataPageCurator?.LoadStatistics(null, SelectedTest?.Tag as Test);
                    return;
                }
                else if (menuType == MainMenuType.EditCreateTests)
                {
                    if (value?.Tag == null)
                    {
                        _navigationService.NavigateTo(new CuratorsViewModel(_dialogService, _navigationService));
                    }
                    else if (value?.Tag is Topic subject)
                    {
                        _navigationService.NavigateTo(new CuratorsViewModel(_dialogService, _navigationService));
                    }
                }
                else if (menuType == MainMenuType.TestResults)
                {
                    _navigationService.NavigateTo(new CuratorsViewModel(_dialogService, _navigationService));
                }


                if (value?.Tag is Participant participant)
                {

                    switch (menuType)
                    {
                        case MainMenuType.TestResults:
                            CurrentParticipant = participant;
                            break;

                        case MainMenuType.Statistics:
                            CurrentParticipant = participant;
                            CuratorStatisticsPage.DataPageCurator?.LoadStatistics(participant, SelectedTest?.Tag as Test);
                            break;
                    }
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
                    //if (value?.Tag is Attempt attempt && CurrentParticipant != null)
                    //{
                    //    var test = _testService.GetTestById(attempt.TestId);
                    //    int? sec = attempt.FinishedAt == null ? null : (int)(attempt.FinishedAt - attempt.StartedAt)?.TotalSeconds;

                    //    var studPage = new CozyTest.Pages.Participant.PartTest(test, sec, true, attempt);
                    //    _navigationService.NavigateTo(studPage);
                    //}
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
                var participant = SelectedParticipant?.Tag as Participant;

                CuratorStatisticsPage.DataPageCurator
                    ?.LoadStatistics(participant, test);
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
            set
            {
                SetProperty(ref _currentContent, value);
            }
        }
        public Participant CurrentParticipant
        {
            get => _currentParticipant;
            set => SetProperty(ref _currentParticipant, value);
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

        //с основного окна
        public event EventHandler<NavigationRequestEventArgs> NavigationRequested;
        public event EventHandler ExitRequested;
        public event EventHandler ClearRequested;
        public ICommand GoBackCommand { get; }
        public ICommand ExitCommand { get; }
        private void ExecuteGoBack()
        {
            NavigationRequested?.Invoke(this, new NavigationRequestEventArgs(NavigationAction.GoBack));
        }

        private void ExecuteExit()
        {
            ExitRequested?.Invoke(this, EventArgs.Empty);
        }

        public void RequestClear()
        {
            ClearRequested?.Invoke(this, EventArgs.Empty);
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
        private string _participantName, _participantLogin, _participantPassword;
        public string ParticipantName
        {
            get => _participantName;
            set { _participantName = value; OnPropertyChanged(); }
        }
        public string ParticipantLogin
        {
            get => _participantLogin;
            set { _participantLogin = value; OnPropertyChanged(); }
        }
        public string ParticipantPassword
        {
            get => _participantPassword;
            set { _participantPassword = value; OnPropertyChanged(); }
        }

        private string _teacherName = "";
        private string _teacherLogin = "";
        private string _teacherPassword = "";

        public string CuratorName
        {
            get => _teacherName;
            set { _teacherName = value; OnPropertyChanged(); }
        }
        public string CuratorLogin
        {
            get => _teacherLogin;
            set { _teacherLogin = value; OnPropertyChanged(); }
        }
        public string CuratorPassword
        {
            get => _teacherPassword;
            set { _teacherPassword = value; OnPropertyChanged(); }
        }
    }
    public class DateInfo
    {
        public DateTime Date { get; set; }
        public Participant Participant { get; set; }
    }

    public enum NavigationAction { GoBack, ExitToHome, Logout }

    public class NavigationRequestEventArgs : EventArgs
    {
        public NavigationAction Action { get; }
        public NavigationRequestEventArgs(NavigationAction action) => Action = action;
    }
}