using System;
using System.Linq;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NosceIpsum.Properties;
using Timer = System.Timers.Timer;

namespace NosceIpsum
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Timer _popupTimer;
        private readonly Timer _closeTimer;

        public MainWindow()
        {
            _popupTimer = new Timer(GetPopupInterval());
            _closeTimer = new Timer(Settings.Default.PopupDisplaySec * 1000);

            InitializeComponent();
        }

        private void Text_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            Close();
        }

        private void Border1MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }

        private void Window_Loaded(object sender, EventArgs e)
        {
            _popupTimer.Elapsed += PopupTimerElapsed;
            _closeTimer.Elapsed += CloseTimerElapsed;
            FormFadeOut.Completed += FormFadeOut_Completed;

            _popupTimer.Start();
        }

        void FormFadeOut_Completed(object sender, EventArgs e)
        {
            Dispatcher.Invoke(new Action(delegate { Visibility = Visibility.Hidden; }));
        }

        private static int GetPopupInterval()
        {
            var interval = Settings.Default.PopupIntervalMin.Split('-').Select(x => int.Parse(x));
            int output;

            if (interval.Count() == 1)
                output = interval.First();
            else
            {
                var r = new Random();
                output = r.Next(interval.First(), interval.Last() + 1);
            }

            return output * 1000 * (Debugger.IsAttached ? 1 : 60);
        }

        void PopupTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _popupTimer.Stop();

            Border.Dispatcher.Invoke(new Action(delegate { Border.Background = RandomColor(); }));
            Text.Dispatcher.Invoke(new Action(delegate { Text.Content = RandomWisdom(Text.Content); }));
            Dispatcher.Invoke(new Action(
                delegate
                    {
                        var r = new Random();

                        Top = r.Next(10, (int) (SystemParameters.PrimaryScreenHeight - Text.ActualHeight));
                        Left = r.Next(10, (int) (SystemParameters.PrimaryScreenWidth - Text.ActualWidth));

                        Visibility = Visibility.Visible;
                        FormFade.Begin();
                    }));

            _closeTimer.Start();
        }

        void CloseTimerElapsed(object sender, ElapsedEventArgs e)
        {
            _closeTimer.Stop();

            Dispatcher.Invoke(new Action(() => FormFadeOut.Begin()));
            _popupTimer.Start();
        }

        public Brush RandomColor()
        {
            var r = new Random();
            var colors = new[]
            {
                Colors.MediumSpringGreen,
                Colors.MediumTurquoise,
                Colors.Plum,
                Colors.SandyBrown,
                Colors.YellowGreen
            };

            return new SolidColorBrush(colors[r.Next(0, colors.Length)]);
        }

        public string RandomWisdom(object currentContent)
        {
            var r = new Random();
            var wisdom = Settings.Default.Wisdom.Split(';');
            string output;

            do
            {
                output = wisdom[r.Next(0, wisdom.Length)];
            } while (currentContent != null && output == currentContent.ToString());

            return output;
        }
    }
}
