namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    /// <summary>
    /// NPC system navigates NPCs between random waypoints, used for performance testing.
    /// </summary>
    [Preserve]
    public unsafe class NPCSystem : SystemMainThreadFilter<NPCSystem.Filter>, ISignalOnComponentAdded<HFSMAgent>,ISignalOnComponentAdded<NPC>
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public NPC* NPC;
            public KCC* KCC;
            public HFSMAgent* HFSMAgent;
        }

        private EntityRef playerEntity; // Reference to the player entity
        private Transform3D* playerTransform; // Reference to the player's Transform component

        public void OnAdded(Frame f, EntityRef entity, HFSMAgent* component)
        {
            HFSMRoot hfsmRoot = f.FindAsset<HFSMRoot>(component->Data.Root.Id);
            HFSMManager.Init(f, entity, hfsmRoot);
        }
        
        public override void Update(Frame frame, ref Filter filter)
        {
            HFSMManager.Update(frame, frame.DeltaTime, filter.Entity);

            var entityRef = filter.Entity;
            var transform = filter.Transform;
            var npc = filter.NPC;
            var kcc = filter.KCC;
            
            var players = frame.GetComponentIterator<Player>();
            foreach (var player in players)
            {
                var playerTransform = frame.Unsafe.GetPointer<Transform3D>(player.Entity);
                var distanceSqr = (playerTransform->Position - transform->Position).SqrMagnitude;

                if (distanceSqr <= 20 * 20) // Check if within attack range
                {
                    // Trigger the Attack event
                    HFSMManager.TriggerEvent(frame, filter.Entity, "Attack");
                    break; 
                }
            }


            // Timer used for detection of NPC being stuck.
            npc->CheckTime += frame.DeltaTime;

            if (npc->CheckTime > 1 && kcc->IsGrounded == true && npc->IsPlayerAway) 
            {
                // Try jumping after 1 second.
                kcc->Jump(FPVector3.Up * 5);
            }

            if (npc->TargetPosition == default || npc->CheckTime > 5)
            {
                // The NPC doesn't have active waypoint of is stuck for more than 5 second => find new waypoint.
                npc->CheckTime = 0;
                npc->TargetPosition = frame.GetRandomWaypoint().Position;
            }

            var toCheckPosition = (npc->CheckPosition - transform->Position).XOZ;
            if (toCheckPosition.SqrMagnitude > 1)
            {
                // Reset timer when the KCC is 1m away from last check position.
                npc->CheckPosition = transform->Position;
                npc->CheckTime = 0;
            }

            var toTargetPosition = (npc->TargetPosition - transform->Position).XOZ;
            if (toTargetPosition.SqrMagnitude < 1)
            {
                // Target waypoint almost reached, let's reset and find a new one.
                npc->TargetPosition = default;
            }

            if (npc->IsPlayerClose)
            {
                //Stop Moving for Shooting the Player.
                kcc->SetKinematicSpeed(0);
            }
            if(npc->IsPlayerAway)
            {
                //Patrol again.
                kcc->SetLookRotation(FPQuaternion.LookRotation(toTargetPosition).AsEuler.XY);
                kcc->SetInputDirection(toTargetPosition);
                kcc->SetKinematicSpeed(4);
            }
        }

        public void OnAdded(Frame f, EntityRef entity, NPC* component)
        {
            component->FireInterval = FP._0_50;
        }
    }
}