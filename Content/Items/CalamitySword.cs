using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items
{
	// This is a basic item template.
	// Please see tModLoader's ExampleMod for every other example:
	// https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
	public class CalamitySword : ModItem
	{
        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.Light_and_Shadow.hjson' file.
        public override void SetDefaults()
        {
            Item.width = 1000;
            Item.height = 400;
            Item.useTime = 100;
            Item.useAnimation = 200;
            Item.useStyle = ItemUseStyleID.Swing;

            // 鞭子必备两行
            Item.noMelee = true;
            Item.noUseGraphic = true;

            // 直接射原版彩虹鞭子，测试是否能发射
            Item.shoot = ProjectileID.RainbowWhip;
            Item.shootSpeed = 1f;

            Item.DamageType = DamageClass.SummonMeleeSpeed;
            Item.autoReuse = true;
        }

        public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DirtBlock, 10);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
