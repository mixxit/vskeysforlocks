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
            api.RegisterCommand("forcekeyserial", "forces a key serial", "", CmdForceKeySerial, "root");
            api.RegisterCommand("getkeycode", "gets key code", "", CmdGetKeyCode, "root");
            base.StartServerSide(api);
        }

        private void CmdGetKeyCode(IServerPlayer player, int groupId, CmdArgs args)
        {
            if (player.Entity.RightHandItemSlot != null && player.Entity.RightHandItemSlot.Itemstack != null && player.Entity.RightHandItemSlot.Itemstack.Attributes != null)
            {
                player.SendMessage(groupId, $"Right Hand: ", EnumChatType.CommandSuccess);
                foreach (var att in player.Entity.RightHandItemSlot.Itemstack.Attributes)
                {
                    player.SendMessage(groupId, $"{att.Key} {att.Value}: ", EnumChatType.CommandSuccess);

                }
            }

            if (player.Entity.LeftHandItemSlot != null && player.Entity.LeftHandItemSlot.Itemstack != null && player.Entity.LeftHandItemSlot.Itemstack.Attributes !=null)
            {
                player.SendMessage(groupId, $"Left Hand: ", EnumChatType.CommandSuccess);
                foreach (var att in player.Entity.LeftHandItemSlot.Itemstack.Attributes)
                {
                    player.SendMessage(groupId, $"{att.Key} {att.Value}: ", EnumChatType.CommandSuccess);
                }
            }

        }

        private void CmdForceKeySerial(IServerPlayer player, int groupId, CmdArgs args)
        {
            if (args.Length < 1)
            {
                player.SendMessage(groupId, $"Missing argument (key serial)", EnumChatType.CommandError);
                return;
            }

            try
            {
                long serial = Convert.ToInt64(args[0]);

                TryForceKeySerialInSlot(player.Entity.RightHandItemSlot, serial);
                TryForceKeySerialInSlot(player.Entity.LeftHandItemSlot, serial);

                player.SendMessage(groupId, $"Attempt to force key serial completed, please check serial", EnumChatType.CommandSuccess);
            } catch (Exception)
            {
                player.SendMessage(groupId, $"Invalid argument (key serial)", EnumChatType.CommandError);
                return;
            }

            

        }

        private void TryForceKeySerialInSlot(ItemSlot itemSlot, long serial)
        {
            if (itemSlot.Itemstack == null || itemSlot.Itemstack.Item == null || (itemSlot.Itemstack.Item as PadlockKeyItem) == null)
                return;

            // already fixed
            if ((itemSlot.Itemstack.Item as PadlockKeyItem).IsKeySerialized(itemSlot.Itemstack))
                return;

            (itemSlot.Itemstack.Item as PadlockKeyItem).SetKeySerial(itemSlot.Itemstack, serial);
            itemSlot.MarkDirty();
        }

        private void CmdFixBrokenKey(IServerPlayer player, int groupId, CmdArgs args)
        {
            TryFixKeyInSlot(player.Entity.RightHandItemSlot);
            TryFixKeyInSlot(player.Entity.LeftHandItemSlot);

            player.SendMessage(groupId, $"Attempt to fix key completed, please check serial", EnumChatType.CommandSuccess);
            
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
