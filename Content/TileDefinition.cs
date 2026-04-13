using System;
using System.Reflection; // 需要引入反射命名空间
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace Light_and_Shadow.Content
{
    public class TileDefinition : TagSerializable
    {
        // ============ 基础方块属性 ============
        public short TileIndex;      // 方块ID（支持Mod方块）
        public short WallIndex;      // 墙壁ID（支持Mod墙壁）
        
        public short TileFrameX;     // 方块帧X坐标（用于方块样式/旋转）
        public short TileFrameY;     // 方块帧Y坐标
        
        public byte TileColor;       // 方块颜色（例如：白漆、黑漆等）
        
        public short WallFrameX;     // 墙壁帧X坐标
        public short WallFrameY;     // 墙壁帧Y坐标
        public byte WallColor;       // 墙壁颜色
        
        public TileWallBrightnessInvisibilityData CoatingData;  // 涂层数据（亮度、隐形等）

        // ============ 标志位数据1 (ExtraDatas) ============
        // 使用BitsByte存储8个布尔值
        public BitsByte ExtraDatas;
        // [0] = true: 非原版方块 (Mod方块)
        // [1] = true: 非原版墙壁 (Mod墙壁)
        // [2] = true: 被虚化状态
        // [4] = true: 实心方块 (Solid)
        // [5] = true: 半方块 (HalfBlock)
        // [6] = true: 斜坡向上, false: 斜坡向下
        // [7] = true: 斜坡向左, false: 斜坡向右

        // ============ 标志位数据2 (ExtraDatas2) ============
        // 必存项（与ExtraDatas不同）
        public BitsByte ExtraDatas2;
        // [0] = true: 有特殊平台类型
        // [1-2] 组合确定平台类型 (0, 1, 2, 3)
        // [3] = true: 红线
        // [4] = true: 绿线
        // [5] = true: 蓝线
        // [6] = true: 黄线
        // [7] = true: 有促动器

        // ============ 便捷属性 ============
        public bool RedWire => ExtraDatas2[3];
        public bool GreenWire => ExtraDatas2[4];
        public bool BlueWire => ExtraDatas2[5];
        public bool YellowWire => ExtraDatas2[6];
        public bool HasActuator => ExtraDatas2[7];

        public bool VanillaTile => !ExtraDatas[0];   // 是否原版方块
        public bool VanillaWall => !ExtraDatas[1];   // 是否原版墙壁
        
        // 方块类型（根据标志位判断）
        public BlockType BlockType
        {
            get
            {
                if (ExtraDatas[4]) return BlockType.Solid;        // 实心
                if (ExtraDatas[5]) return BlockType.HalfBlock;    // 半方块
                bool up = ExtraDatas[6];
                bool left = ExtraDatas[7];
                if (!left && !up) return BlockType.SlopeDownRight; // 斜坡
                if (!left && up) return BlockType.SlopeUpRight;
                if (left && !up) return BlockType.SlopeDownLeft;
                if (left && up) return BlockType.SlopeUpLeft;
                return BlockType.Solid;
            }
        }

        // ============ 构造函数 ============
        public TileDefinition() { }

        public TileDefinition(short tileIndex, short wallIndex, Tile tile, 
                            BitsByte extraDatas, BitsByte extraDatas2)
        {
            TileIndex = tileIndex;
            WallIndex = wallIndex;
            TileFrameX = tile.TileFrameX;
            TileFrameY = tile.TileFrameY;
            TileColor = tile.TileColor;
            WallFrameX = (short)tile.WallFrameX;
            WallFrameY = (short)tile.WallFrameY;
            WallColor = tile.WallColor;
            
            // 修正点1: 直接赋值，内部逻辑由构造函数处理
            CoatingData = tile.Get<TileWallBrightnessInvisibilityData>();
            
            ExtraDatas = extraDatas;
            ExtraDatas2 = extraDatas2;
        }

        // ============ 获取标志位数据 ============
        public static BitsByte GetExtraData(Tile tile)
        {
            BitsByte data = new();
            data[0] = tile.TileType >= TileID.Count;      // 是否Mod方块
            data[1] = tile.WallType >= WallID.Count;      // 是否Mod墙壁
            data[2] = tile.IsActuated;                     // 是否虚化
            
            var blockType = tile.BlockType;
            switch (blockType)
            {
                case BlockType.Solid:
                    data[4] = true;
                    break;
                case BlockType.HalfBlock:
                    data[5] = true;
                    break;
            }
            
            if (blockType is BlockType.SlopeUpLeft or BlockType.SlopeUpRight)
                data[6] = true;  // 向上
            if (blockType is BlockType.SlopeUpLeft or BlockType.SlopeDownLeft)
                data[7] = true;  // 向左
                
            return data;
        }

        public static BitsByte GetExtraData2(Tile tile, byte platformType)
        {
            BitsByte data = new();

            // 平台类型编码
            switch (platformType)
            {
                case 0:
                    data[0] = true;
                    break;
                case 1:
                    data[0] = true;
                    data[1] = true;
                    break;
                case 2:
                    data[0] = true;
                    data[2] = true;
                    break;
                case 3:
                    data[0] = true;
                    data[1] = true;
                    data[2] = true;
                    break;
            }
            
            // 电路信息
            data[3] = tile.RedWire;
            data[4] = tile.GreenWire;
            data[5] = tile.BlueWire;
            data[6] = tile.YellowWire;
            data[7] = tile.HasActuator;  // 促动器
            
            return data;
        }

        public int GetPlatformDrawType()
        {
            if (!ExtraDatas2[0]) return -1;
            if (!ExtraDatas2[1] && !ExtraDatas2[2]) return 0;
            if (ExtraDatas2[1] && !ExtraDatas2[2]) return 1;
            if (!ExtraDatas2[1] && ExtraDatas2[2]) return 2;
            if (ExtraDatas2[1] && ExtraDatas2[2]) return 3;
            return 0;
        }

        // ============ 序列化/反序列化 ============
        public static Func<TagCompound, TileDefinition> DESERIALIZER = s => DeserializeData(s);

        public static TileDefinition DeserializeData(TagCompound tag)
        {
            var output = new TileDefinition
            {
                TileIndex = -1,
                WallIndex = -1
            };
            
            if (tag.TryGet("TileIndex", out short tileIndex))
            {
                output.TileIndex = tileIndex;
                output.TileFrameX = tag.GetShort("TileFrameX");
                output.TileFrameY = tag.GetShort("TileFrameY");
                tag.TryGet("TileColor", out output.TileColor);
            }
            
            if (tag.TryGet("WallIndex", out short wallIndex))
            {
                output.WallIndex = wallIndex;
                output.WallFrameX = tag.GetShort("WallFrameX");
                output.WallFrameY = tag.GetShort("WallFrameY");
                tag.TryGet("WallColor", out output.WallColor);
            }
            
            // ✅ 修正点2: 使用反射修复私有字段赋值
            if (tag.TryGet("CoatingData", out byte coatingData))
            {
                // 1. 创建默认实例
                output.CoatingData = new TileWallBrightnessInvisibilityData();
                
                // 2. 获取内部私有字段 'bitpack'
                // 注意: 这是一个结构体, 我们需要通过反射修改其内部状态
                var field = typeof(TileWallBrightnessInvisibilityData)
                    .GetField("bitpack", BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (field != null)
                {
                    // 3. 创建一个新的 BitsByte 并赋值
                    var bits = new BitsByte();
                    bits[0] = ((BitsByte)coatingData)[0]; // IsTileInvisible
                    bits[1] = ((BitsByte)coatingData)[1]; // IsWallInvisible
                    bits[2] = ((BitsByte)coatingData)[2]; // IsTileFullbright (注意拼写)
                    bits[3] = ((BitsByte)coatingData)[3]; // IsWallFullbright
                    
                    // 4. 设置回结构体
                    field.SetValue(output.CoatingData, bits);
                }
            }
            
            if (output.TileIndex is not -1 || output.WallIndex is not -1)
            {
                output.ExtraDatas = tag.GetByte("ExtraDatas");
            }
            
            output.ExtraDatas2 = tag.GetByte("ExtraDatas2");
            return output;
        }

        public TagCompound SerializeData()
        {
            var tag = new TagCompound();
            
            if (TileIndex is not -1)
            {
                tag["TileIndex"] = TileIndex;
                tag["TileFrameX"] = TileFrameX;
                tag["TileFrameY"] = TileFrameY;
                if (TileColor is not 0)
                    tag["TileColor"] = TileColor;
            }
            
            if (WallIndex is not -1)
            {
                tag["WallIndex"] = WallIndex;
                tag["WallFrameX"] = WallFrameX;
                tag["WallFrameY"] = WallFrameY;
                if (WallColor is not 0)
                    tag["WallColor"] = WallColor;
            }
            
            // ✅ 修正点3: 序列化时直接获取 Data 属性
            // TileWallBrightnessInvisibilityData 有一个公共的 byte Data 属性
            tag["CoatingData"] = CoatingData.Data;

            if (TileIndex is not -1 || WallIndex is not -1)
            {
                tag["ExtraDatas"] = (byte)ExtraDatas;
            }
            
            tag["ExtraDatas2"] = (byte)ExtraDatas2;

            return tag;
        }
    }
}