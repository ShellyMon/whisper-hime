using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Model
{
    public class information
    {
        public information()
        {
        }
        /// <summary>
        /// Desc:图片id
        /// Nullable:True
        /// </summary>           
        public string pid { get; set; }

        /// <summary>
        /// Desc:
        /// Nullable:True
        /// </summary>           
        public string p { get; set; }

        /// <summary>
        /// Desc:作者id
        /// Nullable:True
        /// </summary>           
        public string uid { get; set; }

        /// <summary>
        /// Desc:标题
        /// Nullable:True
        /// </summary>           
        public string title { get; set; }

        /// <summary>
        /// Desc:作者名
        /// Nullable:True
        /// </summary>           
        public string author { get; set; }

        /// <summary>
        /// Desc:是否r18
        /// Nullable:True
        /// </summary>           
        public bool? r18 { get; set; }

        /// <summary>
        /// Desc:
        /// Nullable:True
        /// </summary>           
        public string tags { get; set; }

        /// <summary>
        /// Desc:tag表
        /// Nullable:True
        /// </summary>           
        public string imgUrls { get; set; }

        /// <summary>
        /// Desc:
        /// Nullable:True
        /// </summary>           
        public string uploadDate { get; set; }

        /// <summary>
        /// Desc:主键
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
    }
}
