using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.GameContent.Biomes;

namespace Calamity_Overhaul_Patch.common.Systems
{
    public class LivingTreeSteleGenerator : ModSystem
    {
        public static List<int> LivingTreeXPositions = new List<int>();

        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            int vanillaTreePassIndex = tasks.FindIndex(t => t.Name == "Living Trees");

            if (vanillaTreePassIndex != -1)
            {
                tasks.Insert(vanillaTreePassIndex + 1, new PassLegacy("Living Tree Stele Shrine", (progress, config) =>
                {
                    progress.Message = "Generating Living Trees";
                    GenerateLivingTreesWithStele();
                }));
            }

            int finalCleanupIndex = tasks.FindIndex(t => t.Name == "Final Cleanup");
            if (finalCleanupIndex != -1)
            {
                tasks.Insert(finalCleanupIndex, new PassLegacy("Generate Stele Shrines", (progress, config) =>
                {
                    progress.Message = "Generating Stele Shrines";

                    foreach (int x in LivingTreeXPositions)
                    {
                        SpawnSteleUnderTree(x);
                    }
                    LivingTreeXPositions.Clear();
                }));
            }
        }

        private void GenerateLivingTreesWithStele()
        {
            var rand = WorldGen.genRand;
            int centerAvoidDistance = 200;
            double worldSizeScale = (double)Main.maxTilesX / 4200.0;

            int treeCount = rand.Next(0, (int)(2.0 * worldSizeScale) + 1);
            if (treeCount == 0)
                treeCount++;

            if (Main.drunkWorld)
                treeCount += (int)(2.0 * worldSizeScale);
            else if (Main.tenthAnniversaryWorld)
                treeCount += (int)(3.0 * worldSizeScale);
            else if (Main.remixWorld)
                treeCount += (int)(2.0 * worldSizeScale);

            for (int i = 0; i < treeCount; i++)
            {
                bool success = false;
                int attempts = 0;

                while (!success && attempts < Main.maxTilesX / 2)
                {
                    attempts++;
                    int x = rand.Next(WorldGen.beachDistance, Main.maxTilesX - WorldGen.beachDistance);

                    if (WorldGen.tenthAnniversaryWorldGen && !WorldGen.remixWorldGen)
                        x = rand.Next((int)(Main.maxTilesX * 0.15), (int)(Main.maxTilesX * 0.85));

                    if (Math.Abs(x - Main.maxTilesX / 2) < centerAvoidDistance)
                        continue;

                    int y = FindSurfaceHeight(x);
                    if (y < 150)
                        continue;

                    if (!IsValidLivingTreeTerrain(x, y))
                        continue;

                    success = WorldGen.GrowLivingTree(x, y);

                    if (success)
                    {
                        LivingTreeXPositions.Add(x);
                    }
                }
            }

            Main.tileSolid[TileID.LeafBlock] = false;
        }

        private int FindSurfaceHeight(int x)
        {
            int y = 0;
            while (y < Main.worldSurface && !Main.tile[x, y].HasTile)
                y++;

            if (Main.tile[x, y].TileType == 0)
                y--;

            return y;
        }

        private bool IsValidLivingTreeTerrain(int centerX, int centerY)
        {
            for (int x = centerX - 50; x < centerX + 50; x++)
            {
                for (int y = centerY - 50; y < centerY + 50; y++)
                {
                    Tile tile = Main.tile[x, y];
                    if (!tile.HasTile) continue;

                    switch (tile.TileType)
                    {
                        case TileID.SnowBlock:
                        case TileID.IceBlock:
                        case TileID.Sand:
                        case TileID.CorruptSandstone:
                        case TileID.Crimsand:
                        case TileID.HallowSandstone:
                        case TileID.JungleGrass:
                        case TileID.Mud:
                            return false;
                    }
                }
            }
            return true;
        }

        private void SpawnSteleUnderTree(int treeX)
        {
            EnchantedSwordBiome swordShrine = GenVars.configuration.CreateBiome<EnchantedSwordBiome>();
            Point origin = new Point(treeX, (int)GenVars.worldSurface + WorldGen.genRand.Next(50, 100));
            swordShrine.Place(origin, GenVars.structures);

            int centerX = origin.X;
            int centerY = origin.Y;

            for (int i = 0; i < 40; i++)
            {
                centerY++;
                Tile tile = Framing.GetTileSafely(centerX, centerY);
                Tile below = Framing.GetTileSafely(centerX, centerY + 1);

                if (below.HasTile && Main.tileSolid[below.TileType])
                {
                    int platformY = centerY; // 地面
                    int swordY = centerY - 1;

                    // 删掉原来的剑
                    Tile swordTile = Framing.GetTileSafely(centerX, swordY);
                    if (swordTile.HasTile && swordTile.TileType == TileID.Stone)
                    {
                        WorldGen.KillTile(centerX, swordY, false, false, true);
                    }

                    // 清空石碑位置
                    for (int x = -1; x <= 0; x++)
                    {
                        for (int y = -2; y <= 0; y++)
                        {
                            int cx = centerX + x;
                            int cy = platformY + y;
                            if (WorldGen.InWorld(cx, cy))
                            {
                                WorldGen.KillTile(cx, cy, false, false, true);
                            }
                        }
                    }

                    // ✅ 关键：放在地面 platformY，不是 swordY
                    
                    // ==============================
                    // 新版：石碑顶部向上 + 左右2格随机空洞（原版剑冢风格）
                    // ==============================
                    for (int y = platformY - 2; y > (int)GenVars.worldSurface - 5; y--)
                    {
                        // 中间一格必空
                        WorldGen.KillTile(centerX, y, false, false, true);

                        // 左边一格随机空
                        if (WorldGen.genRand.NextBool())
                        {
                            WorldGen.KillTile(centerX - 1, y, false, false, true);
                        }

                        // 右边一格随机空
                        if (WorldGen.genRand.NextBool())
                        {
                            WorldGen.KillTile(centerX + 1, y, false, false, true);
                        }
                    }
                    
                    WorldGen.PlaceObject(centerX, platformY, ModContent.TileType<Calamity_OverHaul_Patch.Content.Tiles.MysteriousStoneStele>(), true);

                    break;
                }
            }
        }
    }
}