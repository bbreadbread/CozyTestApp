using Microsoft.Extensions.Options;
using Notifications.Wpf;
using Safety_Wheel.Models;
using Safety_Wheel.Pages.Participant.DTestTypes;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Safety_Wheel.Pages.Participant
{
    /// <summary>
    /// Логика взаимодействия для PartTest.xaml
    /// </summary>
    public partial class PartTest : Page, INotifyPropertyChanged
    {
        public static bool _isTestActivated;
        private bool _currentQuestionClosed;
        public static bool _canClosed = false;
        public static Test _test { get; set; } = new ();
        private List<Question> _questions;
        private int _currentQuestionIndex = 0;
        public static Attempt _attempt = new();
        private AttemptService _attemptService = new();
        private ParticipantAnswerService participantAnswerService = new();
        private DTestTypeService testTypeService = new();
        public string NameTest { get; set; }
        public string TopicName { get; set; }
        public string ActionStatus { get; set; }

        private string _timeLimit;
        private string _commentForQuestion;
        private string _typeTest;
        private int _correctCount;
        private int _uncorrectCount;
        private int _tmptyCount;

        
        public static DispatcherTimer _timer = new();
        private DateTime _startTime;
        private int? _timeLimitSeconds;

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public PartTest(Test currentTest, int? seconds = null, bool? iamisteacher = false, Attempt atReady = null)
        {
            _test = currentTest;
            _timeLimitSeconds = seconds;

            if (iamisteacher == false)
            {
                _canClosed = false;
                _isTestActivated = true;

                InitializeTimer();
                ActionStatus = "Прохождение теста: ";
                _attempt = new Attempt
                {
                    ParticipantId = CurrentUser.Id,
                    TestId = _test.Id,
                    StartedAt = DateTime.Now,
                    //FinishedAt
                    //Score
                    Status = "В работе"
                };
            }
            else
            {
                _attempt = atReady;
                string timeStr = _attempt.FinishedAt?.ToString("d") ?? "--:--:--";
                ActionStatus = $"Результаты теста: {_attempt.Participant.Name} / {timeStr} / ";
                TimeLimit = (_attempt.FinishedAt - _attempt.StartedAt)?.ToString(@"mm\:ss") ?? "--:--";
            }

            //TypeTest = testTypeService.GetTypeById(_attempt.DTestType).Name;

            NameTest = _test.Name;
            TopicName = _test.Topic.Name;
            _startTime = DateTime.Now;

            DataContext = this;

            _questions = participantAnswerService.GetQuestiosForCurrentTest(_test.Id);

            InitializeComponent();
            LoadQuestionNumbers();
            if (iamisteacher == true)
            {
                CompleteTest();
                HowManyCorrect();
                ButtonConfirm.Visibility = Visibility.Collapsed;
            }
            else
            {
                //if (_attempt.DTestType == 1)
                //    CommentsImage.Visibility = Visibility.Visible;
                //else if (_attempt.DTestType == 3 || _attempt.DTestType == 2)
                //    CommentsImage.Visibility = Visibility.Collapsed;
            }
            LoadCurrentQuestion();

            StudDTestTypeOne.QuestionAnswered += closed => _currentQuestionClosed = closed;
        }
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        private void InitializeTimer()
        {
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateTimeDisplay();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_isTestActivated == false)
            {
                _timer.Stop();
                
                _attempt.Score = HowManyCorrect();
                _attempt.Status = "Завершен (Принудительный выход)";
                _attempt.FinishedAt = DateTime.Now;
                CompleteTest();

                _attemptService.Update(_attempt);
            }
            UpdateTimeDisplay();

            if (_timeLimitSeconds.HasValue)
            {
                var elapsed = DateTime.Now - _startTime;
                if (elapsed.TotalSeconds >= _timeLimitSeconds.Value)
                {
                    TimeExpired();
                }
            }
        }

        private void UpdateTimeDisplay()
        {
            var elapsed = DateTime.Now - _startTime;

                if (_timeLimitSeconds.HasValue)
                {
                    int remainingSeconds = Math.Max(0, _timeLimitSeconds.Value - (int)elapsed.TotalSeconds);
                    TimeLimit = $"{remainingSeconds / 60:00}:{remainingSeconds % 60:00}";

                    if (TimeTextBlock != null)
                    {
                        if (remainingSeconds <= 10) TimeTextBlock.Foreground = Brushes.Red;
                        else if (remainingSeconds <= 30) TimeTextBlock.Foreground = Brushes.Gold;
                        else TimeTextBlock.Foreground = Brushes.Black;
                    }
                }
                else
                {
                    TimeLimit = $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
                }

            OnPropertyChanged(nameof(TimeLimit)); 
        }

        private void TimeExpired()
        {
            _timer.Stop();

            MessageBox.Show("Время вышло! Тест будет автоматически завершен.",
                           "Время истекло",
                           MessageBoxButton.OK,
                           MessageBoxImage.Warning);


            _attempt.Score = HowManyCorrect();
            _attemptService.Update(_attempt);
            _attempt.Status = "Завершен (время истекло)";
            _attempt.FinishedAt = DateTime.Now;
            CompleteTest();
        }

        private void CompleteTest()
        {
            CorrectCount = 0;
            UncorrectCount = 0;
            EmptyCount = 0;

            _canClosed = true;
            ButtonConfirm.Content = "Завершить тест";
            ButtonConfirm.IsEnabled = true;

            InfoResult.Visibility = Visibility.Visible;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void LoadQuestionNumbers()
        {
            GeneratedQuestionsPanel.Children.Clear();

            for (int i = 0; i < _questions.Count; i++)
            {
                int questionIndex = i;

                Button itemButton = new Button
                {
                    Tag = questionIndex,
                    Content = $"{i + 1}",
                    Style = (Style)FindResource("QuestItemButton")
                };
                itemButton.Click += (s, e) =>
                {

                    if (s is Button clickedButton)
                    {
                        _currentQuestionIndex = questionIndex;
                        LoadCurrentQuestion();
                    }
                        
                };

                GeneratedQuestionsPanel.Children.Add(itemButton);
            }
        }
        private void LoadCurrentQuestion(bool? last = null)
        {
            StudDTestTypeSecond._participantAnswersListTypeSecond.Clear();
            _currentQuestionClosed = false;
            UpdateConfirmButtonStyle();
            if (last == null)
            {
                var currentQuestion = _questions[_currentQuestionIndex];

                int questionType = GetQuestionType(currentQuestion);

                CommentForQuestion = currentQuestion.Comments;
                OnPropertyChanged(nameof(CommentForQuestion));

                switch (questionType)
                {
                    case 1:
                        TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion));
                        break;

                    case 2:
                        TestContentFrame.Navigate(new StudDTestTypeSecond(currentQuestion));
                        break;

                    case 3:
                        TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion, true));
                        break;

                    default:
                        MessageBox.Show($"Неизвестный тип вопроса: {questionType}");
                        TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion));
                        break;
                }

            }
            else
            {
                if (_currentQuestionIndex + 1 < _questions.Count)
                {

                    var currentQuestion = _questions[_currentQuestionIndex + 1];

                    int questionType = GetQuestionType(currentQuestion);

                    switch (questionType)
                    {
                        case 1:
                            TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion));
                            break;

                        case 2:
                            TestContentFrame.Navigate(new StudDTestTypeSecond(currentQuestion));
                            break;

                        case 3:
                            TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion, true));
                            break;

                        default:
                            MessageBox.Show($"Неизвестный тип вопроса: {questionType}");
                            TestContentFrame.Navigate(new StudDTestTypeOne(currentQuestion));
                            break;
                    }
                    _currentQuestionIndex++;
                }

            }
            UpdateQuestionSelection();
        }

        private void UpdateQuestionSelection()
        {
            for (int i = 0; i < GeneratedQuestionsPanel.Children.Count; i++)
            {
                if (GeneratedQuestionsPanel.Children[i] is Button button)
                {
                    if (i == _currentQuestionIndex)
                    {
                        button.Background = Brushes.LightBlue;
                        button.Foreground = Brushes.Black;
                    }
                    else
                    {
                        button.Background = Brushes.White;
                        button.Foreground = Brushes.Black;
                    }
                }
            }
        }

        private int GetQuestionType(Question question)
        {
            if (question.QuestionTypeId.HasValue)
            {
                return (int)question.QuestionTypeId.Value;
            }

            return 1; 
        }

        private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (_canClosed == true){ NavigationService.Navigate(new PartHomePage()); return; }
            if (_currentQuestionClosed) 
                return;

            var currentQuestion = _questions[_currentQuestionIndex];
            int questionType = GetQuestionType(currentQuestion);

            switch (questionType)
            {
                case 1:
                    if (StudDTestTypeOne._participantAnswer.QuestionId != currentQuestion.Id) return;
                    participantAnswerService.Add(StudDTestTypeOne._participantAnswer);
                    if (GeneratedQuestionsPanel.Children[_currentQuestionIndex] is Button currentButton)
                    {
                        currentButton.Style = (Style)FindResource("DownQuestItemButton");
                    }
                    break;

                case 2:
                    if (StudDTestTypeSecond._participantAnswersListTypeSecond.Count == 0) return;

                    foreach (var an in StudDTestTypeSecond._participantAnswersListTypeSecond)
                        participantAnswerService.Add(an);

                    StudDTestTypeSecond._participantAnswersListTypeSecond.Clear();
                    if (GeneratedQuestionsPanel.Children[_currentQuestionIndex] is Button currentButton2)
                    {
                        currentButton2.Style = (Style)FindResource("DownQuestItemButton");
                    }
                    break;

                case 3:
                    if (StudDTestTypeOne._participantAnswersListTypeThree.Count == 0) return;

                    foreach (var an in StudDTestTypeOne._participantAnswersListTypeThree)
                        participantAnswerService.Add(an);

                    StudDTestTypeOne._participantAnswersListTypeThree.Clear();
                    if (GeneratedQuestionsPanel.Children[_currentQuestionIndex] is Button currentButton3)
                    {
                        currentButton3.Style = (Style)FindResource("DownQuestItemButton");
                    }
                    break;

                default:
                    MessageBox.Show($"Неизвестный тип вопроса: {questionType}");
                    break;
            }

            participantAnswerService.Commit();

            if (participantAnswerService.IsReady(_attempt, _test))
            {
                _timer.Stop();
                _canClosed = true;

                ButtonConfirm.Content = "Завершить тест";
                CommentsImage.Visibility = Visibility.Visible;

                _attempt.FinishedAt = DateTime.Now;
                _attempt.Score = HowManyCorrect();
                _attempt.Status = "Завершен";
                _attemptService.Update(_attempt);
            }
            else
            {
                LoadCurrentQuestion(true);
            }
        }

        private void UpdateConfirmButtonStyle()
        {
            if (_currentQuestionClosed)
                ButtonConfirm.Style = (Style)FindResource("ConfirmButtonDisenabled");
            else
                ButtonConfirm.Style = (Style)FindResource("ConfirmButton");
        }

        private int HowManyCorrect()
        {
            var questionIds = _questions.Select(q => q.Id).ToList();

            var AllCorrectness = participantAnswerService.GetAllQuestionCorrectness(_attempt, questionIds);

            for (int i = 0; i < _questions.Count; i++)
            {
                if (GeneratedQuestionsPanel.Children[i] is Button btn)
                {
                    var questionId = _questions[i].Id;

                    if (AllCorrectness.TryGetValue(questionId, out var correctness))
                    {
                        string styleKey;

                        if (correctness == null)
                        {
                            styleKey = "GoldQuestItemButton";
                            EmptyCount++;
                        }
                        else if (correctness.Value)
                        {
                            styleKey = "GreenQuestItemButton";
                            CorrectCount++;
                        }
                        else
                        {
                            styleKey = "RedQuestItemButton";
                            UncorrectCount++;
                        }

                        btn.Style = (Style)FindResource(styleKey);
                    }
                }
            }

            return CorrectCount;
        }

        private void CommentsImageClick_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var image = sender as Image;
            var toolTip = image?.ToolTip as ToolTip;
            if (toolTip == null) return;

            if (toolTip.DataContext == null)
                toolTip.DataContext = image.DataContext;

            toolTip.IsOpen = !toolTip.IsOpen;
        }
        public int CorrectCount
        {
            get => _correctCount;
            set
            {
                _correctCount = value;
                OnPropertyChanged(nameof(CorrectCount));
            }
        }
        public int UncorrectCount
        {
            get => _uncorrectCount;
            set
            {
                _uncorrectCount = value;
                OnPropertyChanged(nameof(UncorrectCount));
            }
        }

        public int EmptyCount
        {
            get => _tmptyCount;
            set
            {
                _tmptyCount = value;
                OnPropertyChanged(nameof(EmptyCount));
            }
        }

        public string TimeLimit
        {
            get => _timeLimit;
            set
            {
                _timeLimit = value;
                OnPropertyChanged(nameof(TimeLimit));
            }
        }
                
        public string CommentForQuestion
        {
            get => _commentForQuestion;
            set
            {
                _commentForQuestion = value;
                OnPropertyChanged(nameof(CommentForQuestion));
            }
        }
                
        public string TypeTest
        {
            get => _typeTest;
            set
            {
                _typeTest = value;
                OnPropertyChanged(nameof(TypeTest));
            }
        }
    }
}
