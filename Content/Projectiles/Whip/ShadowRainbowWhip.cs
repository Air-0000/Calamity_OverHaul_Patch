using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Projectiles.Whip
{
    public class ShadowRainbowWhip : ModProjectile
    {
        // 最大射程（原版彩虹鞭子 = 220）
        private const float MAX_RANGE = 440f;

        public override void SetDefaults()
        {
            // 完全继承原版彩虹鞭子
            Projectile.CloneDefaults(ProjectileID.RainbowWhip);
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            // 覆盖射程（核心）
            AIType = ProjectileID.RainbowWhip;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            // 强制限制鞭子最长长度，防止原版AI限制
            if (Projectile.ai[1] > MAX_RANGE)
                Projectile.ai[1] = MAX_RANGE;

            // 你原本的自定义内容（保留不动）
            // float detectRange = 400f;
            // int attackCooldown = 30;
            // bool canWallDetect = true;
            // bool canPenetrateWall = false;
        }

        // 让原版自动绘制，不自己写
        public override bool PreDraw(ref Color lightColor) => true;
    }
}