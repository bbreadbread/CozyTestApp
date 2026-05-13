using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using CozyTest.ViewModels.CuratorVM;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;

namespace CozyTest.ViewModels.ParticipantVM
{
    public class PartAllTestViewModel : BaseViewModel
    {
        private readonly TestService _testService;
        private readonly TopicService _topicService;
        private readonly AttemptService _attemptService;
        private readonly ParticipantFavoriteTestService _favoriteTestService;
        private readonly ParticipantAssignedTestService _assignedTestService;
        private readonly ParticipantPublicTestService _publicTestService;
        private readonly IServiceProvider _serviceProvider;
        private List<Test> _allTests = new();

        public ObservableCollection<ParticipantTestCardViewModel> Tests { get; } = new();
        public ObservableCollection<Topic> Topics { get; } = new();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set { _selectedTopic = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private DateTime? _selectedDate;
        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private string _testNameFilter = "";
        public string TestNameFilter
        {
            get => _testNameFilter;
            set { _testNameFilter = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private bool _isAssignedToMe = true;
        public bool IsAssignedToMe
        {
            get => _isAssignedToMe;
            set { _isAssignedToMe = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private bool _isPublic;
        public bool IsPublic
        {
            get => _isPublic;
            set { _isPublic = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        private bool _isCompleted;
        public bool IsCompleted
        {
            get => _isCompleted;
            set { _isCompleted = value; OnPropertyChanged(); _ = ApplyFiltersAsync(); }
        }

        public ICommand CardClickCommand { get; }
        public ICommand ClearFiltersCommand { get; }
        public ICommand AddToFavoriteCommand { get; }
        public ICommand RemoveFromFavoriteCommand { get; }
        public ICommand ViewHistoryCommand { get; }

        public PartAllTestViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            TestService testService,
            TopicService topicService,
            AttemptService attemptService,
            ParticipantFavoriteTestService favoriteTestService,
            ParticipantAssignedTestService assignedTestService,
            ParticipantPublicTestService publicTestService,
            IServiceProvider serviceProvider) : base(navigationService, dialogService)
        {
            _testService = testService;
            _topicService = topicService;
            _attemptService = attemptService;
            _favoriteTestService = favoriteTestService;
            _assignedTestService = assignedTestService;
            _publicTestService = publicTestService;
            _serviceProvider = serviceProvider;

            CardClickCommand = new RelayCommand(_ => OnCardClick(_ as ParticipantTestCardViewModel));
            ClearFiltersCommand = new RelayCommand(_ => _ = ClearFiltersAsync());
            AddToFavoriteCommand = new RelayCommand(_ => AddToFavorite(_ as ParticipantTestCardViewModel));
            RemoveFromFavoriteCommand = new RelayCommand(_ => RemoveFromFavorite(_ as ParticipantTestCardViewModel));
            ViewHistoryCommand = new RelayCommand(_ => ViewHistory(_ as ParticipantTestCardViewModel));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await LoadTopicsAsync();
                await LoadTestsAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка инициализации: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadTopicsAsync()
        {
            await _topicService.InitializeAsync();
            Topics.Clear();
            foreach (var topic in _topicService.Topics)
                Topics.Add(topic);
        }

        private async Task LoadTestsAsync()
        {
            try
            {
                _allTests.Clear();
                Tests.Clear();

                await _testService.GetAvailableTestsForParticipantAsync(CurrentUser.Id);
                await _attemptService.GetParticipantAttemptsAsync(CurrentUser.Id);
                await _favoriteTestService.GetParticipantFavoritesAsync(CurrentUser.Id);
                await _assignedTestService.GetParticipantAssignmentsAsync(CurrentUser.Id);
                await _publicTestService.GetPublicTestsForParticipantAsync(CurrentUser.Id);

                _allTests = _testService.AvailableTests.ToList();
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки тестов: {ex.Message}", "Ошибка");
            }
        }

        public async Task ApplyFiltersAsync()
        {
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    var filtered = _allTests.AsEnumerable();

                    if (SelectedTopic != null)
                        filtered = filtered.Where(t => t.TopicId == SelectedTopic.Id);

                    if (SelectedDate.HasValue)
                        filtered = filtered.Where(t => t.DateOfCreating.Date == SelectedDate.Value.Date);

                    if (!string.IsNullOrWhiteSpace(TestNameFilter))
                        filtered = filtered.Where(t => t.Name != null &&
                            t.Name.Contains(TestNameFilter, StringComparison.OrdinalIgnoreCase));

                    var filteredCards = new List<ParticipantTestCardViewModel>();

                    foreach (var test in filtered)
                    {
                        var attempts = _attemptService.Attempts.Where(a => a.TestId == test.Id).ToList();
                        var isFavorite = _favoriteTestService.IsFavorite(test.Id, CurrentUser.Id);
                        var assignment = _assignedTestService.GetAssignment(test.Id, CurrentUser.Id);
                        var publicTest = _publicTestService.GetPublicTest(test.Id, CurrentUser.Id);

                        var card = new ParticipantTestCardViewModel(
                            test, attempts, isFavorite, assignment, publicTest);

                        filteredCards.Add(card);
                    }

                    var assignedAndFavorite = filteredCards
                        .Where(c => c.IsAssigned && c.IsFavorite)
                        .OrderByDescending(c => c.Test.DateOfCreating)
                        .ToList();

                    var favoriteOnly = filteredCards
                        .Where(c => c.IsFavorite && !c.IsAssigned)
                        .OrderByDescending(c => c.Test.DateOfCreating)
                        .ToList();

                    var assignedOnly = filteredCards
                        .Where(c => c.IsAssigned && !c.IsFavorite)
                        .OrderByDescending(c => c.Test.DateOfCreating)
                        .ToList();

                    var regular = filteredCards
                        .Where(c => !c.IsAssigned && !c.IsFavorite)
                        .OrderByDescending(c => c.Test.DateOfCreating)
                        .ToList();

                    var sortedCards = new List<ParticipantTestCardViewModel>();
                    sortedCards.AddRange(assignedAndFavorite);
                    sortedCards.AddRange(favoriteOnly);
                    sortedCards.AddRange(assignedOnly);
                    sortedCards.AddRange(regular);

                    Tests.Clear();
                    foreach (var card in sortedCards)
                        Tests.Add(card);
                }
                catch (Exception ex)
                {
                    _dialogService.ShowMessage($"Ошибка фильтрации: {ex.Message}", "Ошибка");
                }
            });
        }

        private async Task ClearFiltersAsync()
        {
            SelectedTopic = null;
            SelectedDate = null;
            TestNameFilter = "";
            IsAssignedToMe = false;
            IsPublic = false;
            IsCompleted = false;
            await ApplyFiltersAsync();
        }

        private async void OnCardClick(ParticipantTestCardViewModel? card)
        {
            if (card == null || !card.CanStart) return;

            if (card.Test != null)
            {
                IsLoading = true;
                var viewModel = ActivatorUtilities.CreateInstance<PassingTestViewModel>(
                    _serviceProvider,
                    card.Test);

                _navigationService.NavigateTo(viewModel);
                IsLoading = false;
            }
        }

        private async void AddToFavorite(ParticipantTestCardViewModel? card)
        {
            if (card == null) return;
            await _favoriteTestService.AddToFavoritesAsync(CurrentUser.Id, card.Test.Id);
            card.IsFavorite = true;

            await ApplyFiltersAsync();
        }

        private async void RemoveFromFavorite(ParticipantTestCardViewModel? card)
        {
            if (card == null) return;
            await _favoriteTestService.RemoveFromFavoritesAsync(CurrentUser.Id, card.Test.Id);
            card.IsFavorite = false;

            await ApplyFiltersAsync();
        }

        private void ViewHistory(ParticipantTestCardViewModel? card)
        {
            if (card == null) return;
            _dialogService.ShowMessage($"История прохождений теста \"{card.Test.Name}\"", "История");
        }
    }
}