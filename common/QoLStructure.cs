using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace Light_and_Shadow.Common
{
    public class QoLStructure
    {
        public TagCompound Tag;

        public Dictionary<string, ushort> entries = new();
        public Dictionary<ushort, ushort> typeMaping = new();

        public string BuildTime;
        public string ModVersion;
        public short Width;
        public short Height;
        public short OriginX;
        public short OriginY;
        public List<TileDefinition> StructureDatas;
        public List<string> SignTexts;

        public int GetOrAddEntry(string fullName)
        {
            if (entries.TryGetValue(fullName, out ushort entry))
            {
                return entry;
            }
            entries.Add(fullName, (ushort)entries.Count);
            return entries.Count - 1;
        }

        internal QoLStructure(string pathName)
        {
            Tag = TagIO.FromFile(pathName);
            if (Tag is null)
                return;
            Setup();
        }

        internal QoLStructure(TagCompound tag)
        {
            Tag = tag;
            Setup();
        }

        private void Setup()
        {
            BuildTime = Tag.GetString(nameof(BuildTime));
            ModVersion = Tag.GetString(nameof(ModVersion));
            Width = Tag.GetShort(nameof(Width));
            Height = Tag.GetShort(nameof(Height));
            OriginX = Tag.GetShort(nameof(OriginX));
            OriginY = Tag.GetShort(nameof(OriginY));
            SignTexts = (List<string>)(Tag.GetList<string>("SignTexts") ?? new List<string>());
            StructureDatas = (List<TileDefinition>)Tag.GetList<TileDefinition>("StructureData");
            SetupEntry();
        }

        private void SetupEntry()
        {
            var names = Tag.GetList<string>("EntriesName");
            var types = Tag.GetList<ushort>("EntriesType");
            for (int i = 0; i < names.Count; i++)
            {
                string name = names[i];
                ushort type = types[i];
                entries.Add(name, type);

                if (name.EndsWith("t")) // Tile
                {
                    string modName = name[..^1];
                    if (ModContent.TryFind<ModTile>(modName, out var tile))
                    {
                        typeMaping.Add(type, tile.Type);
                    }
                }
                else if (name.EndsWith("w")) // Wall
                {
                    string modName = name[..^1];
                    if (ModContent.TryFind<ModWall>(modName, out var wall))
                    {
                        typeMaping.Add(type, wall.Type);
                    }
                }
            }
        }

        public int ParseTileType(TileDefinition tileData)
        {
            if (tileData.VanillaTile)
                return tileData.TileIndex;
            return typeMaping.TryGetValue((ushort)tileData.TileIndex, out var value) ? value : -1;
        }

        public int ParseWallType(TileDefinition tileData)
        {
            if (tileData.VanillaWall)
                return tileData.WallIndex;
            return typeMaping.TryGetValue((ushort)tileData.WallIndex, out var value) ? value : -1;
        }
    }
}
