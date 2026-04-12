using JetBrains.Annotations;
using Light_and_Shadow.Content.Projectiles.Minions;
using Microsoft.Xna.Framework;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;
using static Light_and_Shadow.Content.Items.GameStageHelper;
using Light_and_Shadow.Content.Items.Stuffs;


namespace Light_and_Shadow.Content.Items.Weapons
{
    
    public class ShadowRainbowWhip : ModItem
    {   
        
        public override void SetDefaults()
        {
            // maybe change in future: 伤害、击退、使用时间、动画时间、射出速度
            Item.damage = 10; 
            Item.knockBack = 2f;
            Item.useTime = 40;
            Item.useAnimation = 30;
            Item.shootSpeed = 1f;

            Item.width = 30;
            Item.height = 30;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item152;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.shoot = ProjectileID.RainbowWhip;
            Item.DamageType = DamageClass.SummonMeleeSpeed;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
            => velocity *= WhipCalculator.WhipVelocity();
        
        public override void ModifyWeaponDamage(Player player, ref StatModifier damage) 
            => damage.Flat += WhipCalculator.WhipDamage(
                DamageType: WhipCalculator.BasicDamageTypeId);
        // public override void UpdateInventory(Player player)
        // {
            
            
        // }

        // public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        // {
            
        // }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddTile(ModContent.TileType<Tiles.ShadowAnvil>())
                .AddIngredient(ModContent.ItemType<RainbowCrystal>(), 1)
                .AddIngredient(ModContent.ItemType<ShadowCrystal>(), 6)
                .AddIngredient(ModContent.ItemType<LightCrystal>(), 6)
                .Register();
        }
        /// <summary>
        /// 当玩家手持这个物品时，持续调用
        /// </summary>
        public override void HoldItem(Player player)
        {
            // 1. 多人游戏核心：只让“自己”运行，服务器/别人不运行
            if (player.whoAmI != Main.myPlayer)
                return;

            // 2. 计时器：每帧 +1，用来控制召唤频率
            player.taxTimer++;

            // 3. 每 30 帧（0.5 秒）执行一次
            if (player.taxTimer >= 30)
            {
                player.taxTimer = 0; // 重置计时器

                // 4. 检查：仆从位没满，才允许召唤
                if (player.numMinions < player.maxMinions)
                {
                    int realDamage = WhipCalculator.WhipDamage(
                        DamageType: WhipCalculator.SummonDamageTypeId);
                    // 5. 创建召唤物（安全、标准、多人兼容）
                    int proj = Projectile.NewProjectile(
                        player.GetSource_ItemUse(Item),  // 来源：物品使用（正确不报错）
                        player.Center,                   // 生成位置：玩家中心
                        Vector2.Zero,                    // 移动速度：静止生成
                        ModContent.ProjectileType<RainbowSummon>(), // 召唤物实体
                        realDamage,                  // 伤害
                        Item.knockBack,                 // 击退
                        player.whoAmI                   // 归属玩家
                    );

                    // 6. 必须设置：标记为仆从（吃鞭子、正确继承属性）
                    Main.projectile[proj].minion = true;
                    Main.projectile[proj].originalDamage = realDamage;
                }
            }
        }



    }

    public  class WhipDamage : DamageClass  
    {

    }

    public class WhipCalculator // 计算类
    {
        public const int BasicDamageTypeId = 0;
        public const int AdvancedDamageTypeId = 1;
        public const int SummonDamageTypeId = 2;

        public static int WhipDamage(int DamageType)
        {       
            int basicDamage = 10;
            GameStage stage = GetCurrentGameStage();
            int intStage = (int)stage;
            if (DamageType == BasicDamageTypeId)
            {
                if (stage < GameStage.HardModePrePlantera) 
                {
                    return (int)(basicDamage * (1f + Math.Log( intStage + 1)));
                    
                }
                if (stage < GameStage.PostPlantera) 
                {
                    return (int)(basicDamage * (  intStage * 0.8f * ( intStage-6) ));
                    
                }
                return (int)((basicDamage+1) * (  intStage * 0.8f * ( intStage-6) ));
            }
            else if(DamageType == AdvancedDamageTypeId)
            {
                if (stage >=  GameStage.PostMoonLord)
                {
                    return basicDamage + ( intStage - 10)*5;
                }
            }
            else if (DamageType == SummonDamageTypeId)
            {
                if (stage <  GameStage.HardModePrePlantera) 
                {
                    return (int)(basicDamage * (1f + Math.Log( intStage + 1)) * 0.5f);
                    
                }
                if (stage <  GameStage.PostPlantera) 
                {
                    return (int)(basicDamage * (  intStage * 0.8f * ( intStage-6) ));
                    
                }
                return (int)((basicDamage+1) * (  intStage * 0.8f * ( intStage-6) ));
            }
            return 0;
            
        }

        public static float WhipVelocity()
        {
            GameStage stage = GetCurrentGameStage();
            float multiplier = 1;
            if (stage >=  GameStage.PostEyeOfCthulhu)
                multiplier = 2;
            if (stage >=  GameStage.HardModePrePlantera)
                multiplier = 3;
            if (stage >=  GameStage.PostMechanicalBoss) 
                multiplier = 3.5f;
            if (stage >=  GameStage.PostPlantera) 
                multiplier = 4;
            if (stage >= GameStage.PostGolem)
                multiplier = 5;
            if (stage >= GameStage.PostFishron)
                multiplier = 6;
            if (stage >=  GameStage.PostMoonLord) 
                multiplier = 7;
            return multiplier;
        }
    }
    //11111
    public class WhipAdvancedDamageHandler : GlobalNPC // 额外伤害buff处理 
    {
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            // 1. 检查击中者是不是召唤物 (Minion)
            if (projectile.minion && npc.HasBuff(BuffID.RainbowWhipNPCDebuff))
            {
                modifiers.FlatBonusDamage += WhipCalculator.WhipDamage(WhipCalculator.AdvancedDamageTypeId); 
            }
        }
    }

}