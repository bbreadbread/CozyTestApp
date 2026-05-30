using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CozyTest.ViewModels
{
    public class LogsViewModel : BaseViewModel
    {
        private readonly IDbContextFactory<CozyTestContext> _factory;
        private readonly ILoggingService _logger;

        public ObservableCollection<UserActionLog> LogsList { get; } = new();
        public ObservableCollection<UserActionLog> FilteredLogsList { get; } = new();

        public ObservableCollection<string> WhoList { get; } = new()
        {
            "Все",
            "Куратор",
            "Куратор-администратор",
            "Тестируемый"
        };

        public ObservableCollection<string> CuratorsList { get; } = new();
        public ObservableCollection<string> ParticipantsList { get; } = new();

        public ObservableCollection<string> LevelLogList { get; } = new()
        {
            "Все",
            "Authorization",
            "Create",
            "Edit",
            "Admin",
            "Archive",
            "Delete",
            "Public",
            "Assigned"
        };

        public ObservableCollection<string> ObjectList { get; } = new()
        {
            "Все",
            "Curator",
            "Participant",
            "Group",
            "Test",
            "Question",
            "Application"
        };

        private string _selectedWho = "Все";
        public string SelectedWho
        {
            get => _selectedWho;
            set
            {
                _selectedWho = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectWho));
                ApplyFilters();
            }
        }

        public string SelectWho => SelectedWho;

        private string _selectedCurator;
        public string SelectedCurator
        {
            get => _selectedCurator;
            set
            {
                _selectedCurator = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private string _selectedParticipant;
        public string SelectedParticipant
        {
            get => _selectedParticipant;
            set
            {
                _selectedParticipant = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private string _selectedLevelLog = "Все";
        public string SelectedLevelLog
        {
            get => _selectedLevelLog;
            set
            {
                _selectedLevelLog = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private string _selectedObject = "Все";
        public string SelectedObject
        {
            get => _selectedObject;
            set
            {
                _selectedObject = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        private UserActionLog _selectedLog;
        public UserActionLog SelectedLog
        {
            get => _selectedLog;
            set
            {
                _selectedLog = value;
                OnPropertyChanged();
            }
        }

        public ICommand WhoMadeCommand { get; }
        public ICommand LevelLogCommand { get; }
        public ICommand ObjectCommand { get; }
        public ICommand ClearFiltersCommand { get; }

        public LogsViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            IDbContextFactory<CozyTestContext> factory,
            ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _factory = factory;
            _logger = logger;

            WhoMadeCommand = new RelayCommand(_ => FilterByWhoMade());
            LevelLogCommand = new RelayCommand(_ => FilterByLevelLog());
            ObjectCommand = new RelayCommand(_ => FilterByObject());
            ClearFiltersCommand = new RelayCommand(_ => ClearFiltersAsync());

            _ = LoadDataAsync();
        }

        private async Task ClearFiltersAsync()
        {
            SelectedWho = "Все";
            SelectedCurator = "Все";
            SelectedParticipant = "Все";
            SelectedLevelLog = "Все";
            SelectedObject = "Все";
        }

        private async Task LoadDataAsync()
        {
            using var db = _factory.CreateDbContext();
            var logs = await db.UserActionLogs
                .OrderByDescending(l => l.TimeStamp)
                .ToListAsync();

            var curators = await db.Curators
                .Select(c => c.Name)
                .Distinct()
                .ToListAsync();

            var participants = await db.Participants
                .Select(p => p.Name)
                .Distinct()
                .ToListAsync();

            await System.Windows.Application.Current.Dispatcher.InvokeAsync(() =>
            {
                LogsList.Clear();
                foreach (var log in logs)
                    LogsList.Add(log);

                CuratorsList.Clear();
                CuratorsList.Add("Все");
                foreach (var c in curators)
                    CuratorsList.Add(c);

                ParticipantsList.Clear();
                ParticipantsList.Add("Все");
                foreach (var p in participants)
                    ParticipantsList.Add(p);

                ApplyFilters();
            });
        }

        private void ApplyFilters()
        {
            var filtered = LogsList.AsEnumerable();

            if (SelectedWho != "Все")
            {
                filtered = filtered.Where(l =>
                {
                    if (string.IsNullOrEmpty(l.TypeWhoMade)) return false;

                    return SelectedWho switch
                    {
                        "Куратор" => l.TypeWhoMade == "Curator" && !l.Message.Contains("администратор"),
                        "Куратор-администратор" => l.TypeWhoMade == "Curator" && l.Message.Contains("администратор"),
                        "Тестируемый" => l.TypeWhoMade == "Participant",
                        _ => true
                    };
                });
            }

            if (!string.IsNullOrEmpty(SelectedCurator) && SelectedCurator != "Все" &&
                (SelectedWho == "Куратор" || SelectedWho == "Куратор-администратор"))
            {
                filtered = filtered.Where(l => l.WhoMade == SelectedCurator);
            }

            if (!string.IsNullOrEmpty(SelectedParticipant) && SelectedParticipant != "Все" &&
                SelectedWho == "Тестируемый")
            {
                filtered = filtered.Where(l => l.WhoMade == SelectedParticipant);
            }

            if (SelectedLevelLog != "Все")
            {
                filtered = filtered.Where(l =>
                    l.LevelLog == GetLevelLogValue(SelectedLevelLog));
            }

            if (SelectedObject != "Все")
            {
                filtered = filtered.Where(l =>
                    l.TypeObject != null && l.TypeObject == SelectedObject);
            }

            FilteredLogsList.Clear();
            foreach (var log in filtered)
                FilteredLogsList.Add(log);
        }

        private int GetLevelLogValue(string level)
        {
            return level switch
            {
                "Authorization" => 0,
                "Create" => 1,
                "Edit" => 2,
                "Admin" => 3,
                "Archive" => 4,
                "Delete" => 5,
                "Public" => 6,
                "Assigned" => 7,
                _ => -1
            };
        }

        private void FilterByWhoMade()
        {
            if (SelectedLog == null) return;
            SelectedWho = "Все";

            if (SelectedLog.TypeWhoMade == "Curator")
                SelectedWho = SelectedLog.Message.Contains("администратор")
                    ? "Куратор-администратор"
                    : "Куратор";
            else if (SelectedLog.TypeWhoMade == "Participant")
                SelectedWho = "Тестируемый";

            SelectedCurator = SelectedLog.WhoMade;
            SelectedParticipant = SelectedLog.WhoMade;
        }

        private void FilterByLevelLog()
        {
            if (SelectedLog == null) return;
            SelectedLevelLog = SelectedLog.LevelLog switch
            {
                0 => "Authorization",
                1 => "Create",
                2 => "Edit",
                3 => "Admin",
                4 => "Archive",
                5 => "Delete",
                6 => "Public",
                7 => "Assigned",
                _ => "Все"
            };
        }

        private void FilterByObject()
        {
            if (SelectedLog == null) return;
            SelectedObject = SelectedLog.TypeObject ?? "Все";
        }
    }
}
