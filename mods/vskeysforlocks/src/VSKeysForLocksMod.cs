using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using vskeysforlocks.src.BlockBehaviors;

namespace vskeysforlocks.src
{
    public class VSKeysForLocksMod : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockBehaviorClass("BlockBehaviorLockableByKey", typeof(BlockBehaviorLockableByKey));
            api.RegisterBlockEntityClass("BlockEntityLockableByKey", typeof(BlockEntityLockableByKey));
            api.RegisterItemClass("padlockkey", typeof(PadlockKeyItem));
            api.RegisterItemClass("padlockwithkeymechanism", typeof(PadlockWithKeyMechanismItem));
        }
    }
}
