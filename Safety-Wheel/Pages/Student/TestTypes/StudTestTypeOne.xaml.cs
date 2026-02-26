using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Safety_Wheel.Pages.Student.TestTypes
{
    /// <summary>
    /// Логика взаимодействия для StudTestTypeOne.xaml
    /// </summary>
    public partial class StudTestTypeOne : Page
    {
        public static event Action<bool> QuestionAnswered;
        private readonly Attempt _attempt;

        private Question _question { get; set; }
        private OptionService _serviceOpt { get; set; } = new();
        public static StudentAnswer _studentAnswer { get; set; } = new();
        public static StudentAnswerService _studentAnswerService { get; set; } = new();
        public static List<StudentAnswer> _studentAnswersListTypeThree { get; set; } = new();
        public string PathImage { get; set; }
        public string TextQuestion { get; set; }

        public StudTestTypeOne(Question question, bool? isThreeType = null)
        {
            _attempt = StudTest._attempt;
            _question = question;
            TextQuestion = _question.TestQuest;
            InitializeComponent();

            if (!string.IsNullOrEmpty(_question.PicturePath))
            {
                if (_question.PicturePath != null)
                {
                    PathImage = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _question.PicturePath);
                }

                if (!string.IsNullOrEmpty(_question.PicturePath))
                {
                    PathImage = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, _question.PicturePath.TrimStart('/', '\\'));
                }
            }
            if (isThreeType == null)
                LoadOptions();
            else
                LoadOptions(true);

            CheckAlreadyAnswered();

            if (StudTest._canClosed == true || ((_attempt.TestType == 1 || _attempt.TestType == 2) && _studentAnswerService.GetByQuestionAndAttempt(_question.Id, _attempt.Id) != null))
                ShowAnswers();

            DataContext = this;
        }
        private void LoadOptions(bool? isThreeType = null)
        {
            GeneratedAnswersPanel.Children.Clear();

            _serviceOpt.GetAll(_question.Id);

            for (int i = 1; i <= _serviceOpt.Options.Count; i++)
            {
                if (isThreeType == null)
                    CreateAndAddOption(i.ToString(), _serviceOpt.Options[i - 1]);
                else CreateAndAddOptionTypeThree(i.ToString(), _serviceOpt.Options[i - 1]);
            }
        }
        private void CheckAlreadyAnswered()
        {
            _studentAnswerService.GetAll(_attempt.Id, _question.Id);

            bool answered = _studentAnswerService.StudentAnswers
                                   .Any(sa => sa.AttemptId == _attempt.Id &&
                                              sa.QuestionId == _question.Id);

            if (!answered)
                return;

            QuestionAnswered?.Invoke(true);

            foreach (var st in GeneratedAnswersPanel.Children.OfType<StackPanel>())
            {
                foreach (var btn in st.Children.OfType<Button>())
                {
                    if (btn.Tag is string t && int.TryParse(t, out int num))
                    {
                        if (_studentAnswerService.StudentAnswers.Any(sa => sa.Option.Number == num))
                        {
                            btn.Style = (Style)FindResource("DownOptionButton");
                        }
                        btn.IsEnabled = false;
                    }
                }
            }
        }
        private void ShowAnswers()
        {
            var correctOptionNumbers = _question.Options
                .Where(o => o.IsCorrect == true)
                .Select(o => o.Number)
                .ToList();

            var studentSelectedOptions = _studentAnswerService.StudentAnswers
                .Where(sa => sa.AttemptId == _attempt.Id && sa.QuestionId == _question.Id)
                .Select(sa => sa.Option.Number)
                .ToList();

            bool hasAnswer = studentSelectedOptions.Any();

            if (hasAnswer)
            {
                QuestionAnswered?.Invoke(true);
            }

            foreach (var st in GeneratedAnswersPanel.Children.OfType<StackPanel>())
            {
                foreach (var btn in st.Children.OfType<Button>())
                {
                    if (btn.Tag is string t && int.TryParse(t, out int optionNumber))
                    {
                        bool isCorrectOption = correctOptionNumbers.Contains(optionNumber);
                        bool isStudentSelected = studentSelectedOptions.Contains(optionNumber);

                        if (isStudentSelected)
                        {
                            if (isCorrectOption)
                            {
                                btn.Tag = "SelectedTrue";
                            }
                            else
                            {
                                btn.Tag = "SelectedFalse";
                            }
                        }
                        else
                        {
                            if (isCorrectOption)
                            {
                                btn.Tag = "True";
                            }
                            else
                            {
                                btn.Tag = "False";
                            }
                        }

                        if (hasAnswer)
                        {
                            btn.IsEnabled = false;
                        }
                    }
                }
            }
        }
        private List<Border> _selectedBorders = new List<Border>();
        private Dictionary<Border, ScaleTransform> _borderTransforms = new Dictionary<Border, ScaleTransform>();
        private void CreateAndAddOption(string tagValue, Option option)
        {
            Button button = new()
            {
                Tag = tagValue,
                Content = option.TextAnswer
            };

            button.Style = (Style)FindResource("OptionButton");

            button.Click += (s, e) =>
            {
                if (StudTest._canClosed == false)
                    if (s is Button clickedButton)
                    {
                        ResetAllButtonsStyle();
                        clickedButton.Style = (Style)FindResource("DownOptionButton");
                        if (clickedButton.Tag is string tagValue && int.TryParse(tagValue, out int optionId))
                        {
                            var selectedOption = _question.Options.FirstOrDefault(o => o.Number == optionId);
                            if (selectedOption == null) return;
                            _studentAnswer = new StudentAnswer
                            {
                                QuestionId = _question.Id,
                                OptionId = optionId,
                                IsCorrect = selectedOption.IsCorrect,
                                AnsweredAt = DateTime.Now,
                                Attempt = _attempt,
                                Option = selectedOption,
                                Question = _question
                            };
                        }
                    }
            };

            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(button);

            if (Application.Current.TryFindResource("LeftGravityStackPanel") is Style stackStyle)
                stackPanel.Style = stackStyle;

            GeneratedAnswersPanel.Children.Add(stackPanel);
        }

        private void CreateAndAddOptionTypeThree(string tagValue, Option option)
        {
            Button button = new()
            {
                Tag = tagValue,
                Content = option.TextAnswer
            };

            button.Style = (Style)FindResource("OptionButton");
            ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0);
            button.RenderTransform = scaleTransform;

            Border border = new Border
            {
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5),
                Padding = new Thickness(5),
                CornerRadius = new CornerRadius(5),
                Background = Brushes.White,
                Cursor = Cursors.Hand
            };

            _borderTransforms[border] = scaleTransform;

            button.Click += (s, e) =>
            {
                if (StudTest._canClosed == false)
                    if (s is Button clickedButton)
                    {
                        if (clickedButton.Style == (Style)FindResource("DownOptionButton")) clickedButton.Style = (Style)FindResource("OptionButton");
                        else clickedButton.Style = (Style)FindResource("DownOptionButton");

                        if (clickedButton.Tag is string tagValue && int.TryParse(tagValue, out int optionId))
                        {
                            var selectedOption = _question.Options.FirstOrDefault(o => o.Number == optionId);
                            if (selectedOption == null) return;
                            bool isAlreadySelected = _selectedBorders.Contains(border);

                            if (isAlreadySelected)
                            {
                                _selectedBorders.Remove(border);
                                AnimateBorder(border, 1.0, Brushes.LightGray, 1);
                                _studentAnswersListTypeThree.RemoveAll(sa => sa.OptionId == optionId);
                            }
                            else
                            {
                                _selectedBorders.Add(border);
                                AnimateBorder(border, 0.95, new SolidColorBrush(Color.FromRgb(0, 0, 139)), 2);

                                var newAnswer = new StudentAnswer
                                {
                                    AttemptId = _attempt.Id,
                                    QuestionId = _question.Id,
                                    OptionId = optionId,
                                    IsCorrect = selectedOption.IsCorrect,
                                    AnsweredAt = DateTime.Now,
                                    Attempt = _attempt,
                                    Option = selectedOption,
                                    Question = _question
                                };

                                _studentAnswersListTypeThree.Add(newAnswer);
                            }
                        }
                    }
            };


            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(button);

            if (Application.Current.TryFindResource("LeftGravityStackPanel") is Style stackStyle)
                stackPanel.Style = stackStyle;

            GeneratedAnswersPanel.Children.Add(stackPanel);
        }

        private void ResetAllButtonsStyle()
        {
            foreach (var child in GeneratedAnswersPanel.Children)
            {
                if (child is StackPanel stackPanel && stackPanel.Children.Count > 0)
                {
                    if (stackPanel.Children[0] is Button button)
                    {
                        button.Style = (Style)FindResource("OptionButton");
                    }
                }
            }
        }

        private void AnimateBorder(Border border, double scale, Brush borderBrush, double borderThickness)
        {
            if (_borderTransforms.TryGetValue(border, out ScaleTransform transform))
            {
                DoubleAnimation scaleAnimation = new DoubleAnimation
                {
                    To = scale,
                    Duration = TimeSpan.FromSeconds(0.15),
                    EasingFunction = new QuadraticEase()
                };

                transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
                transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);

                border.BorderBrush = borderBrush;
                border.BorderThickness = new Thickness(borderThickness);
            }
        }

    }
}
