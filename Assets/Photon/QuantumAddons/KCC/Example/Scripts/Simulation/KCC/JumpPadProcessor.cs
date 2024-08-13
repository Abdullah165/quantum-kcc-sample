namespace Quantum
{
	using Photon.Deterministic;

	/// <summary>
	/// JumpPad adds an impulse to a player KCC upon collision.
	/// </summary>
	public unsafe class JumpPadProcessor : KCCProcessor
	{
		public FP Impulse;

		public override bool OnEnter(KCCContext context, KCCProcessorInfo processorInfo, KCCOverlapHit overlapHit)
		{
			// By default the player is pushed upwards.
			FPVector3 impulse = FPVector3.Up * Impulse;

			if (processorInfo.GetStaticCollider(context.Frame, out MapStaticCollider3D collider) == true && collider.ShapeType == Shape3DType.Sphere)
			{
				// For sphere colliders we calculate direction from sphere origin to KCC root position.
				FPVector3 offset = context.KCC->Position - collider.Position;
				if (offset.SqrMagnitude > FP.EN1)
				{
					impulse = offset.Normalized * Impulse;
				}
			}

			// Clear kinematic and dynamic velocity entirely.
			context.KCC->SetKinematicVelocity(FPVector3.Zero);
			context.KCC->SetDynamicVelocity(FPVector3.Zero);

			// Applying the impulse.
			context.KCC->AddExternalImpulse(impulse);

			// Returning true = the KCC starts tracking collision with this processor/collider.
			// The OnExit() callback will be called when the KCC leaves (probably next frame).
			return true;
		}
	}
}
