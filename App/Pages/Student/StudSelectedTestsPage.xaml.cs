using Notifications.Wpf;
using Safety_Wheel.Models;
using Safety_Wheel.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Safety_Wheel.Pages.Student
{
    /// <summary>
    /// Логика взаимодействия для StudSelectedTestsPage.xaml
    /// </summary>
    public partial class StudSelectedTestsPage : Page
    {
        public static string TypeDiscipline { get; set; } = string.Empty;
        public int TypeTest { get; set; } = 0;

        TestService _testService = new();
        public static Models.Student thisStudent;
        private Button? _selectedButton;

        public StudSelectedTestsPage(string typeDiscipline, Models.Student student)
        {
            thisStudent = student;
            TypeDiscipline = typeDiscipline;
            InitializeComponent();
            this.DataContext = this;

            LoadTests();
        }

        private void LoadTests()
        {
            _testService.GetAll(teacherId:thisStudent.TeachersId);
            _testService.GetTestsBySubjectName(TypeDiscipline);

            GeneratedOptionsPanel.Children.Clear();

            int counter = 1;
            foreach (var test in _testService.Tests)
            {
                if (test.IsPublic == true)
                {
                    CreateTestOptionElement(test, counter);
                    counter++;
                }
            }
        }

        private void CreateTestOptionElement(Test test, int optionNumber)
        {
            Border border = new Border();

            Button mainButton = new Button
            {
                Tag = test.Id.ToString()
            };

            if (Application.Current.TryFindResource("ButtonTests") is Style st)
                mainButton.Style = st;

            Grid contentGrid = new Grid
            {
                Width = 1200,
                VerticalAlignment = VerticalAlignment.Center
            };

            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // название
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // режимы
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) }); // play

            TextBlock titleText = new TextBlock
            {
                Text = test.Name,
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 20, 0)
            };
            Grid.SetColumn(titleText, 0);

            StackPanel iconsContainer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            AddIconWithIndicator(iconsContainer, test.Id, 1, "relax_icon.png");
            AddIconWithIndicator(iconsContainer, test.Id, 2, "time_icon.png");
            AddIconWithIndicator(iconsContainer, test.Id, 3, "exam_icon.png");

            Grid.SetColumn(iconsContainer, 1);

            Button playBtn = new Button
            {
                Tag = test.Id.ToString(),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Cursor = Cursors.Hand,
                Margin = new Thickness(0, 0, 20, 0)
            };

            if (Application.Current.TryFindResource("PlayButtonOptions") is Style playStyle)
                playBtn.Style = playStyle;

            Grid.SetColumn(playBtn, 2);

            contentGrid.Children.Add(titleText);
            contentGrid.Children.Add(iconsContainer);
            contentGrid.Children.Add(playBtn);

            Viewbox viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly
            };

            viewbox.Child = contentGrid;
            mainButton.Content = viewbox;

            mainButton.Click += (s, e) =>
            {
                if (TypeTest == 0)
                {
                    new NotificationManager().Show(new NotificationContent
                    {
                        Title = "Внимание",
                        Message = "Для продолжения выберите тип теста в левой части экрана",
                        Type = NotificationType.Warning
                    });
                    return;
                }

                if ((s as FrameworkElement)?.Tag is string idStr &&
                    decimal.TryParse(idStr, out decimal id))
                {
                    var selected = _testService.Tests.FirstOrDefault(t => t.Id == id);
                    if (selected != null)
                        NavigationService.Navigate(new StudTestInfo(selected, TypeTest));
                }
            };

            border.Child = mainButton;
            GeneratedOptionsPanel.Children.Add(border);
        }
        private void AddIconWithIndicator(StackPanel container, decimal testId, int testType, string iconName)
        {
            var attemptService = new AttemptService();
            var lastAttempt = attemptService.GetLastByType(CurrentUser.Id, (int)testId, testType);

            var testInfo = _testService.GetTestById((int)testId);
            int maxScore = testInfo?.MaxScore ?? 100;

            Brush indicatorColor = GetIndicatorColor(lastAttempt, maxScore);

            Border iconContainer = new Border
            {
                Width = 57,
                Height = 57,
                CornerRadius = new CornerRadius(25),
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                Margin = new Thickness(7),
                VerticalAlignment = VerticalAlignment.Center
            };

            iconContainer.Effect = new DropShadowEffect
            {
                ShadowDepth = 2,
                BlurRadius = 2,
                Opacity = 0.3,
                Color = Colors.Black
            };

            Grid innerGrid = new Grid();

            Image icon = new Image
            {
                Source = new BitmapImage(new Uri(System.IO.Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    $"Images/{iconName}"), UriKind.RelativeOrAbsolute)),
                Width = 40,
                Height = 40,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            innerGrid.Children.Add(icon);

            if (indicatorColor != Brushes.Transparent)
            {
                iconContainer.BorderBrush = indicatorColor;
                iconContainer.BorderThickness = new Thickness(3);

                iconContainer.Effect = new DropShadowEffect
                {
                    ShadowDepth = 0,
                    BlurRadius = 10,
                    Opacity = 0.7,
                    Color = ((SolidColorBrush)indicatorColor).Color
                };

                Border scoreIndicator = new Border
                {
                    Width = 25,
                    Height = 25,
                    CornerRadius = new CornerRadius(10),
                    Background = indicatorColor,
                    BorderThickness = new Thickness(2),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, -5, -5),
                    Child = new TextBlock
                    {
                        Text = lastAttempt?.Score != null ?
                            $"{(double)lastAttempt.Score.Value / maxScore * 100:0}%" : "?",
                        FontSize = 10,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };

                innerGrid.Children.Add(scoreIndicator);
            }
            else
            {
                iconContainer.BorderBrush = Brushes.LightGray;
                iconContainer.BorderThickness = new Thickness(1);
                iconContainer.Opacity = 0.7;
            }

            if (lastAttempt != null && lastAttempt.Score != null)
            {
                double percentage = (double)lastAttempt.Score.Value / maxScore * 100;
                iconContainer.ToolTip = new ToolTip
                {
                    Content = $"Результат: {lastAttempt.Score.Value}/{maxScore} ({percentage:0.0}%)",
                    Background = Brushes.White,
                    Foreground = Brushes.Black
                };
            }
            else
            {
                iconContainer.ToolTip = new ToolTip
                {
                    Content = "Тест не пройден",
                    Background = Brushes.White,
                    Foreground = Brushes.Black
                };
            }

            iconContainer.Child = innerGrid;
            container.Children.Add(iconContainer);
        }

        private Brush GetIndicatorColor(Attempt attempt, int maxScore)
        {
            if (attempt == null || attempt.Score == null)
            {
                return Brushes.Transparent;
            }

            int score = attempt.Score.Value;
            double percentage = (double)score / maxScore * 100;

            if (percentage >= 95)
            {
                return Brushes.Green;
            }
            else if (percentage >= 70)
            {
                return Brushes.Gold;
            }
            else
            {
                return Brushes.Red;
            }
        }

        private void ButtonRelax_Click(object sender, RoutedEventArgs e)
        {
            TypeTest = 1;
            SetButtonAsSelected((Button)sender);
        }

        private void ButtonTime_Click(object sender, RoutedEventArgs e)
        {
            TypeTest = 2;
            SetButtonAsSelected((Button)sender);
        }

        private void ButtonExam_Click(object sender, RoutedEventArgs e)
        {
            TypeTest = 3;
            SetButtonAsSelected((Button)sender);
        }

        private void SetButtonAsSelected(Button button)
        {
            if (_selectedButton != null)
            {
                _selectedButton.Style = (Style)FindResource("MainButton");
            }

            _selectedButton = button;
            if (_selectedButton != null)
            {
                _selectedButton.Style = (Style)FindResource("DownButton");
            }
        }

    }
}