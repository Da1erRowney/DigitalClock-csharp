using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Clock
{
    public partial class MainWindow : Window
    {
        #region [Fields and Properties]


        private DispatcherTimer timer;
        private bool isPinned = false;
        private bool isTopMost = false;
        private List<Alarm> alarms = new List<Alarm>
        {
            new Alarm { Time = new TimeSpan(10, 0, 0), IsActive =true  },
            new Alarm { Time = new TimeSpan(11, 0, 0), IsActive =true  },
            new Alarm { Time = new TimeSpan(12, 30, 0), IsActive =true  },
            new Alarm { Time = new TimeSpan(15, 0, 0), IsActive =true  },
            new Alarm { Time = new TimeSpan(16, 0, 0), IsActive =true  },
            new Alarm { Time = new TimeSpan(17, 15, 0), IsActive =true  },
        };

        // Используйте ObservableCollection для автоматического обновления интерфейса
        public ObservableCollection<AppInfo> Apps { get; set; } = new ObservableCollection<AppInfo>();

        #endregion


        #region [Constructor]


        public MainWindow()
        {
            InitializeComponent();
            LoadAppData(); // Загружаем данные при старте приложения
            StartClock();
            AddToStartup();
        }


        #endregion


        #region [Main Function]

        //Запуск хода времни
        private void StartClock()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += UpdateClock;
            timer.Start();
        }

        //Обновление времени
        private void UpdateClock(object sender, EventArgs e)
        {
            var now = DateTime.Now;
            HourLabel.Content = now.ToString("HH");
            MinuteLabel.Content = now.ToString("mm");
            SecondLabel.Content = now.ToString("ss");
            DateLabel.Content = now.ToString("dddd, MMMM dd, yyyy");

            foreach (var alarm in alarms)
            {
                if (alarm.IsActive && alarm.Time.Hours == now.Hour && alarm.Time.Minutes == now.Minute && alarm.Time.Seconds == now.Second)
                {
                    // Change the color to indicate an active alarm
                    HourLabel.Foreground = new SolidColorBrush(Colors.Red);
                    HourBorder.Background = new SolidColorBrush(Colors.White);

                    MinuteLabel.Foreground = new SolidColorBrush(Colors.Red);
                    MinuteBorder.Background = new SolidColorBrush(Colors.White);

                    SecondLabel.Foreground = new SolidColorBrush(Colors.Red);
                    SecondBorder.Background = new SolidColorBrush(Colors.White);

                    DateBorder.Background = new SolidColorBrush(Colors.White);
                    DateLabel.Foreground = new SolidColorBrush(Colors.Red);

                    ToggleBorder.Background = new SolidColorBrush(Colors.White);
                    ToggleButton.Foreground = new SolidColorBrush(Colors.Red);
                    ToggleButton.Background = new SolidColorBrush(Colors.White);
                    alarm.IsActive = false;
                    break;
                }
            }

            LoadAppData();
        }
        private void Clock_Click(object sender, MouseButtonEventArgs e)
        {
            foreach (var alarm in alarms)
            {
                alarm.IsActive = true; // Disable the alarm
            }
            HourLabel.Foreground = new SolidColorBrush(Colors.White); // Reset color
            HourBorder.Background = new SolidColorBrush(Colors.Black);

            MinuteLabel.Foreground = new SolidColorBrush(Colors.White);
            MinuteBorder.Background = new SolidColorBrush(Colors.Black);

            SecondLabel.Foreground = new SolidColorBrush(Colors.White);
            SecondBorder.Background = new SolidColorBrush(Colors.Black);

            DateBorder.Background = new SolidColorBrush(Colors.Black);
            DateLabel.Foreground = new SolidColorBrush(Colors.White);

            ToggleBorder.Background = new SolidColorBrush(Colors.Black);
            ToggleButton.Background = new SolidColorBrush(Colors.Black);
            ToggleButton.Foreground = new SolidColorBrush(Colors.White);
        }

        //Сохранение данных об приложениях в файл
        private void SaveAppData()
        {
            var json = JsonConvert.SerializeObject(Apps);
            System.IO.File.WriteAllText("appData.json", json);
        }

        //Загрузка данных из конструктора
        private void LoadAppData()
        {
            //if (System.IO.File.Exists("appData.json"))
            //{
            //    var json = System.IO.File.ReadAllText("appData.json");
            //    var appList = JsonConvert.DeserializeObject<ObservableCollection<AppInfo>>(json);
            //    if (appList != null)
            //    {
            //        Apps = appList;
            //    }
            //}
            Apps.Clear();
            var taskbarWindows = WindowHelper.GetVisibleTaskbarWindows();
            foreach (var (title, path, icon) in taskbarWindows)
            {
                // Проверяем, есть ли уже такой путь в Url
                //if (!Apps.Any(app => app.Url.Equals(path, StringComparison.OrdinalIgnoreCase)))
                //{
                    Apps.Add(new AppInfo { Name = title, Url = path, AppIcon = icon });
                //}
            }

            // Обновляем источник данных для панели кнопок
            AppButtonsPanel.ItemsSource = Apps;
            UpdateAppVisible();
        }

        private void UpdateAppVisible()
        {
            if (Apps.Count > 0)
            {
                TooglePanel.Visibility = Visibility.Visible;
               
            }
            else
            {
                TooglePanel.Visibility = Visibility.Collapsed;
            }
        }

        //Автозагрузка приложения
        private void AddToStartup()
        {
            string appName = "DigitalClock";
            string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

            using (RegistryKey key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                key.SetValue(appName, "\"" + appPath + "\"");
            }
        }
        #endregion


        #region [Event handlers]
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

        private void Window_ContextMenuOpening(object sender, System.Windows.Controls.ContextMenuEventArgs e)
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
            SaveAppData();
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

            // Изменяем размер шрифта для меток
            if (HourLabel != null) HourLabel.FontSize = newSize;
            if (MinuteLabel != null) MinuteLabel.FontSize = newSize;
            if (SecondLabel != null) SecondLabel.FontSize = newSize;
            if (DateLabel != null) DateLabel.FontSize = newSize / 3; // Для даты устанавливаем меньший размер
            if (ToggleButton != null)
            {
                if (ToggleButton.FontSize < 16)
                {
                    ToggleButton.FontSize = newSize / 3;
                }
                ToggleButton.Width = newSize * 3;
                ToggleButton.Height = newSize;
            }

            // Изменяем размер изображений в кнопках
            foreach (var appInfo in Apps)
            {
                var button = FindButtonByAppInfo(appInfo);
                if (button != null)
                {
                    var image = FindVisualChild<System.Windows.Controls.Image>(button);
                    if (image != null)
                    {
                        image.Width = newSize * 0.75; // Пропорционально размеру шрифта
                        image.Height = newSize * 0.75; // Пропорционально размеру шрифта
                    }
                }
            }
        }

        // Вспомогательный метод для нахождения кнопки по AppInfo
        private Button FindButtonByAppInfo(AppInfo appInfo)
        {
            foreach (var item in AppButtonsPanel.Items)
            {
                var container = AppButtonsPanel.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
                if (container != null)
                {
                    var button = FindVisualChild<Button>(container);
                    // Проверка, соответствует ли кнопка данному AppInfo
                    if (button != null && button.DataContext is AppInfo info && info.Url == appInfo.Url)
                    {
                        return button;
                    }
                }
            }
            return null;
        }

        // Вспомогательный метод для нахождения дочернего элемента
        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    var childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        // Новый метод для изменения размера формы
        private void FormSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double newWidth = e.NewValue;
            double newHeight = newWidth * (Height / Width); // Сохраняем пропорции

            this.Width = newWidth;
            this.Height = newHeight;
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

            ToggleButton.Height = 50;
            ToggleButton.Width = 300;
            ToggleButton.FontSize = 16;

            if( this.ToggleButton.Content == "Ваши приложения")
            {

                this.ToggleButton.Width = 300;
                this.ToggleButton.Height = 50;
            }
            else
            {
                this.ToggleButton.Width = 20;
                this.ToggleButton.Height = 20;
            }

            // Изменяем размер изображений в кнопках
            foreach (var appInfo in Apps)
            {
                var button = FindButtonByAppInfo(appInfo);
                if (button != null)
                {
                    var image = FindVisualChild<System.Windows.Controls.Image>(button);
                    if (image != null)
                    {
                        image.Width = 60;
                        image.Height = 60;
                    }
                }
            }
        }

        private void AddAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Запрос URL приложения
            string appUrl = Microsoft.VisualBasic.Interaction.InputBox("Введите URL приложения:", "Добавить приложение");

            if (!string.IsNullOrEmpty(appUrl))
            {
                // Извлечение имени приложения из URL
                string appName = System.IO.Path.GetFileNameWithoutExtension(appUrl);
                appName = char.ToUpper(appName[0]) + appName.Substring(1); // Делаем первую букву заглавной

                // Поиск процесса по имени
                var processName = System.IO.Path.GetFileNameWithoutExtension(appUrl);
                var runningProcesses = System.Diagnostics.Process.GetProcessesByName(processName);
                Icon appIcon = null;

                if (runningProcesses.Length > 0)
                {
                    // Получаем путь к исполняемому файлу
                    string executablePath = runningProcesses[0].MainModule.FileName;

                    // Получаем иконку приложения
                    appIcon = System.Drawing.Icon.ExtractAssociatedIcon(executablePath);
                }

                // Добавляем приложение в коллекцию
                Apps.Add(new AppInfo { Name = appName, Url = appUrl, AppIcon = appIcon });
                AppButtonsPanel.ItemsSource = Apps;                               
                UpdateAppVisible();
            }
        }

        private void AppButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                AppInfo appInfo = button.DataContext as AppInfo;
                if (appInfo != null)
                {
                    var processName = System.IO.Path.GetFileNameWithoutExtension(appInfo.Url);
                    var runningProcesses = System.Diagnostics.Process.GetProcessesByName(processName);

                    if (runningProcesses.Length > 0)
                    {
                        foreach (var process in runningProcesses)
                        {
                            // Проверка, что окно активно
                            if (process.MainWindowHandle != IntPtr.Zero)
                            {
                                // Получаем путь к исполняемому файлу процесса
                                string executablePath = process.MainModule.FileName;

                                // Проверяем, совпадает ли путь с appInfo.Url
                                if (string.Equals(executablePath, appInfo.Url, StringComparison.OrdinalIgnoreCase))
                                {
                                    ShowWindow(process.MainWindowHandle, 9); // SW_RESTORE
                                    ShowWindow(process.MainWindowHandle, 3);
                                    SetForegroundWindow(process.MainWindowHandle);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (System.IO.File.Exists(appInfo.Url))
                        {
                            try
                            {
                                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(appInfo.Url) { UseShellExecute = true });
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show($"Ошибка при запуске приложения: {ex.Message}");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Указанный путь к приложению не существует.");
                        }
                    }
                }
            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            AppLinksPanel.Visibility = Visibility.Visible;
            this.ToggleButton.Content = "Ваши приложения";
            this.ToggleButton.Width = 300;
            this.ToggleButton.Height = 50;
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            AppLinksPanel.Visibility = Visibility.Collapsed;
            this.ToggleButton.Content = "...";
            this.ToggleButton.Width = 20;
            this.ToggleButton.Height = 20;
        }
        #endregion


        #region [Win32 required functions]


        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        #endregion


    }
}