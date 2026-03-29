using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CreateTestsVM;
using System.Collections.ObjectModel;
using System.Windows;

namespace CozyTest.ViewModels.ParticipantVM
{
    public class PartAllTestViewModel : ObservableObject
    {
        private readonly TestService _testService = new();
        private readonly Topic? _subject;

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new ObservableCollection<TestListItemViewModel>();

        public PartAllTestViewModel(Topic? subject = null)
        {
            _subject = subject;
            LoadTests();
        }

        private void LoadTests()
        {
            Tests.Clear();
            if (_subject == null)
            {
                _testService.GetAllForParticipants(CurrentUser.Id);
            }
            else
            {
            }

            foreach (var test in _testService.Tests)
                Tests.Add(new TestListItemViewModel(test, _testService));
        }

        public void RemoveTest(Test test)
        {
            var result = MessageBox.Show(
                $"Удалить тест «{test.Name}»?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            _testService.Remove(test);

            var item = Tests.FirstOrDefault(x => x.Test == test);
            if (item != null)
                Tests.Remove(item);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }
    }
}
