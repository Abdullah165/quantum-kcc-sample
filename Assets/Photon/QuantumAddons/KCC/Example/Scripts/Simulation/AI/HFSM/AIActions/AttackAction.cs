namespace Quantum
{
    using System;
    using Photon.Deterministic;

    [Serializable]
    public unsafe partial class AttackAction : AIAction
    {
        private FP _attackRange = 20;

        public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
        {
            NPC* npc = frame.Unsafe.GetPointer<NPC>(e);

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

                    frame.Signals.NPCShoot(npc, npcTransform, toPlayerPosition);
                }
                else
                {
                    npc->IsPlayerClose = false;
                }

                break;
            }
        }
    }
}