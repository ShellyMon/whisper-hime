using LiteDB;
using SoraBot.Basics;
using SoraBot.Entity;

namespace SoraBot.BLL
{
    /// <summary>
    /// 抽老婆
    /// </summary>
    internal class WifeGachaBll
    {
        internal static Wife? DoWifeGacha()
        {
            var db = Ioc.Require<ILiteDatabase>();

            var expr = BsonExpression.Create("RANDOM()");

            return db.GetCollection<Wife>()
                .Query()
                .OrderBy(expr)
                .Limit(1)
                .FirstOrDefault();
        }
    }
}
