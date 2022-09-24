
namespace Diary.Core.Servises
{
    public static class FileManager
    {

        public static string AppPath()
        {
            return FileSystem.Current.AppDataDirectory;
        }

        public static string AppPath(string _file)
        {
            return Path.Combine(FileSystem.Current.AppDataDirectory, _file);
        }

    }
}
