using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace Clock
{
    public partial class MainWindow : Window
    {
        #region [Fields and Properties]


        private DispatcherTimer timer;
        private bool isPinned = false;
        private bool isTopMost = false;

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
            if (System.IO.File.Exists("appData.json"))
            {
                var json = System.IO.File.ReadAllText("appData.json");
                var appList = JsonConvert.DeserializeObject<ObservableCollection<AppInfo>>(json);
                if (appList != null)
                {
                    Apps = appList;
                    AppButtonsPanel.ItemsSource = Apps;

                    UpdateAppVisible();
                }
            }
        }

        private void UpdateAppVisible()
        {
            if (Apps.Count > 0)
            {
                TooglePanel.Visibility = Visibility.Visible;
                this.ToggleButton.IsChecked = true; // Устанавливаем состояние Checked
                ToggleButton_Checked(this, new RoutedEventArgs()); // Явно вызываем метод
            }
            else
            {
                this.ToggleButton.IsChecked = false; // Устанавливаем состояние Unchecked
                ToggleButton_Unchecked(this, new RoutedEventArgs()); // Явно вызываем метод
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

        private void AddAppMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Запрос URL приложения
            string appUrl = Microsoft.VisualBasic.Interaction.InputBox("Введите URL приложения:", "Добавить приложение");

            if (!string.IsNullOrEmpty(appUrl))
            {
                // Извлечение имени приложения из URL
                string appName = System.IO.Path.GetFileNameWithoutExtension(appUrl);
                appName = char.ToUpper(appName[0]) + appName.Substring(1); // Делаем первую букву заглавной

                // Добавляем приложение в коллекцию
                Apps.Add(new AppInfo { Name = appName, Url = appUrl });
                AppButtonsPanel.ItemsSource = Apps; // Установка источника данных                                
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
                        // Если приложение уже запущено, активируем его
                        foreach (var process in runningProcesses)
                        {
                            if (process.MainWindowHandle != IntPtr.Zero)
                            {
                                // Попробуем восстановить окно, если оно минимизировано
                                ShowWindow(process.MainWindowHandle, 9); // SW_RESTORE
                                ShowWindow(process.MainWindowHandle, 3);
                                SetForegroundWindow(process.MainWindowHandle);
                                return; // Завершаем метод после активации
                            }
                        }
                    }
                    else
                    {
                        // Проверяем, существует ли приложение
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