using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Model
{
    internal class LoliconApiResult<TResponse> where TResponse : class, new()
    {
        public string Error { get; set; }
        public TResponse Data { get; set; }
    }
}
