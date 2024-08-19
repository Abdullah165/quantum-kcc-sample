namespace Quantum
{
    public unsafe class ClimbingSurfaceCollisionsSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D, ISignalOnCollisionExit3D
    {
        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<Player>(info.Entity, out var player))
            {
                if (f.Unsafe.TryGetPointer<ClimbingSurface>(info.Other, out _))
                {
                    f.Signals.OnCollisionPlayerHitClimbingSurface(info, player);
                }
            }

        }

        public void OnCollisionExit3D(Frame f, ExitInfo3D info)
        {
            if (f.Unsafe.TryGetPointer<Player>(info.Entity, out var player))
            {
                if (f.Unsafe.TryGetPointer<ClimbingSurface>(info.Other, out _))
                {
                    f.Signals.OnCollisionPlayerExitClimbingSurface(info, player);
                }
            }
        }
    }
}
