using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace vskeysforlocks.src
{
    public class PadlockKeyItem : Item
    {
        WorldInteraction[] interactions;

        public override void OnLoaded(ICoreAPI api)
        {
            if (api.Side != EnumAppSide.Client) 
                return;

            ICoreClientAPI capi = api as ICoreClientAPI;

            interactions = ObjectCacheUtil.GetOrCreate(api, "padlockkeyInteractions", () =>
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
                        ActionLangCode = "padlockkey:heldhelp-padlockkey",
                        HotKeyCode = "sneak",
                        MouseButton = EnumMouseButton.Right,
                        Itemstacks = stacks.ToArray()
                    }
                };
            });
        }
        
        public override void OnCollected(ItemStack stack, Entity entity)
        {
            base.OnCollected(stack, entity);
            if (api.Side.IsServer())
                SetKeySerial(stack);
        }

        public override void OnCreatedByCrafting(ItemSlot[] allInputslots, ItemSlot outputSlot, GridRecipe byRecipe)
        {
            if (api.Side.IsServer())
                SetKeySerial(outputSlot.Itemstack);

            base.OnCreatedByCrafting(allInputslots, outputSlot, byRecipe);
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
                        (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", Lang.Get("padlockkey:ingameerror-cannotusekey-notreinforced"));
                    return;
                }

                BlockReinforcement bre = modBre.GetReinforcment(blockSel.Position);

                if (!bre.Locked)
                {
                    if (api is ICoreClientAPI)
                        (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", Lang.Get("padlockkey:ingameerror-cannotusekey-notlocked"));
                    return;
                }

                if (api is ICoreClientAPI)
                    (api as ICoreClientAPI).TriggerIngameError(this, "incomplete", "To be implemented");

                handling = EnumHandHandling.PreventDefault;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public void SetKeySerial(ItemStack itemStack)
        {
            if (!api.Side.IsServer())
                return;

            if (itemStack.Attributes != null)
            {
                if (!itemStack.Attributes.HasAttribute("keySerial"))
                {
                    itemStack.Attributes.SetInt("keySerial", api.World.Rand.Next(10000, 99999));
                    if (!itemStack.Attributes.HasAttribute("keySerial"))
                        throw new Exception("This should not happen");
                }
            }
        }

        public int GetKeySerial(ItemStack itemStack)
        {
            if (itemStack.Attributes != null)
            {
                if (!itemStack.Attributes.HasAttribute("keySerial"))
                    return -1;

                return itemStack.Attributes.GetInt("keySerial", -1);
            }
            return -1;
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            int keySerial = GetKeySerial(inSlot.Itemstack);
            if (keySerial > 9999)
                dsc.AppendLine(Lang.Get("Key Serial: {0}", keySerial.ToString()));

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            var baseHelp = base.GetHeldInteractionHelp(inSlot);
            return interactions.Append(baseHelp);
        }
    }
}