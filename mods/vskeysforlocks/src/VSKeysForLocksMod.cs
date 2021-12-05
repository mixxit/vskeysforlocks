using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
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

        public override void StartServerSide(ICoreServerAPI api)
        {
            api.RegisterCommand("fixbrokenkey", "corrects non-long keys", "", CmdFixBrokenKey, "root");
            base.StartServerSide(api);
        }

        private void CmdFixBrokenKey(IServerPlayer player, int groupId, CmdArgs args)
        {
            TryFixKeyInSlot(player.Entity.RightHandItemSlot);
            TryFixKeyInSlot(player.Entity.LeftHandItemSlot);

            player.SendMessage(groupId, $"Side: {player.Entity.World.Side.ToString()}", EnumChatType.OwnMessage);
            
        }

        private void TryFixKeyInSlot(ItemSlot itemSlot)
        {
            if (itemSlot.Itemstack == null || itemSlot.Itemstack.Item == null || (itemSlot.Itemstack.Item as PadlockKeyItem) == null)
                return;

            // already fixed
            if ((itemSlot.Itemstack.Item as PadlockKeyItem).IsKeySerialized(itemSlot.Itemstack))
                return;

            if (!itemSlot.Itemstack.Attributes.HasAttribute("keySerial"))
                return;

            var serial = itemSlot.Itemstack.Attributes.GetInt("keySerial"); // when deserialized json item it will default to long over int
            (itemSlot.Itemstack.Item as PadlockKeyItem).SetKeySerial(itemSlot.Itemstack, (long)serial);
            itemSlot.MarkDirty();
        }
    }
}
