using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace vskeysforlocks.src
{
    public class PadlockSkeletonKeyItem : Item
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) 
                return;

            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "padlockskeletonkeyInteractions", () =>
            {
                List<ItemStack> stacks = new List<ItemStack>();

                foreach (Block block in api.World.Blocks)
                {
                    if (block.Code == null) continue;

                    if (block.HasBehavior<BlockBehaviorLockable>(true) && block.CreativeInventoryTabs != null && block.CreativeInventoryTabs.Length > 0)
                        stacks.Add(new ItemStack(block));
                }

                return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = "padlockskeletonkey:heldhelp-padlockskeletonkey",
                        HotKeyCode = "sneak",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {

            if (blockSel != null && byEntity.World.BlockAccessor.GetBlock(blockSel.Position).HasBehavior<BlockBehaviorLockable>(true))
            {
                ModSystemBlockReinforcement modBre = byEntity.World.Api.ModLoader.GetModSystem<ModSystemBlockReinforcement>();

                IPlayer player = (byEntity as EntityPlayer).Player;

                if (!modBre.IsReinforced(blockSel.Position))
                {
                    if (api is ICoreClientAPI)
                        (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", Lang.Get("padlockskeletonkey:ingameerror-cannotuseskeletonkey-notreinforced"));
                    return;
                }

                BlockReinforcement bre = modBre.GetReinforcment(blockSel.Position);

                if (!bre.Locked)
                {
                    if (api is ICoreClientAPI)
                        (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", Lang.Get("padlockskeletonkey:ingameerror-cannotuseskeletonkey-notlocked"));
                    return;
                }

                if (api is ICoreClientAPI)
                    (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", "To be implemented");

                handling = EnumHandHandling.PreventDefault;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            var baseHelp = base.GetHeldInteractionHelp(inSlot);
            return interactions.Append(baseHelp);
        }
    }
}