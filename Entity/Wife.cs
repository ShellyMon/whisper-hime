using LiteDB;

namespace WhisperHime.Entity
{
    /// <summary>
    /// 老婆实体
    /// </summary>
    public class Wife
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 名字
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 台词
        /// </summary>
        public string Sentence { get; set; } = string.Empty;

        /// <summary>
        /// 图片地址
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
