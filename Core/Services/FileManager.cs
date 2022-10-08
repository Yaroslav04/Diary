
namespace Diary.Core.Servises
{
    public static class FileManager
    {

        public static string AppPath()
        {
            return @"/storage/emulated/0/Diary/"; ;
        }

        public static string AppPath(string _file)
        {
            return Path.Combine(AppPath(), _file);
        }

    }
}
