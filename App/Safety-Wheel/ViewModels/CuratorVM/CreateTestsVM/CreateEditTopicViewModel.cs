using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM.AdministrationVM;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CuratorVM.CreateTestsVM
{
    public class CreateEditTopicViewModel : BaseViewModel
    {
        public override string WindowTitle => "Управление темами";

        private readonly TopicService _topicService;
        private readonly IDialogService _dialogService;

        private ObservableCollection<Topic> _topics;
        public ObservableCollection<Topic> Topics
        {
            get => _topics;
            set { _topics = value; OnPropertyChanged(); }
        }

        private ObservableCollection<Topic> _filteredTopics;
        public ObservableCollection<Topic> FilteredTopics
        {
            get => _filteredTopics;
            set { _filteredTopics = value; OnPropertyChanged(); }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterTopics();
            }
        }

        private string _topicName;
        public string TopicName
        {
            get => _topicName;
            set
            {
                _topicName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanUpdate));
            }
        }

        private Topic _selectedTopic;
        public Topic SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();
                if (value != null)
                {
                    TopicName = value.Name;
                }
                OnPropertyChanged(nameof(CanUpdate));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public bool CanUpdate => SelectedTopic != null && !string.IsNullOrWhiteSpace(TopicName);

        public ICommand AddTopicCommand { get; }
        public ICommand UpdateTopicCommand { get; }
        public ICommand DeleteTopicCommand { get; }

        public CreateEditTopicViewModel(
            INavigationService navigationService,
            IDialogService dialogService,
            TopicService topicService, ILoggingService logger) : base(navigationService, dialogService, logger)
        {
            _topicService = topicService;
            _dialogService = dialogService;

            AddTopicCommand = new RelayCommand(_ => AddTopic());
            UpdateTopicCommand = new RelayCommand(_ => UpdateTopic());
            DeleteTopicCommand = new RelayCommand(_ => DeleteTopic(_ as Topic));

            _ = LoadTopicsAsync();
        }

        private async Task LoadTopicsAsync()
        {
            try
            {
                IsLoading = true;
                await _topicService.InitializeAsync();

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    Topics = new ObservableCollection<Topic>(_topicService.Topics);
                    FilteredTopics = new ObservableCollection<Topic>(Topics);
                });
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка загрузки тем: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterTopics()
        {
            if (Topics == null) return;

            var filtered = Topics.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                filtered = filtered.Where(t => t.Name != null &&
                    t.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            }

            FilteredTopics = new ObservableCollection<Topic>(filtered);
        }

        private async void AddTopic()
        {
            if (string.IsNullOrWhiteSpace(TopicName))
            {
                 _dialogService.ShowMessage("Введите название темы", "Предупреждение");
                return;
            }

            try
            {
                IsLoading = true;

                bool exists = Topics.Any(t => t.Name != null &&
                    t.Name.Equals(TopicName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    _dialogService.ShowMessage("Тема с таким названием уже существует", "Ошибка");
                    return;
                }

                var newTopic = new Topic
                {
                    Name = TopicName.Trim()
                };

                await _topicService.AddTopicAsync(newTopic);

                Topics.Add(newTopic);
                FilterTopics();

                TopicName = "";
                SelectedTopic = null;

                _dialogService.ShowMessage("Тема успешно добавлена", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка добавления темы: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void UpdateTopic()
        {
            if (SelectedTopic == null)
            {
                _dialogService.ShowMessage("Выберите тему для обновления", "Предупреждение");
                return;
            }

            if (string.IsNullOrWhiteSpace(TopicName))
            {
                _dialogService.ShowMessage("Введите название темы", "Предупреждение");
                return;
            }

            try
            {
                IsLoading = true;

                bool exists = Topics.Any(t => t.Id != SelectedTopic.Id &&
                    t.Name != null &&
                    t.Name.Equals(TopicName.Trim(), StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    _dialogService.ShowMessage("Тема с таким названием уже существует", "Ошибка");
                    return;
                }

                SelectedTopic.Name = TopicName.Trim();
                await _topicService.UpdateTopicAsync(SelectedTopic);

                await LoadTopicsAsync();

                TopicName = "";
                SelectedTopic = null;

                _dialogService.ShowMessage("Тема успешно обновлена", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка обновления темы: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool CanDeleteTopic(object parameter)
        {
            var topic = parameter as Topic;
            if (topic == null) return false;

            return !_topicService.HasTestsWithTopic(topic.Id);
        }

        private async void DeleteTopic(Topic? topic)
        {
            if (topic == null)
            {
                _dialogService.ShowMessage("Выберите тему для удаления", "Предупреждение");
                return;
            }

            bool hasTests = _topicService.HasTestsWithTopic(topic.Id);

            if (hasTests)
            {
                _dialogService.ShowMessage("Нельзя удалить тему, к которой привязаны тесты!", "Ошибка");
                return;
            }

            bool confirm = _dialogService.ShowConfirmation($"Вы уверены, что хотите удалить тему \"{topic.Name}\"?", "Подтверждение удаления");

            if (!confirm) return;

            try
            {
                IsLoading = true;
                await _topicService.DeleteTopicAsync(topic.Id);
                Topics.Remove(topic);
                FilterTopics();
                TopicName = "";
                SelectedTopic = null;
                _dialogService.ShowMessage("Тема успешно удалена", "Успех");
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"Ошибка удаления темы: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}