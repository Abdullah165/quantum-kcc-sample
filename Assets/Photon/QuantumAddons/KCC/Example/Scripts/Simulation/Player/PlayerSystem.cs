namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>
    {
        private FP lastWPressTime;
        private FP lastDPressTime;
        private bool isDashing;
        private FP dashTimer; // Timer to track dash duration

        private FP DashCooldown = FP._0_05; // Time window to detect double tap for dash
        private FP DashDuration = FP._0_02; // Duration of the dash
        private FP NormalSpeed = FP._1; // Normal movement speed

        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public KCC* KCC;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            Player* player = filter.Player;
            if (player->PlayerRef.IsValid == false)
                return;

            KCC* kcc = filter.KCC;
            Input* input = frame.GetPlayerInput(player->PlayerRef);

            kcc->AddLookRotation(input->LookRotationDelta.X, input->LookRotationDelta.Y);

            // Handle dash logic
            if (input->MoveDirection.Y > 0) // "W" key pressed
            {
                // Check if the time between this press and the last press is within the DashCooldown
                if (frame.DeltaTime - lastWPressTime < DashCooldown)
                {
                    // Immediately trigger dash on second "W" press
                    isDashing = true;
                    dashTimer = DashDuration; // Start dash timer
                    PerformDash(kcc, player, FPVector3.Forward, 1000);
                }

                // Update the last press time to the current time
                lastWPressTime = frame.DeltaTime;
            }

            // Handle dash logic for "D" key
            if (input->MoveDirection.X > 0) // "D" key pressed
            {
                if (frame.DeltaTime - lastDPressTime <= DashCooldown)
                {
                    isDashing = true;
                    dashTimer = DashDuration; // Start dash timer
                    PerformDash(kcc,player, FPVector3.Right,1000); // Dash to the right
                }

                lastDPressTime = frame.DeltaTime;
            }

            // Continue dashing if in dash state
            if (isDashing)
            {
                dashTimer -= frame.DeltaTime;

                // End dash if the timer runs out
                if (dashTimer <= FP._0)
                {
                    isDashing = false;
                }
            }

            // Handle normal movement if not dashing
            if (!isDashing)
            {
                HandleNormalMovement(kcc, input);
            }

            // Handle Jumping
            if (input->Jump.WasPressed == true && kcc->IsGrounded == true)
            {
                kcc->Jump(FPVector3.Up * player->JumpForce);
            }
        }

        private void PerformDash(KCC* kcc, Player* player,FPVector3 desiredDirection, FP dashForce)
        {
            FPVector3 dashDirection = kcc->Data.TransformRotation * desiredDirection;
            kcc->AddExternalForce(dashDirection * dashForce); // Apply force for dash
        }

        private void HandleNormalMovement(KCC* kcc, Input* input)
        {
            FPVector3 movementDirection = kcc->Data.TransformRotation * input->MoveDirection.XOY;
            kcc->SetInputDirection(movementDirection); // Apply normal speed
        }
    }
}
