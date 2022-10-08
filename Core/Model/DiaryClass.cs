using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diary.Core.Model
{
    public class DiaryClass
    {
        [AutoIncrement]
        [PrimaryKey]
        [NotNull]
        public int N { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Descripton { get; set; }
        public int Period { get; set; }
        public string DayPath { get; set; } //0 - all day, 1 - morning 00 - 10, 2 - lunch 10 - 18, 3 - dinner 18 - 00, 4 - am 00 - 12, 5 - pm 12 - 24
        public DateTime SaveDate { get; set; }
        public string SaveDateToShow => SaveDate.ToShortDateString();
        public bool ProgressEnable
        {
            get
            {
                if (Period > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        public double Progress { get; set; }
    }
}
