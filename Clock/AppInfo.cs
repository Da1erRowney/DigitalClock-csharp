using System.Drawing; // Не забудьте добавить это пространство имен

namespace Clock
{
    public partial class MainWindow
    {
        // Класс для хранения данных приложений
        public class AppInfo
        {
            public string Name { get; set; }
            public string Url { get; set; }
            public Icon AppIcon { get; set; } // Добавлено свойство для иконки
        }
    }
}