using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;

namespace vskeysforlocks.src
{
    public class VSKeysForLocksMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("padlockkey", typeof(PadlockKeyItem));
            api.RegisterItemClass("padlockskeletonkey", typeof(PadlockSkeletonKeyItem));
            api.RegisterItemClass("padlockrekeyerkit", typeof(PadlockRekeyerKitItem));
            
        }
    }
}
