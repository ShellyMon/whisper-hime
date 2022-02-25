using Sisters.WudiLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace whisper_hime
{
    public delegate Task SauceImplementEventHandler(HttpApiClient api, Sisters.WudiLib.Posts.Message e);
    public delegate void SauceEventHandler(HttpApiClient api, Sisters.WudiLib.Posts.Message e);
    public class Pub
    {
        public event SauceImplementEventHandler SauceImplementEvent;
        public event SauceEventHandler SauceEvent;
        public async Task SauceAsync(HttpApiClient api, Sisters.WudiLib.Posts.Message e,int num)
        {
            if (num == 0)
            {
                if (SauceEvent != null)
                {
                    SauceEvent(api, e);
                }
            }
            else
            {
                if (SauceImplementEvent != null)
                {
                    await SauceImplementEvent(api, e);
                }
            }
        }
    }
}
