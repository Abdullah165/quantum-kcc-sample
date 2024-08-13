namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>, ISignalOnComponentAdded<Player>
    {
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

            if (input->Jump.WasPressed && kcc->IsGrounded)
            {
                kcc->Jump(FPVector3.Up * player->JumpForce);
            }

            // Handle dash logic for "W" key
            if (input->MoveDirection.Y > 0 && !player->lastWPressed) 
            {
                if (player->wTapCounter > 0 && player->wTapCounter <= player->tapWindow)
                {
                   
                    player->isDashing = true;
                    player->dashFrameTimer = player->dashFrameDuration;
                    PerformDash(kcc, FPVector3.Forward, player->DashForce);
                    player->wTapCounter = 0; // Reset counter after dash
                }
                else
                {
                    
                    player->wTapCounter = 1;
                }

                
                if (frame.DeltaTime - lastWPressTime < DashCooldown)
                {
                   
                    isDashing = true;
                    dashTimer = DashDuration; 
                    PerformDash(kcc, player, FPVector3.Forward, 1000);
                }

                
                lastWPressTime = frame.DeltaTime;
            }

            // Handle dash logic for "D" key
            if (input->MoveDirection.X > 0 && !player->lastDPressed) 
            {
                if (player->dTapCounter > 0 && player->dTapCounter <= player->tapWindow)
                {
                    
                    player->isDashing = true;
                    player->dashFrameTimer = player->dashFrameDuration;
                    PerformDash(kcc, FPVector3.Right, player->DashForce);
                    player->dTapCounter = 0; // Reset counter after dash
                }

                else

                lastDPressTime = frame.DeltaTime;
            }

            // Continue dashing if in dash state
            if (isDashing)
            {
                dashTimer -= frame.DeltaTime;

                
                if (dashTimer <= FP._0)
                {
                    
                    player->dTapCounter = 1;
                }
            }

         
            if (player->isDashing)
            {
                player->dashFrameTimer--;

              
                if (player->dashFrameTimer <= 0)
                {
                    player->isDashing = false;
                }
            }

            
            if (!player->isDashing)
            {
                HandleNormalMovement(kcc, input);
            }

            
            if (player->wTapCounter > 0 && player->wTapCounter <= player->tapWindow)
            {
                player->wTapCounter++;
            }

            if (player->dTapCounter > 0 && player->dTapCounter <= player->tapWindow)
            {
                player->dTapCounter++;
            }

         
            if (player->wTapCounter > player->tapWindow) player->wTapCounter = 0;
            if (player->dTapCounter > player->tapWindow) player->dTapCounter = 0;

            // Update last pressed states
            player->lastWPressed = input->MoveDirection.Y > 0;
            player->lastDPressed = input->MoveDirection.X > 0;
        }

        private void PerformDash(KCC* kcc, FPVector3 desiredDirection, FP dashForce)
        {
            FPVector3 dashDirection = kcc->Data.TransformRotation * desiredDirection;
            kcc->AddExternalForce(dashDirection * dashForce);
        }

        private void HandleNormalMovement(KCC* kcc, Input* input)
        {
            FPVector3 movementDirection = kcc->Data.TransformRotation * input->MoveDirection.XOY;
            kcc->SetInputDirection(movementDirection); 
        }

        public void OnAdded(Frame f, EntityRef entity, Player* player)
        {
            player->DashForce = 100000;
            player->tapWindow = 15; // Number of frames within which a double-tap is detected
            player->wTapCounter = 0; 
            player->dTapCounter = 0; 

            player->isDashing = false;
            player->dashFrameDuration = 10; // Number of frames the dash lasts
            player->dashFrameTimer = 0;

            player->lastWPressed = false; // Flags to track key press state
            player->lastDPressed = false;
        }
    }
}
