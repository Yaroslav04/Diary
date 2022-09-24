using Cerebrum.Core.Servises;

namespace Diary;

public partial class App : Application
{
    static DataBase dataBase;
    public static DataBase DataBase
    {
        get
        {
            if (dataBase == null)
            {
                dataBase = new DataBase(FileManager.AppPath(), new List<string> { "ObjectDataBase.db3"});
            }
            return dataBase;
        }
    }

    static bool startSwitch = true;
    public static bool StartSwitch
    {
        get
        {
            return startSwitch;
        }
        set
        {
            startSwitch = value;
        }
    }

    public static List<string> Types
    {
        get
        {
            return new List<string>
            {
                "Здоровье",
                "Еда",
                "Спорт",
                "Работа",
                "Финансы",
                "Ментальность",
            };
        }
    }
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
