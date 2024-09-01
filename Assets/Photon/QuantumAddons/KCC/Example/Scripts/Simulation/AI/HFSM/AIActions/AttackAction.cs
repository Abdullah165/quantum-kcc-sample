namespace Quantum
{
    using System;
    using Photon.Deterministic;

    [Serializable]
    public unsafe partial class AttackAction : AIAction
    {
        private FP _attackRange = 20;
        private FP _fireInterval = FP._0_99;
        private FP TTL = 1;

        public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
        {
            NPC* npc = frame.Unsafe.GetPointer<NPC>(e);
            //npc->IsPlayerClose = true;

            Transform3D* npcTransform = frame.Unsafe.GetPointer<Transform3D>(e);
            KCC* kcc = frame.Unsafe.GetPointer<KCC>(e);

            var players = frame.GetComponentIterator<Player>();

            foreach (var player in players)
            {
                Transform3D* playerTransform = frame.Unsafe.GetPointer<Transform3D>(player.Entity);


                FP distanceSqr = (playerTransform->Position - npcTransform->Position).SqrMagnitude;

                FPVector3 toPlayerPosition = (playerTransform->Position - npcTransform->Position).XOZ;
                // Check if the NPC is within the attack range
                if (distanceSqr <= _attackRange * _attackRange)
                {
                    // NPC is close enough to attack the player
                    npc->IsPlayerClose = true;
                    kcc->SetLookRotation(FPQuaternion.LookRotation(toPlayerPosition).AsEuler.XY);
                    kcc->SetInputDirection(toPlayerPosition);

                    FireBullet(frame, npc, npcTransform,toPlayerPosition);
                }
                else
                {
                    npc->IsPlayerClose = false;
                }

                break;
            }
        }

        private unsafe void FireBullet(Frame frame, NPC* npc, Transform3D* npcTransform,FPVector3 targetDirection)
        {
            npc->FireInterval -= frame.DeltaTime;
            if (npc->FireInterval <= 0)
            {
                var bullet = frame.Create(npc->Bullet.Id);
                
                Transform3D* bulletTransform = frame.Unsafe.GetPointer<Transform3D>(bullet);
                bulletTransform->Position = npcTransform->Position + npc->SpawnPoint.Position;

                PhysicsBody3D* bulletPhysicsBody3D = frame.Unsafe.GetPointer<PhysicsBody3D>(bullet);
                bulletPhysicsBody3D->Velocity =  targetDirection * 5;

                npc->FireInterval = FP._0_10;

                //DestroyBullet(frame, bullet);
            }
        }

        private unsafe void DestroyBullet(Frame frame, EntityRef entityRef)
        {
            TTL -= frame.DeltaTime;
            if (TTL <= 0)
            {
                frame.Destroy(entityRef);
            }
        }
    }
}