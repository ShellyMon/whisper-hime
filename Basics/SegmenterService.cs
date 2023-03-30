using JiebaNet.Segmenter;

namespace SoraBot.Basics
{
    internal class SegmenterService
    {
        private static readonly JiebaSegmenter Segmenter;

        static SegmenterService()
        {
            Segmenter = new JiebaSegmenter();

            // 加载自定义词典
            var dictPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "user_dict.txt");
            Segmenter.LoadUserDict(dictPath);
        }

        internal static string[] Analyze(string input)
        {
            var segments = Segmenter.Cut(input).ToArray();
            return segments;
        }
    }
}
