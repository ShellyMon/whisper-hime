using System.Collections.Generic;

namespace WhisperHime.Entity
{
    /// <summary>
    /// 插画实体
    /// </summary>
    public class Illustration
    {
        /// <summary>
        /// 主键
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 第几张图片
        /// </summary>
        public int p { get; set; }
        /// <summary>
        /// 作品ID
        /// </summary>
        public long Pid { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 标签
        /// </summary>
        public List<string> Tags { get; set; } = new();

        /// <summary>
        /// 是否成人作品
        /// </summary>
        public bool IsAdult { get; set; }

        /// <summary>
        /// 原图下载地址
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// 图片校验值
        /// </summary>
        public string Hash { get; set; } = string.Empty;

        /// <summary>
        /// 作者ID
        /// </summary>
        public long Uid { get; set; }

        /// <summary>
        /// 作者名字
        /// </summary>
        public string Artist { get; set; } = string.Empty;
    }
}
