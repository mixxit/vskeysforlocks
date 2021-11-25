using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using vskeysforlocks.src.BlockBehaviors;

namespace vskeysforlocks.src
{
    public class PadlockWithKeyMechanismItem : Item
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client)
                return;

            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "padlockkeyerInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null) continue;

                    if (block.HasBehavior<BlockBehaviorLockableByKey>(true) && block.CreativeInventoryTabs != null && block.CreativeInventoryTabs.Length > 0)
                        stacks.Add(new ItemStack(block));
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "padlockwithkeymechanism:heldhelp-padlockwithkeymechanism",
                        HotKeyCode = "sneak",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (byEntity.Api.Side == EnumAppSide.Client)
            {
                if (blockSel != null && byEntity.World.BlockAccessor.GetBlock(blockSel.Position).HasBehavior<BlockBehaviorLockableByKey>(true))
                {
                    // We want to do the state from the server side
                    // but it will only be handled client side unless we prevent the default action
                    // this allows OnHeldInteractStart to fire on server
                    handling = EnumHandHandling.PreventDefault;
                    return;
                }
            }

            if (blockSel != null && byEntity.World.BlockAccessor.GetBlock(blockSel.Position).HasBehavior<BlockBehaviorLockableByKey>(true))
            {
                ModSystemBlockReinforcement modBre = byEntity.World.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
                IServerPlayer player = (IServerPlayer)(byEntity as EntityPlayer).Player;

                if (TryFitKeyMechanism(player, blockSel, player.Entity.LeftHandItemSlot.Itemstack))
                {
                    api.World.PlaySoundAt(new AssetLocation("sounds/tool/padlock.ogg"), player, player, false, 12);
                    slot.TakeOut(1);
                    slot.MarkDirty();
                }

                handling = EnumHandHandling.PreventDefault;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        private bool TryFitKeyMechanism(IServerPlayer player, BlockSelection blockSelection, ItemStack keyItemStack)
        {
            if (player == null || blockSelection == null)
                return false;

            BlockEntity block = player.Entity.World.BlockAccessor.GetBlockEntity(blockSelection.Position);

            if (block != null && block is BlockEntityLockableByKey)
            {
                if (!String.IsNullOrEmpty(((BlockEntityLockableByKey)block).GetKeySerial()) && Convert.ToInt32(((BlockEntityLockableByKey)block).GetKeySerial()) > 9999)
                {
                    player.SendIngameError("incomplete", Lang.Get("padlockwithkeymechanism:ingameerror-cannotuse-alreadyhaspadlock"));
                    return false;
                }
            }

            if (keyItemStack == null || keyItemStack.Item == null || (keyItemStack.Item as PadlockKeyItem) == null)
            {
                player.SendIngameError("incomplete", Lang.Get("padlockwithkeymechanism:ingameerror-cannotuse-missingkey"));
                return false;
            }

            ModSystemBlockReinforcement modBre = player.Entity.World.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();
            if (modBre == null)
            {
                player.SendIngameError("incomplete", Lang.Get("padlockwithkeymechanism:ingameerror-cannotuse-notreinforced"));
                return false;
            }

            if (!modBre.IsReinforced(blockSelection.Position))
            {
                player.SendIngameError("incomplete", Lang.Get("padlockwithkeymechanism:ingameerror-cannotuse-notreinforced"));
                return false;
            }

            // Serialize Key if not set
            if (!(keyItemStack.Item as PadlockKeyItem).IsKeySerialized(keyItemStack))
            {
                (player.Entity.LeftHandItemSlot.Itemstack.Item as PadlockKeyItem).SetKeySerial(player.Entity.LeftHandItemSlot.Itemstack);
                player.Entity.LeftHandItemSlot.MarkDirty();
                player.SendMessage(GlobalConstants.GeneralChatGroup,Lang.Get("padlockkey:keyset", (player.Entity.LeftHandItemSlot.Itemstack.Item as PadlockKeyItem).GetKeySerial(keyItemStack)), EnumChatType.OwnMessage);
            }

            var padLockKey = ((PadlockKeyItem)keyItemStack.Item);

            if (!padLockKey.IsKeySerialized(keyItemStack))
            {
                if (api is ICoreClientAPI)
                    (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", Lang.Get("padlockwithkeymechanism:ingameerror-cannotuse-missingkey"));
                return false;
            }

            if (block != null && block is BlockEntityLockableByKey)
            {
                ((BlockEntityLockableByKey)block).SetKeySerial(padLockKey.GetKeySerial(keyItemStack).ToString());
                player.SendMessage(GlobalConstants.GeneralChatGroup, Lang.Get("padlockwithkeymechanism:keyset", padLockKey.GetKeySerial(keyItemStack)), EnumChatType.OwnMessage);
                return true;
            }

            return false;
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            var baseHelp = base.GetHeldInteractionHelp(inSlot);
            return interactions.Append(baseHelp);
        }
    }
}