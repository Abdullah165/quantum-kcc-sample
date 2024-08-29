namespace Quantum
{
    using System;
    using Photon.Deterministic;

    [Serializable]
    public unsafe partial class AttackAction : AIAction
    {
        public override unsafe void Execute(Frame frame, EntityRef e, ref AIContext aiContext)
        {
           //TODO: Chasing and Shooting player.
        }
    }
}