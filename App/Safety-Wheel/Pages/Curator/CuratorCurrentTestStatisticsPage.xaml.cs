using CozyTest.ViewModels.CuratorVM.StatisticsVM;
using ScottPlot.WPF;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace CozyTest.Pages.Curator
{
    public partial class CuratorCurrentTestStatisticsPage : UserControl
    {
        private StatisticsCurrentTestViewModel _vm;
        private WpfPlot _scoreDistributionPlot;

        public CuratorCurrentTestStatisticsPage()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (_vm != null)
                _vm.PropertyChanged -= OnViewModelPropertyChanged;

            _vm = DataContext as StatisticsCurrentTestViewModel;

            if (_vm != null)
                _vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(StatisticsCurrentTestViewModel.ScoreDistributionData))
            {
                Dispatcher.BeginInvoke(() => DrawScoreDistribution());
            }
        }

        private void ScoreDistributionPlot_Loaded(object sender, RoutedEventArgs e)
        {
            _scoreDistributionPlot = sender as WpfPlot;
            DrawScoreDistribution();
        }

        private void DrawScoreDistribution()
        {
            if (_scoreDistributionPlot == null || _vm == null) return;

            var plot = _scoreDistributionPlot;
            plot.Plot.Clear();
            plot.UserInputProcessor.Disable();

            var data = _vm.ScoreDistributionData;
            var labels = _vm.ScoreDistributionLabels;

            if (data == null || !data.Any() || data.All(d => d == 0))
            {
                plot.Refresh();
                return;
            }

            var bars = plot.Plot.Add.Bars(data);

            for (int i = 0; i < bars.Bars.Count; i++)
            {
                bars.Bars[i].FillColor = ScottPlot.Color.FromHex("#4285F4");
                bars.Bars[i].LineColor = ScottPlot.Color.FromHex("#FFFFFF");
                bars.Bars[i].LineWidth = 1;
            }

            plot.Plot.Axes.Bottom.SetTicks(
                Enumerable.Range(0, labels.Length).Select(i => (double)i).ToArray(),
                labels
            );

            plot.Plot.Axes.Bottom.Label.Text = "Получено очков";
            plot.Plot.Axes.Left.Label.Text = "Попыток на всех участников";

            plot.Plot.Axes.Top.MajorTickStyle.Length = 0;
            plot.Plot.Axes.Right.MajorTickStyle.Length = 0;
            plot.Plot.Axes.Top.MinorTickStyle.Length = 0;
            plot.Plot.Axes.Right.MinorTickStyle.Length = 0;

            plot.Plot.Axes.SetLimits(-0.5, data.Length - 0.5, 0, data.Max() * 1.2);
            plot.Plot.HideGrid();

            plot.Refresh();
        }

        private void QuestionHorizontalBarPlot_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is not WpfPlot plot) return;
            if (plot.DataContext is not QuestionDetailStatistics stats) return;

            plot.Plot.Clear();
            plot.UserInputProcessor.Disable();

            if (stats.ChartData == null || !stats.ChartData.Any())
            {
                plot.Refresh();
                return;
            }

            var barPlot = plot.Plot.Add.Bars(stats.ChartData);

            for (int i = 0; i < barPlot.Bars.Count && i < stats.Options.Count; i++)
            {
                var option = stats.Options[i];
                var color = option.IsCorrect ? "#34A853" : "#4285F4";
                barPlot.Bars[i].FillColor = ScottPlot.Color.FromHex(color);
            }

            barPlot.Horizontal = true;

            plot.Plot.Axes.Left.SetTicks(
                Enumerable.Range(0, stats.ChartLabels.Length).Select(i => (double)i).ToArray(),
                stats.ChartLabels.Select(l => l.Length > 30 ? l.Substring(0, 30) + "..." : l).ToArray()
            );

            plot.Plot.Axes.Left.Label.Text = "";
            plot.Plot.Axes.Bottom.Label.Text = "Количество";

            plot.Plot.Axes.Top.MajorTickStyle.Length = 0;
            plot.Plot.Axes.Right.MajorTickStyle.Length = 0;

            plot.Plot.Axes.SetLimits(0, stats.ChartData.Max() * 1.2, -0.5, stats.ChartData.Length - 0.5);
            plot.Plot.HideGrid();

            plot.Refresh();
        }
    }
}