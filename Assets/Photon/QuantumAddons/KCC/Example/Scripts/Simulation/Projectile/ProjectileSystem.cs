namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class ProjectileSystem : SystemMainThreadFilter<ProjectileSystem.Filter>, ISignalNPCShoot,
        ISignalOnCollisionProjectileHitPlayer
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Projectile* Projectile;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            var players = f.GetComponentIterator<Player>();

            foreach (var player in players)
            {
                var playerTransform = f.Unsafe.GetPointer<Transform3D>(player.Entity);

                if (playerTransform == null) continue;

                var projectile = filter.Projectile;
                var projectileTransform = filter.Transform;

                if (projectileTransform == null || projectile == null) continue;

                // Check if the projectile has a recorded hit position
                if (projectile->PlayerHitPosition != FPVector3.Zero)
                {
                    // Get the player's current position
                    var playerPosition = playerTransform->Position;

                    // Update the projectile's position relative to the player's current position
                    projectileTransform->Position = playerPosition + projectile->PlayerHitPosition;
                }
            }

            // if (!filter.Projectile->IsBulletHitPlayer)
            // {
            //     // filter.Projectile->TTL -= f.DeltaTime;
            //     // if (filter.Projectile->TTL <= 0)
            //     // {
            //     //     f.Destroy(filter.Entity);
            //     // }
            // }
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

            if (bulletPhysicsBody3D != null)
                bulletPhysicsBody3D->Velocity = forwardVelocity + upwardVelocity;

            npc->FireInterval = FP._0_50;
        }

        public void OnCollisionProjectileHitPlayer(Frame f, CollisionInfo3D info)
        {
            var projectile = f.Unsafe.GetPointer<Projectile>(info.Entity);
            var projectileTransform = f.Unsafe.GetPointer<Transform3D>(info.Entity);
            var projectilePhysicsCollider3D = f.Unsafe.GetPointer<PhysicsCollider3D>(info.Entity);
            var playerTransform = f.Unsafe.GetPointer<Transform3D>(info.Other);
            var player = f.Unsafe.GetPointer<Player>(info.Other);

            if (projectilePhysicsCollider3D != null)
            {
                projectilePhysicsCollider3D->Enabled = false;
            }

            var collisionPoint = projectileTransform->Position;

            var localImpactPoint = playerTransform->InverseTransformPoint(collisionPoint);

            // Store the relative position (impact point) in the projectile
            projectile->PlayerHitPosition = localImpactPoint;

            player->AttachedProjectilesCount++;
        }
    }
}