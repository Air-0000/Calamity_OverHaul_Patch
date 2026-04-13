using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;
using Terraria.ModLoader.IO;
using Light_and_Shadow.Content;
using Terraria.DataStructures;

namespace Light_and_Shadow.Common.Systems
{
    public class TreeHouseGenerator : ModSystem
    {
        // 1. 存储生成位置信息
        public static List<Point> StructureSpawnPoints = new List<Point>();

        // 内存中缓存的建筑数据 (只存相对数据，不存绝对坐标)
        private List<Point16> tileRelativePositions = new List<Point16>();
        private List<ushort> tileTypes = new List<ushort>();
        private List<ushort> wallTypes = new List<ushort>();
        private List<short> frameXs = new List<short>();
        private List<short> frameYs = new List<short>();
        
        private int structureWidth = 0;
        private int structureHeight = 0;
        private bool isDataLoaded = false;

        // 2. 修改世界生成任务列表
        public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        {
            // --- 步骤 A: 寻找生成位置 ---
            int surfacePassIndex = tasks.FindIndex(t => t.Name == "Surface");
            if (surfacePassIndex != -1)
            {
                tasks.Insert(surfacePassIndex + 1, new PassLegacy("Find TreeHouse Spot", (progress, config) =>
                {
                    progress.Message = "Finding Tree House Location";
                    FindStructureLocation();
                }));
            }

            // --- 步骤 B: 实际放置建筑 ---
            int cleanupIndex = tasks.FindIndex(t => t.Name == "Final Cleanup");
            Mod.Logger.Info($"查找 Final Cleanup 索引: {cleanupIndex}"); // 关键调试

            if (cleanupIndex != -1)
            {
                Mod.Logger.Info("✅ 成功找到，开始插入树屋生成任务");
                tasks.Insert(cleanupIndex, new PassLegacy("Place TreeHouse", (progress, config) =>
                {
                    progress.Message = "Placing Tree House";
                    
                    if (!isDataLoaded) LoadStructureData();

                    PlaceStructure(Main.maxTilesX / 2, 100);
                    Mod.Logger.Info($"🏠 生成树屋在: {Main.maxTilesX / 2}, 500");
                    
                    StructureSpawnPoints.Clear();
                }));
            }
            else
            {
                Mod.Logger.Info("❌ 没有找到 Final Cleanup 任务");
            }
        }

        // 3. 寻找合适的生成位置
        private void FindStructureLocation()
        {
            int centerX = Main.maxTilesX / 2;
            int groundY = 0;

            // 寻找地表高度
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                if (Main.tile[centerX, y].HasTile && Main.tileSolid[Main.tile[centerX, y].TileType])
                {
                    groundY = y;
                    break;
                }
            }

            // 将生成点添加到列表
            StructureSpawnPoints.Add(new Point(centerX, groundY));
        }

        // 4. 加载数据

        private void LoadStructureData()
        {
            if (isDataLoaded) return;

            // 路径配置 (开发环境)
            string path = Path.Combine(Main.SavePath, "tModLoader", "..", "ModSources", Mod.Name, "Structure", "TreeHouse.qotstruct");
            path = Path.GetFullPath(path);

            if (!File.Exists(path))
            {
                Mod.Logger.Warn($"⚠️ 结构文件未找到: {path}");
                return;
            }

            try
            {
                TagCompound tag = TagIO.FromFile(path);
                
                // 获取尺寸信息
                structureWidth = tag.Get<short>("Width");
                structureHeight = tag.Get<short>("Height");
                int originX = tag.Get<short>("OriginX");
                int originY = tag.Get<short>("OriginY");

                Mod.Logger.Info($"📐 读取尺寸: {structureWidth}x{structureHeight}, 原点: {originX},{originY}");

                if (tag.TryGet("StructureData", out object structureObj) && structureObj is System.Collections.IList dataList)
                {
                    Mod.Logger.Info($"🧱 开始解析 {dataList.Count} 个数据项...");

                    for (int index = 0; index < dataList.Count; index++)
                    {
                        if (dataList[index] is TagCompound tileTag)
                        {
                            TileDefinition def = TileDefinition.DeserializeData(tileTag);

                            if (def.TileIndex != -1)
                            {
                                // --- 计算相对坐标 ---
                                // 这里计算的是相对于建筑原点的偏移量
                                int x = index % (structureWidth + 1); 
                                int y = index / (structureWidth + 1);

                                int relativeX = x - originX;
                                int relativeY = y - originY;

                                // 存入相对位置，而不是绝对位置
                                tileRelativePositions.Add(new Point16((short)relativeX, (short)relativeY));
                                tileTypes.Add((ushort)def.TileIndex);
                                wallTypes.Add((ushort)def.WallIndex);
                                frameXs.Add(def.TileFrameX);
                                frameYs.Add(def.TileFrameY);
                            }
                        }
                    }
                    isDataLoaded = true;
                    Mod.Logger.Info($"✅ 成功加载 {tileRelativePositions.Count} 个方块！");
                }
            }
            catch (Exception e)
            {
                Mod.Logger.Error($"❌ 加载结构数据失败: {e.Message}");
                Mod.Logger.Error(e.StackTrace);
            }
        }

        // 5. 实际放置逻辑 (参考 SpawnSteleUnderTree 风格)
        // 传入 centerX 和 groundY，内部处理所有偏移
        private void PlaceStructure(int centerX, int groundY)
        {
            // --- 计算建筑的锚点 (Anchor Point) ---
            // 类似于 SpawnSteleUnderTree 中的逻辑
            // startX: 建筑左上角的 X 坐标 (居中逻辑)
            // startY: 建筑底部的 Y 坐标 (坐落在地面上)
            int startX = centerX - (structureWidth / 2);
            int startY = groundY - structureHeight;

            // --- 遍历并放置 ---
            for (int i = 0; i < tileRelativePositions.Count; i++)
            {
                Point16 relPos = tileRelativePositions[i];
                
                // 计算世界绝对坐标 = 锚点 + 相对偏移
                int wx = startX + relPos.X;
                int wy = startY + relPos.Y;

                // 边界检查
                if (wx >= 0 && wx < Main.maxTilesX && wy >= 0 && wy < Main.maxTilesY)
                {
                    // 放置方块
                    ushort tileType = tileTypes[i];
                    if (tileType > 0 && tileType < TileLoader.TileCount)
                    {
                        // 强制放置，覆盖原有方块
                        WorldGen.PlaceTile(wx, wy, tileType, mute: true, forced: true, -1, 0);
                        
                        if (Main.tile[wx, wy] != null)
                        {
                            Main.tile[wx, wy].TileFrameX = frameXs[i];
                            Main.tile[wx, wy].TileFrameY = frameYs[i];
                        }
                    }

                    // 放置墙壁
                    ushort wallType = wallTypes[i];
                    Tile tile = Main.tile[wx, wy];

                    if (wallType > 0 && wallType < WallLoader.WallCount)
                    {
                        tile.WallType = wallType;
                    }
                }
            }
        }
    }
}