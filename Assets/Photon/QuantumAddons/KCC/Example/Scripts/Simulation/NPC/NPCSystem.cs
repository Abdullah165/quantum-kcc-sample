namespace Quantum
{
	using Photon.Deterministic;
	using UnityEngine.Scripting;

	/// <summary>
	/// NPC system navigates NPCs between random waypoints, used for performance testing.
	/// </summary>
	[Preserve]
	public unsafe class NPCSystem : SystemMainThreadFilter<NPCSystem.Filter>
	{
		public struct Filter
		{
			public EntityRef    Entity;
			public Transform3D* Transform;
			public NPC*         NPC;
			public KCC*         KCC;
			public HFSMAgent* HFSMAgent;
			public Player*       Player;
		}

		public void OnAdded(Frame f, EntityRef entity, HFSMAgent* component)
		{
			HFSMRoot hfsmRoot = f.FindAsset<HFSMRoot>(component->Data.Root.Id);
			HFSMManager.Init(f, entity, hfsmRoot);
		}
		
		public override void Update(Frame frame, ref Filter filter)
		{
			Transform3D* transform = filter.Transform;
			NPC*         npc       = filter.NPC;
			KCC*         kcc       = filter.KCC;
			Player*      player = filter.Player;
			
			HFSMManager.Update(frame, frame.DeltaTime, filter.Entity);


			// Timer used for detection of NPC being stuck.
			npc->CheckTime += frame.DeltaTime;

			if (npc->CheckTime > 1 && kcc->IsGrounded == true)
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

			FPVector3 toCheckPosition = (npc->CheckPosition - transform->Position).XOZ;
			if (toCheckPosition.SqrMagnitude > 1)
			{
				// Reset timer when the KCC is 1m away from last check position.
				npc->CheckPosition = transform->Position;
				npc->CheckTime = 0;
			}

			FPVector3 toTargetPosition = (npc->TargetPosition - transform->Position).XOZ;
			if (toTargetPosition.SqrMagnitude < 1)
			{
				// Target waypoint almost reached, let's reset and find a new one.
				npc->TargetPosition = default;
			}

			FPVector3 playerPosition = player->currentPosition;
			if ((playerPosition - transform->Position).SqrMagnitude < 10)
			{
				//Chase the player.
				kcc->SetLookRotation(FPQuaternion.LookRotation(player->currentPosition).AsEuler.XY);
				kcc->SetInputDirection(player->currentPosition);
				kcc->SetKinematicSpeed(4);
			}
			else
			{
				//Patrol again.
				kcc->SetLookRotation(FPQuaternion.LookRotation(toTargetPosition).AsEuler.XY);
				kcc->SetInputDirection(toTargetPosition);
				kcc->SetKinematicSpeed(4);
			}
		}
	} 
}
