using System;
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
                case Key.F9:
                    RunVideo(3);
                    break;
                case Key.F5:
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
            DoubleAnimation ImageOpacityAnimation = new DoubleAnimation
            {
                From = ImageGrid.Opacity,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            ImageGrid.BeginAnimation(OpacityProperty, ImageOpacityAnimation);
            mediaElement.Source = null;
            mediaElement.Source = new Uri(Config.GetConfigValue("video_" + Number.ToString()));
            mediaElement.Play();
            PlayStatus = MediaStatus.Play;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            PlayStatus = MediaStatus.Stop;
            DoubleAnimation ImageOpacityAnimation = new DoubleAnimation
            {
                From = ImageGrid.Opacity,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(500)
            };
            ImageGrid.BeginAnimation(OpacityProperty, ImageOpacityAnimation);
        }
    }

    enum MediaStatus : byte
    {
        Stop,
        Play,
        Pause
    }
}
