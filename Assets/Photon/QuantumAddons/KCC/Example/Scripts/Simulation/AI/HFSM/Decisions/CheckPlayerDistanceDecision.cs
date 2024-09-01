namespace Quantum
{
    using Photon.Deterministic;
    using System;

    [Serializable]
    public class CheckPlayerDistanceDecision : HFSMDecision
    {
        private FP _safeRange = 20;

        public override unsafe bool Decide(Frame frame, EntityRef target, ref AIContext aiContext)
        {
            var players = frame.GetComponentIterator<Player>();
            Transform3D* npcTransform = frame.Unsafe.GetPointer<Transform3D>(target);
            NPC* npc = frame.Unsafe.GetPointer<NPC>(target);

            foreach (var player in players)
            {
                Transform3D* playerTransform = frame.Unsafe.GetPointer<Transform3D>(player.Entity);

                FP distanceSqr = (playerTransform->Position - npcTransform->Position).SqrMagnitude;

                // Check if the player is outside the danger range
                if (distanceSqr >= _safeRange * _safeRange)
                {
                    //UnityEngine.Debug.Log($"NPC {target} did not detect player close.");
                    npc->IsPlayerAway = true;
                    return true; 
                }

                npc->IsPlayerAway = false;
            }
            return false; 
        }
    }
}