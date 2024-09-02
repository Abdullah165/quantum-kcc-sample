namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;
    
    [Preserve]
    public unsafe class ProjectileSystem :SystemMainThreadFilter<ProjectileSystem.Filter>,ISignalNPCShoot
    {
        public struct Filter
        {
           public EntityRef Entity;
           public Projectile* Projectile;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.Projectile->TTL -= f.DeltaTime;
            if (filter.Projectile->TTL <= 0)
            {
                f.Destroy(filter.Entity);
            }
        }

        public unsafe void NPCShoot(Frame frame, NPC* npc, Transform3D* npcTransform, FPVector3 targetDirection)
        {
            npc->FireInterval -= frame.DeltaTime;
            if (npc->FireInterval > 0) return;
            
            var bullet = frame.Create(npc->Bullet.Id);
            
            var projectile = frame.Unsafe.GetPointer<Projectile>(bullet);
            var config = frame.FindAsset(projectile->ProjectileConfig);
            projectile->TTL = config.projectileTtl;

            var bulletTransform = frame.Unsafe.GetPointer<Transform3D>(bullet);
            bulletTransform->Position = npcTransform->Position + npc->SpawnPoint.Position;

            var bulletPhysicsBody3D = frame.Unsafe.GetPointer<PhysicsBody3D>(bullet);
            var upwardVelocity = FPVector3.Up * config.projectileInitialSpeed;
            var forwardVelocity = targetDirection * FP._1;

            bulletPhysicsBody3D->Velocity = forwardVelocity + upwardVelocity;

            npc->FireInterval = FP._0_50;
        }
    }
}
