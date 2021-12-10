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
                        ActionLangCode = "vskeysforlocks:heldhelp-padlockkey",
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
                long keySerial = GetKeySerial(inSlot.Itemstack); // when deserialized json item it will default to long over int
                dsc.AppendLine(Lang.Get("Key Serial: {0}", keySerial.ToString()));
            }
            else
            {
                dsc.AppendLine(Lang.Get("Key Serial: {0}", "Never used"));
            }

            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }


        public bool IsKeySerialized(ItemStack itemStack)
        {
            return (GetKeySerial(itemStack) > 9999);
        }

        // Seed so client and server can match
        public void SetKeySerial(ItemStack itemStack, long keySerial)
        {
            if (itemStack.Attributes != null)
            {
                itemStack.Attributes.SetLong("keySerial", keySerial); // when deserialized json item it will default to long over int
                if (!itemStack.Attributes.HasAttribute("keySerial"))
                    throw new Exception("This should not happen");
            }
        }

        internal long GetKeySerial(ItemStack itemStack)
        {
            if (itemStack.Attributes != null)
            {
                try
                {
                    if (!itemStack.Attributes.HasAttribute("keySerial"))
                        return -1;

                    return itemStack.Attributes.GetLong("keySerial", -1); // when deserialized json item it will default to long over int
                } catch (InvalidCastException)
                {

                    return -1;
                }
            }
            return -1;
        }

        public override WorldInteraction[] GetHeldInteractionHelp(ItemSlot inSlot)
        {
            var baseHelp = base.GetHeldInteractionHelp(inSlot);
            return interactions.Append(baseHelp);
        }
    }
}
