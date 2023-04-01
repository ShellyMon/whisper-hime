using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Model
{
    public class DarlingUser
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        /// <summary>
        /// qq号
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string qq { get; set; } = string.Empty;
        /// <summary>
        /// 抽取的日期
        /// </summary>
        public string day { get; set; } = string.Empty;
        /// <summary>
        /// 外键id
        /// </summary>
        public int DarlingID { get; set; }
    }
}
