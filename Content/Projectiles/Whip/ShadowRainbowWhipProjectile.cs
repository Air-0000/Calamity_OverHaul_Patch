using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Projectiles.Whip
{
    public class ShadowRainbowWhipProjectile : ModProjectile
    {
        // 静态设置
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }

        // 属性设置（纯官方数值，可同步改范围）
        public override void SetDefaults()
        {
            // DefaultToWhip 完整展开
            Projectile.DefaultToWhip();
            Projectile.extraUpdates = 1;
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            // 万花筒原版属性
            Projectile.damage = 70;
            Projectile.knockBack = 3.5f;
            Projectile.timeLeft = 240;
            Projectile.ownerHitCheck = true;
            Projectile.light = 1f;

            // ====================== 攻击范围（同步修改，这里改！） ======================
            Projectile.WhipSettings.Segments = 16;
            Projectile.WhipSettings.RangeMultiplier = 1.2f;
        }

        // 官方AI计时器
        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }

        // 原生鞭子AI
        public override bool PreAI()
        {
            return true;
        }

        // 彩虹粒子特效
        public override void PostAI()
        {
            if (Timer < Projectile.WhipSettings.Segments * 3f)
            {
                for (int i = 0; i < 2; i++)
                {
                    Vector2 pos = Projectile.Center + Main.rand.NextVector2Circular(10, 10);
                    Vector2 vel = Main.rand.NextVector2Circular(2, 2);
                    Dust dust = Dust.NewDustDirect(pos, 0, 0, DustID.RainbowTorch, vel.X, vel.Y, 0, Color.White, 1.2f);
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                }
            }
        }

        // 命中特效
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.RainbowTorch, 0f, 0f, 0, Color.White, 1.5f);
                dust.noGravity = true;
            }
        }

        // ====================== 【修复核心】正确的万花筒渲染，100%显示 ======================
        public override bool PreDraw(ref Color lightColor)
        {
            // 强制加载原版万花筒纹理
            Main.instance.LoadProjectile(ProjectileID.RainbowWhip);
            Texture2D texture = TextureAssets.Projectile[ProjectileID.RainbowWhip].Value;

            // 获取鞭子节点
            List<Vector2> points = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, points);

            // 原版万花筒渲染参数（正确大小，不压扁、不缩小）
            SpriteEffects flip = Projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Vector2 origin = new Vector2(texture.Width / 2, texture.Height / 2);

            // 绘制鞭子主体
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 pos = points[i] - Main.screenPosition;
                float rot = points.Count > i + 1 ? (points[i + 1] - points[i]).ToRotation() : Projectile.rotation;
                Color color = Lighting.GetColor(points[i].ToTileCoordinates());

                // 原版万花筒尺寸，自动适配范围
                Main.EntitySpriteDraw(texture, pos, null, color, rot, origin, 1f, flip, 0);
            }

            return false;
        }
    }
}