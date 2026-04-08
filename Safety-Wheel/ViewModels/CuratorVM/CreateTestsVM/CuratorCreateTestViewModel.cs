using CozyTest.Models;
using CozyTest.Services;
using CozyTest.ViewModels.CuratorVM;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class CuratorCreateTestViewModel : BaseViewModel
    {
        public bool IsEditMode { get; }
        public Test Test { get; set; }

        public ObservableCollection<QuestionCreateViewModel> Questions { get; } = new();

        private readonly TopicService _topicService = new();
        public ObservableCollection<Topic> Topics => _topicService.Topics;

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();
                if (value != null) Test.TopicId = value.Id;
            }
        }

        public ICommand AddQuestionCommand { get; }
        public ICommand RemoveQuestionCommand { get; }
        public ICommand SaveTestCommand { get; }

        private readonly TestService _testService = new();
        private readonly QuestionService _questionService = new();
        private readonly OptionService _optionService = new();

        public CuratorCreateTestViewModel(INavigationService navigationService, IDialogService dialogService)
            : base(navigationService, dialogService)
        {
            IsEditMode = false;
            Test = new Test();
            AddQuestionCommand = new RelayCommand(_ => AddNewQuestion());
            RemoveQuestionCommand = new RelayCommand(question => RemoveQuestion(question));
            SaveTestCommand = new RelayCommand(_ => Save());
        }

        public CuratorCreateTestViewModel(INavigationService navigationService, IDialogService dialogService, Test test)
            : base(navigationService, dialogService)
        {
            IsEditMode = true;
            LoadTestForEdit(test);
            AddQuestionCommand = new RelayCommand(_ => AddNewQuestion());
            RemoveQuestionCommand = new RelayCommand(question => RemoveQuestion(question));
            SaveTestCommand = new RelayCommand(_ => Save());
        }

        private void AddNewQuestion()
        {
            Questions.Add(new QuestionCreateViewModel());
        }

        public void RemoveQuestion(object parameter)
        {
            if (parameter is QuestionCreateViewModel question)
            {
                var result = MessageBox.Show($"Вы уверены, что хотите удалить вопрос?",
                                             "Подтверждение удаления",
                                             MessageBoxButton.YesNo,
                                             MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    Questions.Remove(question);

                    if (question.NewQuestion.Id != 0 && IsEditMode)
                    {
                        _questionService.Remove(question.NewQuestion);
                    }
                }
            }
        }

        public void Save()
        {
            if (SelectedTopic == null)
            {
                MessageBox.Show("Выберите тему");
                return;
            }

            if (!Questions.Any())
            {
                MessageBox.Show("Добавьте хотя бы один вопрос");
                return;
            }

            foreach (var q in Questions)
            {
                if (string.IsNullOrWhiteSpace(q.Text))
                {
                    MessageBox.Show("Заполните текст всех вопросов");
                    return;
                }

                if (!q.Options.Any())
                {
                    MessageBox.Show($"Добавьте варианты ответа к вопросу");
                    return;
                }

                if (!q.Options.Any(o => o.IsCorrect == true))
                {
                    MessageBox.Show($"Отметьте правильный ответ в вопросе");
                    return;
                }

                foreach (var opt in q.Options.Where(o => o.IsImage && string.IsNullOrEmpty(o.Value)))
                {
                    MessageBox.Show($"Выберите изображение для варианта ответа");
                    return;
                }
            }

            if (IsEditMode)
            {
                UpdateTest();
            }
            else
            {
                CreateTest();
            }
        }

        private void CreateTest()
        {
            _testService.Add(Test, Questions.Count);

            int qi = 1;
            foreach (var qvm in Questions)
            {
                qvm.NewQuestion.TestId = Test.Id;
                qvm.NewQuestion.Number = qi++;

                var savedQuestion = _questionService.Add(qvm.NewQuestion, _testService.GetLastTest(), (int)qvm.NewQuestion.Number);

                int oi = 1;
                foreach (var opt in qvm.Options)
                {
                    opt.NewOption.QuestionId = savedQuestion.Id;
                    opt.NewOption.Number = oi++;
                    _optionService.Add(opt.NewOption, (int)opt.NewOption.Number);
                }
            }

            MessageBox.Show("Тест сохранён");
        }

        private void UpdateTest()
        {
            Test.PenaltyMax = Questions.Count;
            _testService.Update(Test);

            var dbQuestions = _questionService.GetQuestiosForCurrentTest(Test.Id).ToList();

            int qi = 1;
            foreach (var qvm in Questions)
            {
                qvm.NewQuestion.TestId = Test.Id;
                qvm.NewQuestion.Number = qi++;

                Question savedQuestion = qvm.NewQuestion.Id == 0
                    ? _questionService.Add(qvm.NewQuestion, Test, qi - 1)
                    : UpdateExistingQuestion(qvm);

                SyncOptions(qvm, savedQuestion);
            }

            foreach (var dbQ in dbQuestions.Where(dq => !Questions.Any(q => q.NewQuestion.Id == dq.Id)))
            {
                _questionService.Remove(dbQ);
            }

            MessageBox.Show("Тест обновлён");
        }

        private Question UpdateExistingQuestion(QuestionCreateViewModel qvm)
        {
            _questionService.Update(qvm.NewQuestion);
            return qvm.NewQuestion;
        }

        private void SyncOptions(QuestionCreateViewModel qvm, Question savedQuestion)
        {
            var dbOptions = _optionService.GetOptionsByQuestion(savedQuestion.Id).ToList();
            int oi = 1;

            foreach (var opt in qvm.Options)
            {
                opt.NewOption.QuestionId = savedQuestion.Id;
                opt.NewOption.Number = oi++;

                if (opt.NewOption.Id == 0)
                    _optionService.Add(opt.NewOption, (int)opt.NewOption.Number);
                else
                    _optionService.Update(opt.NewOption);
            }

            foreach (var dbOpt in dbOptions.Where(d => !qvm.Options.Any(o => o.NewOption.Id == d.Id)))
            {
                _optionService.Remove(dbOpt);
            }
        }

        private void LoadTestForEdit(Test test)
        {
            Test = test;
            SelectedTopic = Topics.FirstOrDefault(s => s.Id == test.TopicId);
            Questions.Clear();

            var questions = _questionService.GetQuestiosForCurrentTest(test.Id);
            foreach (var q in questions)
            {
                var qvm = new QuestionCreateViewModel(q);

                var options = _optionService.GetOptionsByQuestion(q.Id);
                foreach (var opt in options)
                {
                    bool isImage = !string.IsNullOrEmpty(opt.TextAnswer) &&
                                  (opt.TextAnswer.StartsWith("Images/") || opt.TextAnswer.Contains(".jpg") || opt.TextAnswer.Contains(".png"));

                    qvm.Options.Add(new OptionCreateViewModel(isImage, qvm) { NewOption = opt });
                }

                Questions.Add(qvm);
            }
        }
    }
}