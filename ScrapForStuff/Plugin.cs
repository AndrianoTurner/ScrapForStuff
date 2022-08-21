using PulsarModLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PulsarModLoader.PulsarMod;

namespace ScrapForStuff
{
    public class Plugin : PulsarMod
    {
        public override string HarmonyIdentifier()
        {
            return "com.ScrapForStuff";
        }
        public override string Author => "Rayman";
        public override string ShortDescription => "Makes sold stuff at scrapyard turn into scrap on warp";
        public override string Name => "ScrapForStuff";
        public override string Version => "0.0.1";
    }
}
