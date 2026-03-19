using CozyTest.Models;
using CozyTest.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace CozyTest.ViewModels.CreateTestsVM
{
    public class CuratorCreateTestViewModel : ObservableObject
    {
        public bool IsEditMode { get; }
        public Test Test { get; set; }

        public ObservableCollection<QuestionCreateViewModel> Questions { get; }
            = new();

        private readonly TopicService _subjectService = new();
        public ObservableCollection<Topic> Topics => _subjectService.Topics;

        private Topic? _selectedTopic;
        public Topic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                _selectedTopic = value;
                OnPropertyChanged();

                if (value != null)
                    Test.TopicId = value.Id;
            }
        }

        private readonly TestService _testService = new();
        private readonly QuestionService _questionService = new();
        private readonly OptionService _optionService = new();

        public CuratorCreateTestViewModel()
        {
            IsEditMode = false;
            Test = new Test();
            AddGhostQuestion();
        }

        public CuratorCreateTestViewModel(Test test)
        {
            IsEditMode = true;
            LoadTestForEdit(test);
        }

        private void AddGhostQuestion()
        {
            Questions.Add(new QuestionCreateViewModel(new Question(), true, OnQuestionActivated));
        }

        private void OnQuestionActivated()
        {
            if (Questions.Any(q => q.IsGhost))
                return;

            AddGhostQuestion();
        }

        public void Save()
        {
            if (SelectedTopic == null)
            {
                MessageBox.Show("Выберите дисциплину");
                return;
            }

            var questionsToSave = Questions
                .Where(q => !q.IsGhost)
                .ToList();

            if (!questionsToSave.Any())
            {
                MessageBox.Show("Добавьте хотя бы один вопрос");
                return;
            }

            foreach (var q in questionsToSave)
            {
                var realOptions = q.Options
                    .Where(o => !o.IsGhost)
                    .ToList();

                if (!realOptions.Any())
                {
                    MessageBox.Show($"В вопросе №{questionsToSave.IndexOf(q) + 1} нет вариантов ответа");
                    return;
                }

                if (!realOptions.Any(o => o.NewOption.IsCorrect == true))
                {
                    MessageBox.Show($"В вопросе №{questionsToSave.IndexOf(q) + 1} нет правильного ответа");
                    return;
                }
            }

            if (IsEditMode)
            {
                var dbQuestions = _questionService
                    .GetQuestiosForCurrentTest(Test.Id)
                    .ToList();

                //Test.MaxNumPassing = questionsToSave.Count;
                Test.PenaltyMax = questionsToSave.Count;

                _testService.Update(Test);

                int qj = 1;

                foreach (var qvm in questionsToSave)
                {
                    var vmOptions = qvm.Options
                        .Where(o => !o.IsGhost)
                        .ToList();

                    qvm.NewQuestion.TestId = Test.Id;
                    qvm.NewQuestion.Number = qj++;

                    if (qvm.NewQuestion.QuestionTypeId == 2)
                        qvm.NewQuestion.PicturePath = null;

                    Question savedQuestion;

                    if (qvm.NewQuestion.Id == 0)
                    {
                        savedQuestion = _questionService.Add(
                            qvm.NewQuestion,
                            Test,
                            (int)qvm.NewQuestion.Number);
                    }
                    else
                    {
                        _questionService.Update(qvm.NewQuestion);
                        savedQuestion = qvm.NewQuestion;
                    }

                    var dbOptions = _optionService
                        .GetOptionsByQuestion(savedQuestion.Id)
                        .ToList();

                    int oi = 1;

                    foreach (var ovm in vmOptions)
                    {
                        ovm.NewOption.QuestionId = savedQuestion.Id;
                        ovm.NewOption.Number = oi++;

                        if (ovm.NewOption.Id == 0)
                            _optionService.Add(ovm.NewOption, (int)ovm.NewOption.Number);
                        else
                            _optionService.Update(ovm.NewOption);
                    }

                    foreach (var dbOpt in dbOptions)
                    {
                        if (!vmOptions.Any(o => o.NewOption.Id == dbOpt.Id))
                            _optionService.Remove(dbOpt);
                    }
                }

                foreach (var dbQ in dbQuestions)
                {
                    if (!questionsToSave.Any(q => q.NewQuestion.Id == dbQ.Id))
                        _questionService.Remove(dbQ);
                }

                MessageBox.Show("Тест обновлён");
            }
            else
            {
                _testService.Add(Test, questionsToSave.Count);

                int qi = 1;

                foreach (var q in questionsToSave)
                {
                    q.NewQuestion.TestId = Test.Id;
                    if (q.NewQuestion.QuestionTypeId == 2)
                        q.NewQuestion.PicturePath = null;

                    var savedQuestion = _questionService.Add(
                        q.NewQuestion,
                        _testService.GetLastTest(),
                        qi++);

                    var realOptions = q.Options.Where(o => !o.IsGhost).ToList();
                    int oi = 1;

                    foreach (var o in realOptions)
                    {
                        o.NewOption.QuestionId = savedQuestion.Id;
                        o.NewOption.Number = oi++;
                        _optionService.Add(o.NewOption, (int)o.NewOption.Number);
                    }
                }

                MessageBox.Show("Тест сохранён");
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
                var qvm = new QuestionCreateViewModel(q, false, OnQuestionActivated);

                qvm.Options.Clear();

                var options = _optionService.GetOptionsByQuestion(q.Id);

                foreach (var opt in options)
                {
                    qvm.Options.Add(new OptionCreateViewModel(
                        false,
                        q.QuestionTypeId == 2,
                        qvm)
                    {
                        NewOption = opt
                    });
                }

                qvm.SyncGhostOptions();
                Questions.Add(qvm);
            }

            AddGhostQuestion();
        }
    }
}
