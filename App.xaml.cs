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
                "Медикаменты",
                "Еда",
                "Спорт",
                "Работа",
                "Финансы",
                "Ментальность",
            };
        }
    }

    public static List<string> DayPaths
    {
        get
        {
            return new List<string>
            {
                /*0*/"Весь день",
                /*1*/"Утро",
                /*2*/"Обед",
                /*3*/"Вечер",
                /*4*/"До обеда",
                /*5*/"После обеда",
            };
        }
    }
    public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}
}
