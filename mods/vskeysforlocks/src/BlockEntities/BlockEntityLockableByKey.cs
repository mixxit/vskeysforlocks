using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace vskeysforlocks.src
{
    public class BlockEntityLockableByKey : BlockEntity
    {
        string keySerial;
        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetString("keySerial", keySerial);
        }

        public void SetKeySerial(string serial)
        {
            this.keySerial = serial;
            this.MarkDirty();
        }

        public string GetKeySerial()
        {
            return keySerial;
        }


        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolving)
        {
            base.FromTreeAttributes(tree, worldForResolving);
            keySerial = tree.GetString("keySerial");
        }

        public override void OnBlockPlaced(ItemStack byItemStack = null)
        {
            base.OnBlockPlaced(byItemStack);
        }

        public override void OnBlockBroken()
        {
            base.OnBlockBroken();
        }

    }
}