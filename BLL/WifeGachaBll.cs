using LiteDB;
using WhisperHime.Basics;
using WhisperHime.Entity;

namespace WhisperHime.BLL
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
