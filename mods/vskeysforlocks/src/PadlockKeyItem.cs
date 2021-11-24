using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Vintagestory.GameContent;
using vskeysforlocks.src.BlockBehaviors;

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

                    if (block.HasBehavior<BlockBehaviorLockableByKey>(true) && block.CreativeInventoryTabs != null && block.CreativeInventoryTabs.Length > 0)
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

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            if (IsKeySerialized(inSlot.Itemstack))
            {
                int keySerial = GetKeySerial(inSlot.Itemstack);
                dsc.AppendLine(Lang.Get("Key Serial: {0}", keySerial.ToString()));
            }
            else
            {
                dsc.AppendLine(Lang.Get("Key Serial: {0}", "Never used"));
            }

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
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

        public bool IsKeySerialized(ItemStack itemStack)
        {
            return (GetKeySerial(itemStack) > 9999);
        }

        public void SetKeySerial(ItemStack itemStack)
        {
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

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            var baseHelp = base.GetHeldInteractionHelp(inSlot);
            return interactions.Append(baseHelp);
        }
    }
}
