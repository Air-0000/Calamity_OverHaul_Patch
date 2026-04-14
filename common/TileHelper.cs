using Terraria;
using Terraria.ID;
using Terraria.WorldBuilding;
namespace Light_and_Shadow.Common
{
    public static class TileHelper
    {
        internal static bool Place3x2NoSyncDresser(int x, int y, ushort type, int style = 0)
        {
            if (x < 5 || x > Main.maxTilesX - 5 || y < 5 || y > Main.maxTilesY - 5)
                return false;

            WorldGen.PlaceDresserDirect(x, y, type, style,-1);
            return true;
        }
    }
}
