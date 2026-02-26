using Safety_Wheel.Models;
using Safety_Wheel.Pages.Teacher;
using Safety_Wheel.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Safety_Wheel.ViewModels.CreateTestsVM
{
    public class TeacherAllTestViewModel : ObservableObject
    {
        private readonly TestService _testService = new();
        private readonly Subject _subject;

        public ObservableCollection<TestListItemViewModel> Tests { get; } = new ObservableCollection<TestListItemViewModel>();

        public TeacherAllTestViewModel(Subject? subject = null)
        {
            _subject = subject;
            LoadTests();
        }

        private void LoadTests()
        {
            Tests.Clear();

            if (_subject == null)
            {
                _testService.GetAll(null, CurrentUser.Id);
            }
            else
            {
                _testService.GetTestsBySubjectId(_subject.Id, CurrentUser.Id);
            }

            foreach (var test in _testService.Tests)
                Tests.Add(new TestListItemViewModel(test, _testService));

            Tests.Add(new TestListItemViewModel());
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
