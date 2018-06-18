using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace VideoPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLite.SQLiteConfig Config;
        MediaStatus PlayStatus = MediaStatus.Stop;
        List<string> StopList = new List<string>();

        public MainWindow()
        {
            InitializeComponent();

            Config = new SQLite.SQLiteConfig("config.db");
            IdleImage.Source = new BitmapImage(new Uri(Config.GetConfigValue("stop_image")));
            WarningImage.Source = new BitmapImage(new Uri(Config.GetConfigValue("warning_image")));

            DispatcherTimer CheckTime = new DispatcherTimer();
            CheckTime.Tick += CheckTimeTick;
            CheckTime.Interval = TimeSpan.FromMilliseconds(50);
            CheckTime.Start();
        }

        private void CheckTimeTick(object sender, EventArgs e)
        {
            if (PlayStatus == MediaStatus.Play)
            {
                string CurTime = mediaElement.Position.Hours.ToString() + ":" +
                    mediaElement.Position.Minutes.ToString() + ":" +
                    mediaElement.Position.Seconds.ToString() + "." +
                    mediaElement.Position.Milliseconds.ToString();

                DebugLabel.Content = CurTime;

                foreach (string Stop in StopList)
                {
                    if (CurTime.Contains(Stop))
                    {
                        mediaElement.Pause();
                        PlayStatus = MediaStatus.Pause;
                        StopList.Remove(Stop);
                        return;
                    }

                }
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.D1:
                case Key.NumPad1:
                case Key.PageUp:
                    RunVideo(1);
                    break;
                case Key.D2:
                case Key.NumPad2:
                case Key.PageDown:
                    RunVideo(2);
                    break;
                case Key.D3:
                case Key.NumPad3:
                case Key.F5:
                    RunVideo(3);
                    break;
                case Key.Space:
                case Key.F9:
                case Key.OemPeriod:
                    switch (PlayStatus)
                    {
                        case MediaStatus.Play:
                            mediaElement.Pause();
                            PlayStatus = MediaStatus.Pause;
                            break;
                        case MediaStatus.Pause:
                            mediaElement.Play();
                            PlayStatus = MediaStatus.Play;
                            break;
                        case MediaStatus.Stop:
                            break;
                    }
                    break;
            }
        }

        private void RunVideo(int Number)
        {
            switch (PlayStatus)
            {
                case MediaStatus.Play:
                    return;
                case MediaStatus.Pause:
                    if (WarningGrid.Opacity > 0)
                    {
                        WarningGrid.BeginAnimation(OpacityProperty, OpacityAnimation(WarningGrid.Opacity, 0, 10));
                    }
                    else
                    {
                        WarningGrid.BeginAnimation(OpacityProperty, OpacityAnimation(WarningGrid.Opacity, 1, 10));
                        WarningMessage();
                        return;
                    }
                    break;
                case MediaStatus.Stop:
                    ImageGrid.BeginAnimation(OpacityProperty, OpacityAnimation(ImageGrid.Opacity, 0, 500));
                    break;
            }

            mediaElement.Source = null;
            mediaElement.Source = new Uri(Config.GetConfigValue("video_" + Number.ToString()));

            StopList = Config.GetStringList("video_" + Number.ToString() + "_stop");

            mediaElement.Play();
            PlayStatus = MediaStatus.Play;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayStatus = MediaStatus.Stop;
            ImageGrid.BeginAnimation(OpacityProperty, OpacityAnimation(ImageGrid.Opacity, 1, 500));
        }

        private async Task WarningMessage()
        {
            
            await Task.Delay(3000);

            if (WarningGrid.Opacity == 1)
            {
                WarningGrid.BeginAnimation(OpacityProperty, OpacityAnimation(WarningGrid.Opacity, 0, 500));
            }
        }

        private DoubleAnimation OpacityAnimation(double From, double To, int Time)
        {
            return new DoubleAnimation
            {
                From = From,
                To = To,
                Duration = TimeSpan.FromMilliseconds(Time)
            };
        }
    }

    enum MediaStatus : byte
    {
        Stop,
        Play,
        Pause
    }
}
