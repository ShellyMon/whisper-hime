using SqlSugar;

namespace SoraBot.Model
{
    public class Darling
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int id { get; set; }
        /// <summary>
        /// 角色名
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public string YL { get; set; } = string.Empty;
        /// <summary>
        /// 地址
        /// </summary>
        public string path { get; set; } = string.Empty;
    }
}
