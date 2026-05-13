using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Extensions.Options;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;

namespace Safety_Wheel.Pages.Student.TestTypes
{
    /// <summary>
    /// Логика взаимодействия для StudTestTypeSecond.xaml
    /// </summary>
    public partial class StudTestTypeSecond : Page
    {
        private bool _isLocked;
        public static event Action<bool> QuestionAnswered;
        private readonly Attempt _attempt;

        private Question _question { get; set; }
        private OptionService _serviceOpt { get; set; } = new();
        public static StudentAnswer _studentAnswer { get; set; } = new();
        public static List<StudentAnswer> _studentAnswersListTypeSecond { get; set; } = new();
        public static StudentAnswerService _studentAnswerService { get; set; } = new();
        public string TextQuestion { get; set; }

        public StudTestTypeSecond(Question question)
        {
            _attempt = StudTest._attempt;
            _question = question;
            TextQuestion = _question.TestQuest;

            InitializeComponent();

            LoadOptions();
            CheckAlreadyAnswered();

            if (StudTest._canClosed == true || ((_attempt.TestType == 1 || _attempt.TestType == 2) && _studentAnswerService.GetByQuestionAndAttempt(_question.Id, _attempt.Id) != null))
                ShowAnswers();

            DataContext = this;
        }

        private void LoadOptions()
        {
            GeneratedAnswersPanel.Children.Clear();
            _serviceOpt.GetAll(_question.Id);

            for (int i = 1; i <= _serviceOpt.Options.Count; i++)
            {
                CreateAndAddOption(i.ToString(), _serviceOpt.Options[i - 1]);
            }
        }

        private void CheckAlreadyAnswered()
        {
            _studentAnswerService.GetAll(_attempt.Id, _question.Id);

            var answeredOptions = _studentAnswerService.StudentAnswers
                                      .Where(sa => sa.Option != null)
                                      .Select(sa => sa.Option.Number)
                                      .ToHashSet();

            if (!answeredOptions.Any())
                return;                

            QuestionAnswered?.Invoke(true);
            _isLocked = true;

            foreach (var border in GeneratedAnswersPanel.Children.OfType<Border>())
            {
                var overlay = border.Child as Grid;
                if (overlay?.Children[0] is Image img &&
                    img.Tag is string tg &&
                    int.TryParse(tg, out int num))
                {
                    if (answeredOptions.Contains(num))
                    {
                        _selectedBorders.Add(border);
                        AnimateBorder(border, 0.95,
                                      new SolidColorBrush(Color.FromRgb(0, 0, 139)), 2);

                        var opt = _question.Options.First(o => o.Number == num);
                        //_studentAnswersListTypeSecond.Add(new StudentAnswer
                        //{
                        //    QuestionId = _question.Id,
                        //    OptionId = opt.Id,
                        //    IsCorrect = opt.IsCorrect,
                        //    AnsweredAt = DateTime.Now,
                        //    Attempt = _attempt,
                        //    Option = opt,
                        //    Question = _question
                        //});
                    }

                    img.IsEnabled = false;
                    img.IsHitTestVisible = false;
                    img.Cursor = Cursors.Arrow;

                    if (overlay.Children.Count > 1 && overlay.Children[1] is Image magn)
                    {
                        magn.IsEnabled = true;
                        magn.IsHitTestVisible = true; 
                        magn.Cursor = Cursors.Hand;
                    }

                    border.Cursor = Cursors.Arrow;
                }
            }
        }

        private void ShowAnswers()
        {
            var correctOptionNumbers = _question.Options
                .Where(o => o.IsCorrect == true)
                .Select(o => o.Number)
                .ToHashSet();

            var studentSelectedOptions = _studentAnswerService.StudentAnswers
                .Where(sa => sa.AttemptId == _attempt.Id && sa.QuestionId == _question.Id)
                .Select(sa => sa.Option.Number)
                .ToHashSet();

            bool hasAnswer = studentSelectedOptions.Any();

            if (hasAnswer)
            {
                QuestionAnswered?.Invoke(true);
                _isLocked = true;
            }

            foreach (var border in GeneratedAnswersPanel.Children.OfType<Border>())
            {
                var overlay = border.Child as Grid;
                if (overlay?.Children[1] is Image mag && mag.Tag is string magstr && magstr == "mag")
                {
                    mag.IsHitTestVisible = true;
                    mag.IsEnabled = true;
                }

                if (overlay?.Children[0] is Image img && img.Tag is string tg && int.TryParse(tg, out int optionNumber))
                {
                    bool isCorrectOption = correctOptionNumbers.Contains(optionNumber);
                    bool isStudentSelected = studentSelectedOptions.Contains(optionNumber);

                    if (isStudentSelected)
                    {
                        if (isCorrectOption)
                        {
                            // Выбранный правильный ответ
                            border.BorderBrush = (SolidColorBrush)FindResource("BrightGreen");
                            border.BorderThickness = new Thickness(5);
                            border.Background = (SolidColorBrush)FindResource("Green");
                        }
                        else
                        {
                            // Выбранный неправильный ответ
                            border.BorderBrush = (SolidColorBrush)FindResource("BrightRed");
                            border.BorderThickness = new Thickness(5);
                            border.Background = (SolidColorBrush)FindResource("Red");
                        }
                    }
                    else
                    {
                        if (isCorrectOption)
                        {
                            // Правильный и не выбранный ответ
                            border.BorderBrush = (SolidColorBrush)FindResource("Green");
                            border.BorderThickness = new Thickness(5);
                            border.Background = (SolidColorBrush)FindResource("LightGreen");
                        }
                        else
                        {
                            // Неправильный и не выбранный
                            border.BorderBrush = (SolidColorBrush)FindResource("Red");
                            border.BorderThickness = new Thickness(5);
                            border.Background = (SolidColorBrush)FindResource("LightRed");
                        }
                    }

                    img.IsEnabled = false;
                    img.IsHitTestVisible = false;
                    img.Cursor = Cursors.Arrow;
                    border.Cursor = Cursors.Arrow;
                }
            }
        }


        private List<Border> _selectedBorders = new List<Border>();
        private Dictionary<Border, ScaleTransform> _borderTransforms = new Dictionary<Border, ScaleTransform>();



        private void CreateAndAddOption(string tagValue, Option option)
        {
            string imagePath = System.IO.Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                option.TextAnswer);

            int size = 240;
            Image image = new Image
            {
                Source = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                Tag = tagValue,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Cursor = Cursors.Hand
            };

            ScaleTransform scaleTransform = new ScaleTransform(1.0, 1.0);
            image.RenderTransform = scaleTransform;

            Image magnifier = new Image
            {
                Source = new BitmapImage(new Uri(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images/magnifier_icon.png"), UriKind.RelativeOrAbsolute)),
                Width = 32,
                Height = 32,
                Cursor = Cursors.Hand,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 5, 5, 0),
                Opacity = 0.8,
                Tag = "mag"
            };

            Grid grid = new Grid
            {
                Width = size - 10,
                Height = size - 10,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Background = Brushes.Transparent
            };

            grid.Children.Add(image);

            grid.Children.Add(magnifier);

            Grid.SetColumn(magnifier, 0);
            Grid.SetRow(magnifier, 0);
            magnifier.HorizontalAlignment = HorizontalAlignment.Right;
            magnifier.VerticalAlignment = VerticalAlignment.Top;
            magnifier.Margin = new Thickness(0, 5, 5, 0);

            Border border = new Border
            {
                Child = grid,
                BorderBrush = Brushes.LightGray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(5),
                Background = Brushes.White,
                Cursor = Cursors.Hand
            };

            _borderTransforms[border] = scaleTransform;

            image.MouseDown += (s, e) =>
            {
                if (StudTest._canClosed == false)
                {
                    if (_isLocked) return;
                    if (s is Image img && img.Tag is string tag && int.TryParse(tag, out int optionId))
                    {
                        var selectedOption = _question.Options.FirstOrDefault(o => o.Number == optionId);
                        if (selectedOption == null) return;

                        bool isAlreadySelected = _selectedBorders.Contains(border);

                        if (isAlreadySelected)
                        {
                            _selectedBorders.Remove(border);
                            AnimateBorder(border, 1.0, Brushes.LightGray, 1);
                            _studentAnswersListTypeSecond.RemoveAll(sa => sa.OptionId == optionId);
                        }
                        else
                        {
                            _selectedBorders.Add(border);
                            AnimateBorder(border, 0.95, new SolidColorBrush(Color.FromRgb(0, 0, 139)), 2);
                            var newAnswer = new StudentAnswer
                            {
                                QuestionId = _question.Id,
                                OptionId = optionId,
                                IsCorrect = selectedOption.IsCorrect,
                                AnsweredAt = DateTime.Now,
                                Attempt = StudTest._attempt,
                                Option = selectedOption,
                                Question = _question
                            };
                            _studentAnswersListTypeSecond.Add(newAnswer);
                        }
                    }
                }
            };

            magnifier.MouseDown += (s, e) =>
            {
                e.Handled = true;

                var mainWindow = Application.Current.MainWindow;

                Image fullImg = new Image
                {
                    Source = image.Source,
                    Stretch = Stretch.Uniform,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    Opacity = 0,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                ScaleTransform imgScale = new ScaleTransform(0.2, 0.2);
                fullImg.RenderTransform = imgScale;

                Rectangle fade = new Rectangle
                {
                    Fill = Brushes.Black,
                    Opacity = 0
                };

                Grid container = new Grid();
                container.Children.Add(fade);
                container.Children.Add(fullImg);

                Window imageWindow = new Window
                {
                    WindowStyle = WindowStyle.None,
                    WindowState = WindowState.Normal,
                    ResizeMode = ResizeMode.NoResize,
                    Background = Brushes.Transparent,
                    AllowsTransparency = true,
                    Topmost = true,
                    ShowInTaskbar = false,
                    SizeToContent = SizeToContent.Manual,
                    Left = 0,
                    Top = 0,
                    Width = SystemParameters.PrimaryScreenWidth,
                    Height = SystemParameters.PrimaryScreenHeight,
                    Content = container
                };

                fade.Width = imageWindow.Width;
                fade.Height = imageWindow.Height;

                container.MouseDown += (_, __) =>
                {
                    var closeDur = TimeSpan.FromMilliseconds(200);
                    var daOut = new DoubleAnimation(0, closeDur) { FillBehavior = FillBehavior.Stop };
                    daOut.Completed += (___sender, ___e) => imageWindow.Close();

                    fullImg.BeginAnimation(UIElement.OpacityProperty, daOut);
                    fade.BeginAnimation(UIElement.OpacityProperty, daOut);
                    imgScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                        new DoubleAnimation(0.2, closeDur));
                    imgScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                        new DoubleAnimation(0.2, closeDur));
                };

                imageWindow.Show();

                var showDur = TimeSpan.FromMilliseconds(200);
                fullImg.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(1, showDur));
                fade.BeginAnimation(UIElement.OpacityProperty,
                    new DoubleAnimation(0.8, showDur));
                imgScale.BeginAnimation(ScaleTransform.ScaleXProperty,
                    new DoubleAnimation(1.0, showDur) { EasingFunction = new QuadraticEase() });
                imgScale.BeginAnimation(ScaleTransform.ScaleYProperty,
                    new DoubleAnimation(1.0, showDur) { EasingFunction = new QuadraticEase() });
            };

            GeneratedAnswersPanel.Children.Add(border);
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
