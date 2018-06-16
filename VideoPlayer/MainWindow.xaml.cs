using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace VideoPlayer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SQLite.SQLiteConfig Config;
        MediaStatus PlayStatus = MediaStatus.Stop;

        public MainWindow()
        {
            InitializeComponent();

            Config = new SQLite.SQLiteConfig("config.db");
            IdleImage.Source = new BitmapImage(new Uri(Config.GetConfigValue("stop_image")));
            WarningImage.Source = new BitmapImage(new Uri(Config.GetConfigValue("warning_image")));
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.PageUp:
                    RunVideo(1);
                    break;
                case Key.PageDown:
                    RunVideo(2);
                    break;
                case Key.F5:
                    RunVideo(3);
                    break;
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
