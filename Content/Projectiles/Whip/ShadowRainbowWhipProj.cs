using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace Light_and_Shadow.Content.Projectiles.Whip
{
    public class ShadowRainbowWhipProj : ModProjectile
    {
        private Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.SummonMeleeSpeed;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 8;
        }

        public override void AI()
        {
            if (!Owner.active || Owner.dead)
            {
                Projectile.Kill();
                return;
            }

            Owner.heldProj = Projectile.whoAmI;
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // 鞭子挥甩角度（真正的鞭子弧线）
            float num = 40f;
            Projectile.ai[0] += 1f;
            float progress = Projectile.ai[0] / num;
            progress = Utils.Clamp(progress, 0f, 1f);

            // 从玩家手向鼠标方向挥出
            Vector2 aimPos = Vector2.Lerp(Owner.MountedCenter, Main.MouseWorld, progress);
            Vector2 dir = aimPos - Owner.MountedCenter;
            if (dir != Vector2.Zero)
                dir.Normalize();

            float length = 110f * progress;
            Projectile.Center = Owner.MountedCenter + dir * length;
            Projectile.rotation = dir.ToRotation() + MathHelper.PiOver2;

            // 挥完消失
            if (Projectile.ai[0] >= num)
                Projectile.Kill();
        }

        // 线段碰撞 = 真正鞭子判定
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(
                new Vector2(targetHitbox.X, targetHitbox.Y),
                targetHitbox.Size(),
                Owner.MountedCenter,
                Projectile.Center,
                22,
                ref _
            );
        }

        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RainbowWhip;
    }
}