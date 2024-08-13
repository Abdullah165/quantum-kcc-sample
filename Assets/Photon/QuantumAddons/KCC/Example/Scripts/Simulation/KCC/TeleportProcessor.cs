namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// Teleport moves the KCC by an offset upon collision.
	/// </summary>
	public unsafe class TeleportProcessor : KCCProcessor
	{
		public FP TeleportOffset;

		public override bool OnEnter(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapHit overlapHit)
		{
			// TODO: Change the implementation so it teleports the player to a position defined by an entity (Transform3D).

			FPVector3 teleportOffset = context.KCC->Data.TransformDirection * TeleportOffset;

			if (processorInfo.GetStaticCollider(context.Frame, out MapStaticCollider3D collider) == true)
			{
				teleportOffset = collider.Rotation * FPVector3.Forward * TeleportOffset;
			}

			// Clear kinematic and dynamic velocity entirely.
			context.KCC->SetKinematicVelocity(FPVector3.Zero);
			context.KCC->SetDynamicVelocity(FPVector3.Zero);

			context.KCC->Teleport(context.KCC->Position + teleportOffset);

			return true;
		}
	}
}
