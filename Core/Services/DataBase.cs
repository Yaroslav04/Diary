
namespace Cerebrum.Core.Servises
{
    public class DataBase
    {
        readonly SQLiteAsyncConnection objectDataBase;

        public DataBase(string _connectionString, List<string> _dataBaseName)
        {

            objectDataBase = new SQLiteAsyncConnection(Path.Combine(_connectionString, _dataBaseName[0]));
            objectDataBase.CreateTableAsync<DiaryClass>().Wait();
        }

        #region Object

        public Task<int> SaveObjectAsync(DiaryClass _object)
        {
            try
            {
                return objectDataBase.InsertAsync(_object);
            }
            catch
            {
                return null;
            }
        }
        public Task<int> DeleteObjectAsync(DiaryClass _object)
        {
            try
            {
                return objectDataBase.DeleteAsync(_object);
            }
            catch
            {
                return null;
            }

        }
        public Task<int> UpdateObjectAsync(DiaryClass _object)
        {
            try
            {
                return objectDataBase.UpdateAsync(_object);
            }
            catch
            {
                return null;
            }

        }

        public async Task<List<DiaryClass>> GetObjectsAsync()
        {
            return await objectDataBase.Table<DiaryClass>().ToListAsync();
        }

        public async Task<List<DiaryClass>> GetObjectsAsync(string _type)
        {
            return await objectDataBase.Table<DiaryClass>().Where(x => x.Type == _type).ToListAsync();
        }

        public async Task<List<DiaryClass>> GetObjectsAsync(string _type, string _name)
        {
            return await objectDataBase.Table<DiaryClass>().Where(x => x.Type == _type & x.Name == _name).OrderByDescending(x => x.SaveDate).ToListAsync();
        }

        public async Task<DiaryClass> GetLastObjectsAsync(string _type, string _name, string _dayPath)
        {
            return await objectDataBase.Table<DiaryClass>().Where(x => x.Type == _type & x.Name == _name & x.DayPath == _dayPath).OrderByDescending(x => x.SaveDate).FirstOrDefaultAsync();
        }

        public async Task<DiaryClass> GetObjectAsync(int _id)
        {
            return await objectDataBase.Table<DiaryClass>().Where(x => x.N == _id).FirstOrDefaultAsync();
        }

        public async Task<List<string>> GetNamesFromObjectsAsync(string _type)
        {
            List<string> result = new List<string>();
            List<DiaryClass> objects = new List<DiaryClass>();
            objects =  await objectDataBase.Table<DiaryClass>().Where(x => x.Type == _type).ToListAsync();
            if (objects.Count == 0)
            {
                return null;
            }
            else
            {
                foreach (var item in objects)
                {
                    result.Add(item.Name);
                }

                result = result.Distinct().ToList();
                result = result.OrderBy(x => x).ToList();
                return result;
            }
        }

        public async Task<int> GetLastPeriodFromObjectsAsync(string _type, string _name)
        {
            var result = await objectDataBase.Table<DiaryClass>().Where(x => x.Type == _type & x.Name == _name).FirstOrDefaultAsync();
            if (result != null)
            {
                return result.Period;
            }
            else
            {
                return 0;
            }
        }

        #endregion
    }
}
