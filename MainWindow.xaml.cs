using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Stoper
{
    public partial class MainWindow : Window
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly DispatcherTimer _timer = new DispatcherTimer();
        private readonly ObservableCollection<LapItem> _laps = new ObservableCollection<LapItem>();
        private long _lastLapMilliseconds = 0;

        public MainWindow()
        {
            InitializeComponent();

            lstLaps.ItemsSource = _laps;

            // Odświeżanie 60 FPS (~16ms) dla płynnego wyświetlania setnych części sekundy
            _timer.Interval = TimeSpan.FromMilliseconds(16);
            _timer.Tick += Timer_Tick;

            UpdateDisplay(TimeSpan.Zero);
            UpdateButtonsState();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateDisplay(_stopwatch.Elapsed);
        }

        private void UpdateDisplay(TimeSpan time)
        {
            txtMainTime.Text = $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            txtMillis.Text = $"{time.Milliseconds / 10:D2}";
        }

        private void UpdateButtonsState()
        {
            bool isRunning = _stopwatch.IsRunning;
            bool hasElapsed = _stopwatch.ElapsedMilliseconds > 0;

            btnStart.IsEnabled = !isRunning;
            btnStop.IsEnabled = isRunning;
            btnLap.IsEnabled = isRunning;
            btnReset.IsEnabled = !isRunning && hasElapsed;

            if (isRunning)
            {
                txtStatus.Text = "W TRAKCIE POMIARU...";
                txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(52, 211, 153)); // Szmaragd
                badgeStatus.Background = new SolidColorBrush(Color.FromArgb(40, 16, 185, 129));
            }
            else if (hasElapsed)
            {
                txtStatus.Text = "POMIAR ZATRZYMANY";
                txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(251, 146, 60)); // Pomarańcz
                badgeStatus.Background = new SolidColorBrush(Color.FromArgb(40, 249, 115, 22));
            }
            else
            {
                txtStatus.Text = "GOTOWY DO STARTU";
                txtStatus.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184)); // Szary
                badgeStatus.Background = new SolidColorBrush(Color.FromRgb(30, 41, 59));
            }

            // Puste / pełne okrążenia
            if (_laps.Count == 0)
            {
                pnlEmptyLaps.Visibility = Visibility.Visible;
                lstLaps.Visibility = Visibility.Collapsed;
                btnCopy.Visibility = Visibility.Collapsed;
            }
            else
            {
                pnlEmptyLaps.Visibility = Visibility.Collapsed;
                lstLaps.Visibility = Visibility.Visible;
                btnCopy.Visibility = Visibility.Visible;
            }

            txtLapCount.Text = _laps.Count.ToString();
        }

        private void StartStopwatch()
        {
            if (_stopwatch.IsRunning) return;

            PlayClickSound();
            _stopwatch.Start();
            _timer.Start();
            UpdateButtonsState();
        }

        private void StopStopwatch()
        {
            if (!_stopwatch.IsRunning) return;

            PlayClickSound();
            _stopwatch.Stop();
            _timer.Stop();
            UpdateDisplay(_stopwatch.Elapsed);
            UpdateButtonsState();
        }

        private void ResetStopwatch()
        {
            PlayClickSound();
            _stopwatch.Reset();
            _timer.Stop();
            _lastLapMilliseconds = 0;
            _laps.Clear();

            UpdateDisplay(TimeSpan.Zero);
            UpdateButtonsState();
        }

        private void RecordLap()
        {
            if (!_stopwatch.IsRunning && _stopwatch.ElapsedMilliseconds == 0) return;

            PlayLapSound();

            long totalMs = _stopwatch.ElapsedMilliseconds;
            long splitMs = totalMs - _lastLapMilliseconds;
            _lastLapMilliseconds = totalMs;

            int lapNumber = _laps.Count + 1;

            var item = new LapItem
            {
                LapNumber = lapNumber,
                SplitMilliseconds = splitMs,
                TotalMilliseconds = totalMs,
                LapNumberText = $"#{lapNumber:D2}",
                SplitTimeText = $"+{FormatTimeSpan(TimeSpan.FromMilliseconds(splitMs))}",
                TotalTimeText = FormatTimeSpan(TimeSpan.FromMilliseconds(totalMs))
            };

            // Najnowsze na górze
            _laps.Insert(0, item);

            RecalculateLapHighlights();
            UpdateButtonsState();

            if (lstLaps.Items.Count > 0)
            {
                lstLaps.ScrollIntoView(lstLaps.Items[0]);
            }
        }

        private void RecalculateLapHighlights()
        {
            if (_laps.Count < 2)
            {
                foreach (var lap in _laps)
                {
                    lap.BadgeVisibility = Visibility.Collapsed;
                    lap.SplitColor = new SolidColorBrush(Color.FromRgb(241, 245, 249));
                }
                return;
            }

            long min = _laps.Min(l => l.SplitMilliseconds);
            long max = _laps.Max(l => l.SplitMilliseconds);

            foreach (var lap in _laps)
            {
                if (lap.SplitMilliseconds == min)
                {
                    lap.BadgeVisibility = Visibility.Visible;
                    lap.BadgeText = "NAJSZYBSZE";
                    lap.BadgeBackground = new SolidColorBrush(Color.FromArgb(50, 16, 185, 129));
                    lap.BadgeForeground = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                    lap.SplitColor = new SolidColorBrush(Color.FromRgb(52, 211, 153));
                }
                else if (lap.SplitMilliseconds == max)
                {
                    lap.BadgeVisibility = Visibility.Visible;
                    lap.BadgeText = "NAJWOLNIEJSZE";
                    lap.BadgeBackground = new SolidColorBrush(Color.FromArgb(50, 239, 68, 68));
                    lap.BadgeForeground = new SolidColorBrush(Color.FromRgb(248, 113, 113));
                    lap.SplitColor = new SolidColorBrush(Color.FromRgb(248, 113, 113));
                }
                else
                {
                    lap.BadgeVisibility = Visibility.Collapsed;
                    lap.SplitColor = new SolidColorBrush(Color.FromRgb(241, 245, 249));
                }
            }
        }

        private static string FormatTimeSpan(TimeSpan ts)
        {
            if (ts.TotalHours >= 1)
            {
                return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
            }
            return $"{ts.Minutes:D2}:{ts.Seconds:D2}.{ts.Milliseconds / 10:D2}";
        }

        private void PlayClickSound()
        {
            try
            {
                SystemSounds.Asterisk.Play();
            }
            catch { }
        }

        private void PlayLapSound()
        {
            try
            {
                SystemSounds.Beep.Play();
            }
            catch { }
        }

        // --- Obsługa zdarzeń UI ---

        private void BtnStart_Click(object sender, RoutedEventArgs e)
        {
            StartStopwatch();
        }

        private void BtnStop_Click(object sender, RoutedEventArgs e)
        {
            StopStopwatch();
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            ResetStopwatch();
        }

        private void BtnLap_Click(object sender, RoutedEventArgs e)
        {
            RecordLap();
        }

        private void BtnCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_laps.Count == 0) return;

            var sb = new StringBuilder();
            sb.AppendLine("=== WYNIKI STOPER DESKTOP ===");
            sb.AppendLine("Nr\tCzas okrążenia\tŁączny czas");

            foreach (var lap in _laps.OrderBy(l => l.LapNumber))
            {
                sb.AppendLine($"{lap.LapNumberText}\t{lap.SplitTimeText}\t{lap.TotalTimeText}");
            }

            try
            {
                Clipboard.SetText(sb.ToString());
                btnCopy.Content = "✓ Skopiowano!";
                var t = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
                t.Tick += (_, _) =>
                {
                    btnCopy.Content = "📋 Kopiuj";
                    t.Stop();
                };
                t.Start();
            }
            catch { }
        }

        private void ChkAlwaysOnTop_Checked(object sender, RoutedEventArgs e)
        {
            Topmost = true;
        }

        private void ChkAlwaysOnTop_Unchecked(object sender, RoutedEventArgs e)
        {
            Topmost = false;
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (_stopwatch.IsRunning)
                    StopStopwatch();
                else
                    StartStopwatch();
                e.Handled = true;
            }
            else if (e.Key == Key.R)
            {
                if (!_stopwatch.IsRunning)
                {
                    ResetStopwatch();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.L)
            {
                if (_stopwatch.IsRunning)
                {
                    RecordLap();
                    e.Handled = true;
                }
            }
        }
    }

    public class LapItem : System.ComponentModel.INotifyPropertyChanged
    {
        public int LapNumber { get; set; }
        public long SplitMilliseconds { get; set; }
        public long TotalMilliseconds { get; set; }

        public string LapNumberText { get; set; } = "";
        public string SplitTimeText { get; set; } = "";
        public string TotalTimeText { get; set; } = "";

        private Visibility _badgeVisibility = Visibility.Collapsed;
        public Visibility BadgeVisibility
        {
            get => _badgeVisibility;
            set { _badgeVisibility = value; OnPropertyChanged(nameof(BadgeVisibility)); }
        }

        private string _badgeText = "";
        public string BadgeText
        {
            get => _badgeText;
            set { _badgeText = value; OnPropertyChanged(nameof(BadgeText)); }
        }

        private Brush _badgeBackground = Brushes.Transparent;
        public Brush BadgeBackground
        {
            get => _badgeBackground;
            set { _badgeBackground = value; OnPropertyChanged(nameof(BadgeBackground)); }
        }

        private Brush _badgeForeground = Brushes.White;
        public Brush BadgeForeground
        {
            get => _badgeForeground;
            set { _badgeForeground = value; OnPropertyChanged(nameof(BadgeForeground)); }
        }

        private Brush _splitColor = new SolidColorBrush(Color.FromRgb(241, 245, 249));
        public Brush SplitColor
        {
            get => _splitColor;
            set { _splitColor = value; OnPropertyChanged(nameof(SplitColor)); }
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }
}
