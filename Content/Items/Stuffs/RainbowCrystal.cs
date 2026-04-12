using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Items.Stuffs
{
    public class RainbowCrystal : ModItem
    {   
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.Diamond);
            Item.DefaultToPlaceableTile(ModContent.TileType<Content.Tiles.RainbowCrystalTile>());
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(ModContent.TileType<Tiles.ShadowAnvil>())
                .AddIngredient(ItemID.Amber, 5)
                .AddIngredient(ItemID.Ruby, 5)
                .AddIngredient(ItemID.Topaz, 5)
                .AddIngredient(ItemID.Amethyst, 5)
                .AddIngredient(ItemID.Sapphire, 5)
                .AddIngredient(ItemID.Emerald, 5)
                .Register();
        }
    }
}