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
        public DateTime SaveDate { get; set; }
    }
}
