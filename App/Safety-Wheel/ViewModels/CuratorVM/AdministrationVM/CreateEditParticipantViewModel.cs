using CozyTest.Services;
using CozyTest.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.CuratorVM.AdministrationVM
{
    public class CreateEditParticipantViewModel : BaseAdminViewModel
    {
        public override string WindowTitle => "Управление пользователем";

        private readonly ParticipantsViewModel _participantsViewModel;
        private readonly ParticipantService _participantService;
        private readonly CuratorService _curatorService;
        private readonly IDialogService _dialogService;

        private string _nameParticipant;
        private string _loginParticipant;
        private string _passwordParticipant;

        public string NameParticipant
        {
            get => _nameParticipant;
            set => SetProperty(ref _nameParticipant, value);
        }
        public string LoginParticipant
        {
            get => _loginParticipant;
            set => SetProperty(ref _loginParticipant, value);
        }
        public string PasswordParticipant
        {
            get => _passwordParticipant;
            set => SetProperty(ref _passwordParticipant, value);
        }

        private Curator _selectedCuratorForBind;
        public Curator SelectedCuratorForBind
        {
            get => _selectedCuratorForBind;
            set => SetProperty(ref _selectedCuratorForBind, value);
        }

        private bool _bindMe = false;
        public bool BindMe
        {
            get => _bindMe;
            set
            {
                if (SetProperty(ref _bindMe, value) && value)
                {
                    BindFor = false;
                    BindNone = false;
                }
            }
        }

        private bool _bindFor = false;
        public bool BindFor
        {
            get => _bindFor;
            set
            {
                if (SetProperty(ref _bindFor, value) && value)
                {
                    BindMe = false;
                    BindNone = false;
                }
            }
        }

        private bool _bindNone = true;
        public bool BindNone
        {
            get => _bindNone;
            set
            {
                if (SetProperty(ref _bindNone, value) && value)
                {
                    BindMe = false;
                    BindFor = false;
                }
            }
        }

        private ObservableCollection<Curator> _curatorsList;
        public ObservableCollection<Curator> CuratorsList
        {
            get => _curatorsList;
            set => SetProperty(ref _curatorsList, value);
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public RelayCommand SaveParticipantCommand { get; }

        public CreateEditParticipantViewModel(
            IDialogService dialogService,
            INavigationService navigationService,
            ParticipantsViewModel participantsViewModel,
            ParticipantService participantService,
            CuratorService curatorService)
            : base(dialogService, navigationService, participantService, curatorService, null, null, null)
        {
            _participantsViewModel = participantsViewModel;
            _participantService = participantService;
            _curatorService = curatorService;
            _dialogService = dialogService;

            SaveParticipantCommand = new RelayCommand(_ => _ = SaveParticipantAsync(), _ => !IsLoading);

            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                await _curatorService.InitializeAsync();
                CuratorsList = new ObservableCollection<Curator>(_curatorService.Curators);

                if (_participantsViewModel?.SelectedParticipant != null)
                {
                    LoadSelectedParticipant();
                }
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки данных: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadSelectedParticipant()
        {
            if (_participantsViewModel?.SelectedParticipant != null)
            {
                NameParticipant = _participantsViewModel.SelectedParticipant.Name;
                LoginParticipant = _participantsViewModel.SelectedParticipant.Login;
                PasswordParticipant = _participantsViewModel.SelectedParticipant.Password;
            }
        }

        public async Task SaveParticipantAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                if (string.IsNullOrWhiteSpace(NameParticipant) ||
                    string.IsNullOrWhiteSpace(LoginParticipant) ||
                    string.IsNullOrWhiteSpace(PasswordParticipant))
                {
                    _dialogService.ShowMessage("Заполните все поля", "Ошибка");
                    return;
                }

                int? curatorId = null;
                if (BindMe)
                {
                    curatorId = CurrentUser.Id;
                }
                else if (BindFor && SelectedCuratorForBind != null)
                {
                    curatorId = SelectedCuratorForBind.Id;
                }

                if (_participantsViewModel?.SelectedParticipant != null)
                {
                    var part = new Participant()
                    {
                        Id = _participantsViewModel.SelectedParticipant.Id,
                        Name = NameParticipant,
                        Login = LoginParticipant,
                        Password = PasswordParticipant,
                        CuratorCreateId = _participantsViewModel.SelectedParticipant.CuratorCreateId,
                        CuratorCreate = _participantsViewModel.SelectedParticipant.CuratorCreate,
                        IsArchive = _participantsViewModel.SelectedParticipant.IsArchive,
                    };

                    await _participantService.UpdateAsync(part);

                    if (curatorId.HasValue)
                    {
                        await _participantService.UpdateParticipantBindForCuratorAsync(part.Id, curatorId.Value, true);
                    }

                    _participantsViewModel.SelectedParticipant.Name = NameParticipant;
                    _participantsViewModel.SelectedParticipant.Login = LoginParticipant;
                    _participantsViewModel.SelectedParticipant.Password = PasswordParticipant;

                    _dialogService.ShowMessage("Данные успешно обновлены", "Успех");
                }
                else
                {
                    var newP = new Participant()
                    {
                        Name = NameParticipant,
                        Login = LoginParticipant,
                        Password = PasswordParticipant,
                        CuratorCreateId = CurrentUser.Id,
                        CuratorCreate = CurrentUser.ClassUser as Curator,
                        IsArchive = false,
                    };

                    await _participantService.AddAsync(newP);

                    if (curatorId.HasValue && curatorId.Value != CurrentUser.Id)
                    {
                        await _participantService.UpdateParticipantBindForCuratorAsync(newP.Id, curatorId.Value, true);
                    }

                    await _participantsViewModel.ReloadParticipantsAsync();

                    _dialogService.ShowMessage("Участник успешно добавлен", "Успех");
                }

                _dialogService.CloseWindow(this);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка сохранения: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public override async Task ApplyFiltersAsync()
        {
            await Task.CompletedTask;
        }
    }
}