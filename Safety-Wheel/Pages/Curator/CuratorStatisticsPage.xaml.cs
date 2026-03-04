using Safety_Wheel.Models;
using Safety_Wheel.Services;
using ScottPlot.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Safety_Wheel.Pages.Curator
{
    public partial class CuratorStatisticsPage : UserControl
    {
        public static CuratorStatisticsPage DataPageCurator { get; private set; }

        private readonly TestService _testService = new();
        private readonly AttemptService _attemptService = new();
        private readonly ParticipantAnswerService _participantAnswerService = new();

        public CuratorStatisticsPage()
        {
            InitializeComponent();
        }

        public void LoadStatistics(Models.Participant participant = null, Test test = null)
        {
            _testService.GetAll(null, CurrentUser.Id);

            ParticipantTitle.Text = participant == null
                    ? "Общая статистика"
                    : $"Статистика студента: {participant.Name}";

            var testStats = new List<TestStats>();
            var allTime = new List<OverallTimeData>();

            IEnumerable<Test> testsToProcess =
                test != null
                    ? new[] { test }
                    : _testService.Tests;

            foreach (var t in testsToProcess)
            {
                var stats = participant == null
                    ? GetTestStatsOverall(t)
                    : GetTestStats(t, participant);

                if (stats == null)
                    continue;

                testStats.Add(stats);

                allTime.Add(new OverallTimeData
                {
                    TestName = t.Name,
                    AverageDuration = stats.AverageDuration
                });
            }

            TestsItemsControl.ItemsSource = testStats;
            DrawTimeTypeCharts(participant, testsToProcess);
        }

        private void DrawTimeTypeCharts(Models.Participant participant, IEnumerable<Test> tests)
        {
            var typeData = new[]
            {
                 new { Type = 1, Name = "Без спешки", Plot = TimeType1Plot, Text = TimeType1Text, Time = 60 },
                 new { Type = 2, Name = "На время", Plot = TimeType2Plot, Text = TimeType2Text, Time = 20 },
                 new { Type = 3, Name = "Экзамен", Plot = TimeType3Plot, Text = TimeType3Text, Time = 20 }
            };

            foreach (var td in typeData)
            {
                var plot = td.Plot;
                plot.Plot.Clear();

                double avgTime = 0;
                int count = 0;

                foreach (var test in tests)
                {
                    var attempts = participant == null
                        ? _attemptService.GetAttemptsByTest(test.Id)
                            //.Where(a => a.DTestType == td.Type)
                            .Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue)
                        : _attemptService.GetAttemptsByTest(test.Id)
                            //.Where(a => a.ParticipantId == participant.Id)
                            //.Where(a => a.DTestType == td.Type)
                            .Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue);

                    foreach (var a in attempts)
                    {
                        avgTime += (a.FinishedAt.Value - a.StartedAt.Value).TotalMinutes;
                        count++;
                    }
                }

                double actualAvg = count > 0 ? Math.Round(avgTime / count, 2) : 0;
                double rest = Math.Max(0, td.Time - actualAvg);

                var pie = plot.Plot.Add.Pie(new double[] { actualAvg, rest });
                pie.ExplodeFraction = 0.05;
                pie.DonutFraction = 0.8;

                pie.Slices[0].FillColor = ScottPlot.Color.FromHex("#3498db");
                pie.Slices[1].FillColor = ScottPlot.Color.FromHex("#ecf0f1");
                pie.Slices[0].Label = "";
                pie.Slices[1].Label = "";

                plot.Plot.Axes.SetLimits(-1.2, 1.2, -1.2, 1.2);
                plot.Plot.Axes.Frameless();
                plot.Plot.HideGrid();
                plot.UserInputProcessor.Disable();

                td.Text.Text = $"{td.Name}.\nВ среднем {actualAvg:F1}\nиз {td.Time} мин";

                plot.Refresh();
            }
        }

        private TestStats GetTestStatsOverall(Test test)
        {
            var attempts = _attemptService.GetAttemptsByTest(test.Id)
                .Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue)
                .ToList();

            if (!attempts.Any())
            {
                return new TestStats
                {
                    TestId = test.Id,
                    TestName = $"{test.Topic.Name} {test.Name}",
                    HasData = false,
                    EmptyMessage = $"Тест «{test.Name}» ещё никто не проходил"
                };
            }

            var durations = attempts
                .Select(a => (a.FinishedAt.Value - a.StartedAt.Value).TotalMinutes)
                .ToList();

            if (!durations.Any())
                return null;

            var stats = new TestStats
            {
                TestId = test.Id,
                TestName = $"{test.Topic.Name} {test.Name}",
                AverageDuration = Math.Round(durations.Average(), 2)
            };

            foreach (var q in test.Questions.OrderBy(q => q.Number))
            {
                var answers = _participantAnswerService.GetAnswersByQuestion(q.Id);

                if (!answers.Any())
                    continue;

                int correct = answers.Count(a => a.IsCorrect == true);
                double percent = (double)correct / answers.Count * 100;

                stats.QuestionNumbers.Add((double)q.Number);
                stats.SuccessRates.Add(Math.Round(percent, 2));
            }

            return stats;
        }

        private TestStats GetTestStats(Test test, Models.Participant participant)
        {
            var attempts = _attemptService.GetAttemptsByTest(test.Id)
                .Where(a => a.ParticipantId == participant.Id)
                .Where(a => a.StartedAt.HasValue && a.FinishedAt.HasValue)
                .ToList();

            if (!attempts.Any())
            {
                return new TestStats
                {
                    TestId = test.Id,
                    TestName = $"{test.Topic.Name} {test.Name}",
                    HasData = false,
                    EmptyMessage = $"Студент не проходил тест «{test.Topic.Name} {test.Name}»"
                };
            }

            var stats = new TestStats
            {
                TestId = test.Id,
                TestName = $"{test.Topic.Name} {test.Name}"
            };

            var durations = attempts
                .Select(a => (a.FinishedAt.Value - a.StartedAt.Value).TotalMinutes)
                .ToList();

            if (!durations.Any())
                return null;

            stats.AverageDuration = Math.Round(durations.Average(), 2);

            foreach (var q in test.Questions.OrderBy(q => q.Number))
            {
                var answers = _participantAnswerService.GetAnswersByQuestion(q.Id)
                    .Where(a => a.Attempt.ParticipantId == participant.Id)
                    .ToList();

                if (!answers.Any())
                    continue;

                int correct = answers.Count(a => a.IsCorrect == true);
                double percent = (double)correct / answers.Count * 100;

                stats.QuestionNumbers.Add((double)q.Number);
                stats.SuccessRates.Add(Math.Round(percent, 2));
            }

            return stats;
        }

        private void PerformancePlot_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not WpfPlot plot)
                return;

            plot.Plot.Clear();
            plot.UserInputProcessor.Disable();
            if (plot.DataContext is not TestStats stats)
            {
                plot.Plot.Title("Нет данных");
                plot.Refresh();
                return;
            }

            if (!stats.HasData)
            {
                plot.Plot.Title(stats.EmptyMessage ?? "Нет данных");
                plot.Refresh();
                return;
            }

            plot.Plot.Title("Динамика успеваемости");
            plot.Plot.XLabel("Номер вопроса");
            plot.Plot.YLabel("Процент правильных (%)");

            if (!stats.QuestionNumbers.Any())
            {
                plot.Plot.Title("Нет данных для отображения");
                plot.Refresh();
                return;
            }

            plot.Plot.Add.Bars(
                stats.QuestionNumbers.ToArray(),
                stats.SuccessRates.ToArray()
            );

            double minX = stats.QuestionNumbers.Min() - 1;
            double maxX = stats.QuestionNumbers.Max() + 1;

            plot.Plot.Axes.Bottom.SetTicks(
                stats.QuestionNumbers.ToArray(),
                stats.QuestionNumbers.Select(n => n.ToString("F0")).ToArray()
            );

            plot.Plot.Axes.SetLimits(
                minX,
                maxX,
                0,
                100
            );
            
            plot.Refresh();
        }

        private bool _isInitialized = false;

        private void CuratorStatisticsPage_Loaded(object sender, RoutedEventArgs e)
        {
            DataPageCurator = this;

            if (_isInitialized)
                return;

            _isInitialized = true;

            LoadStatistics(null);
        }
    }

    public class TestStats
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public double AverageDuration { get; set; }
        public bool HasData { get; set; } = true;
        public string EmptyMessage { get; set; }
        public List<double> QuestionNumbers { get; set; } = new();
        public List<double> SuccessRates { get; set; } = new();
    }

    public class OverallTimeData
    {
        public string TestName { get; set; }
        public double AverageDuration { get; set; }
    }
}