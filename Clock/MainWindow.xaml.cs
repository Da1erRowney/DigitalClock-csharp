using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Clock
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;
        private bool isPinned = false;
        private bool isTopMost = false;

        public MainWindow()
        {
            InitializeComponent();
            StartClock();
            AddToStartup();
        }

        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateClock;
            timer.Start();
        }

        private void UpdateClock(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            HourLabel.Content = now.ToString("HH");
            MinuteLabel.Content = now.ToString("mm");
            SecondLabel.Content = now.ToString("ss");
            DateLabel.Content = now.ToString("dddd, MMMM dd, yyyy");
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !isPinned && !isTopMost)
            {
                this.DragMove();
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (isTopMost)
            {
                e.Handled = true;
            }
        }

        private void Window_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            MenuItem pinMenuItem = (MenuItem)contextMenu.Items[0];
            MenuItem unpinMenuItem = (MenuItem)contextMenu.Items[1];

            pinMenuItem.IsEnabled = !isPinned;
            unpinMenuItem.IsEnabled = isPinned;
            topMostMenuItem.IsEnabled = !isTopMost; 
            belowMenuItem.IsEnabled = isTopMost;
        }

        private void PinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            isPinned = true;
        }

        private void UnpinMenuItem_Click(object sender, RoutedEventArgs e)
        {
            isPinned = false;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TopMostMenuItem_Click(object sender, RoutedEventArgs e)
        {
            isTopMost = true;
            this.Topmost = true;
            this.Opacity = 0.5;
        }

        private void BelowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            isTopMost = false;
            this.Topmost = false;
            this.Opacity = 1.0;
        }

        private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Opacity = e.NewValue / 100;
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newSize = e.NewValue;

            if (HourLabel != null) HourLabel.FontSize = newSize;
            if (MinuteLabel != null) MinuteLabel.FontSize = newSize;
            if (SecondLabel != null) SecondLabel.FontSize = newSize;
            if (DateLabel != null) DateLabel.FontSize = newSize / 3;
        }

        private void ResetAll_Click(object sender, RoutedEventArgs e)
        {
            OpacitySlider.Value = 100;
            FontSizeSlider.Value = 48;

            this.Opacity = 1.0;

            HourLabel.FontSize = 144;
            MinuteLabel.FontSize = 144;
            SecondLabel.FontSize = 144;
            DateLabel.FontSize = 48; 
        }

        private void AddToStartup()
        {
            string appName = "DigitalClock"; 
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(appName, "\"" + appPath + "\"");
            }
        }
    }
}