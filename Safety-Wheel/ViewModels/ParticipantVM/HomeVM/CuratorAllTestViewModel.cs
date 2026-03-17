using Safety_Wheel.Models;
using Safety_Wheel.Services;
using Safety_Wheel.ViewModels.CreateTestsVM;
using System.Collections.ObjectModel;
using System.Windows;


namespace Safety_Wheel.ViewModels.ParticipantVM.HomeVM
{
    class CuratorAllTestViewModel : ObservableObject
    {
        private readonly TestService _testService = new();
        private readonly Topic _subject;

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new ObservableCollection<TestListItemViewModel>();

        public CuratorAllTestViewModel(Topic? subject = null)
        {
            _subject = subject;
            LoadTests();
        }

        private void LoadTests()
        {
            Tests.Clear();
            Tests.Add(new TestListItemViewModel(null));
            if (_subject == null)
            {
                _testService.GetAll(null, CurrentUser.Id);
            }
            else
            {
                _testService.GetTestsByTopicId(_subject.Id, CurrentUser.Id);
            }

            foreach (var test in _testService.Tests)
                Tests.Add(new TestListItemViewModel(test, _testService));
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
