
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.ObjectData;
using Terraria.Enums;

namespace Calamity_OverHaul_Patch.Content.Tiles
{
    public class ShadowAnvil : ModTile
    {   
        public override bool Slope(int i, int j) => false;
        public override void SetStaticDefaults()
        {
            // 基础属性
            Main.tileSolidTop[Type] = true;
            Main.tileHammer[Type] = false; 
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            Main.tileNoAttach[Type] = true; // ✅ 关键：不让方块附着在上面
            TileID.Sets.IgnoredByNpcStepUp[Type] = true;
            Main.tileTable[Type] = true;
            Main.tileLavaDeath[Type] = false;      // 无所谓，保持默认
			AdjTiles = [TileID.WorkBenches];

			// Placement
            Main.tileFrameImportant[Type] = true;
			TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
			TileObjectData.newTile.CoordinateHeights = [18];
			TileObjectData.addTile(Type);

			AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTable);

            // 本地化名称
            AddMapEntry(new Color(100, 50, 150), CreateMapEntryName());
        }
        public override void NearbyEffects(int i, int j, bool closer)  // 发光效果
        {
            if (closer && Main.rand.NextBool(10))
            {
                Dust.NewDust(new Vector2(i * 16, j * 16), 16, 16, DustID.Shadowflame);
            }
        }
        public override void NumDust(int x, int y, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        // 挖的时候：
        // 挖坏 = 1个粒子
        // 成功挖掉 = 3个粒子

    }

}