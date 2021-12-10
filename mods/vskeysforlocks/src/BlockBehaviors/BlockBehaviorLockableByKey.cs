using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace vskeysforlocks.src.BlockBehaviors
{
    public class BlockBehaviorLockableByKey : BlockBehavior
    {
        public BlockBehaviorLockableByKey(Block block) : base(block)
        {

        }

        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            BlockEntity block = byPlayer.Entity.World.BlockAccessor.GetBlockEntity(blockSel.Position);
            if (block != null && block is BlockEntityLockableByKey)
            {
                if (!String.IsNullOrEmpty(((BlockEntityLockableByKey)block).GetKeySerial()) && Convert.ToInt64(((BlockEntityLockableByKey)block).GetKeySerial()) > 9999)
                {
                    if (!HasRightKeyInHands(world, byPlayer, ((BlockEntityLockableByKey)block).GetKeySerial()))
                    {
                        if (world.Side == EnumAppSide.Client)
                            (world.Api as ICoreClientAPI).TriggerIngameError(this, "locked", Lang.Get("vskeysforlocks:ingameerror-cannotusekey-nokey", ((BlockEntityLockableByKey)block).GetKeySerial()));

                        handling = EnumHandling.PreventSubsequent;
                        return false;
                    }
                }
            }

            return base.OnBlockInteractStart(world, byPlayer, blockSel, ref handling);
        }

        private bool HasRightKeyInHands(IWorldAccessor world, IPlayer player, string keySerialToFind)
        {
            if (String.IsNullOrEmpty(keySerialToFind))
                return false;

            return HasRightKeyInSlot(world, player, player.Entity.RightHandItemSlot, keySerialToFind) || HasRightKeyInSlot(world, player, player.Entity.LeftHandItemSlot, keySerialToFind);
        }

        private bool HasRightKeyInSlot(IWorldAccessor world, IPlayer player, ItemSlot itemSlot, string keySerialToFind)
        {
            if (itemSlot == null)
                return false;

            if (itemSlot.Itemstack == null)
                return false;

            if (itemSlot.Itemstack == null || itemSlot.Itemstack == null || (itemSlot.Itemstack.Item as PadlockKeyItem) == null)
                return false;

            if (!(itemSlot.Itemstack.Item as PadlockKeyItem).IsKeySerialized(itemSlot.Itemstack))
            {
                return false;
            }

            if ((itemSlot.Itemstack.Item as PadlockKeyItem).GetKeySerial(itemSlot.Itemstack).ToString().Equals(keySerialToFind))
            {
                return true;
            }

            return false;
        }
    }
}
