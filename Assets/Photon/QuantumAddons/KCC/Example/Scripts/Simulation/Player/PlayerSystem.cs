namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSystem : SystemMainThreadFilter<PlayerSystem.Filter>, ISignalOnComponentAdded<Player>,
        ISignalOnCollisionPlayerHitClimbingSurface, ISignalOnCollisionPlayerExitClimbingSurface
    {
        public struct Filter
        {
            public EntityRef Entity;
            public Player* Player;
            public KCC* KCC;
            public Transform3D* Transform;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            Player* player = filter.Player;
            if (player->PlayerRef.IsValid == false)
                return;

            KCC* kcc = filter.KCC;
            Input* input = frame.GetPlayerInput(player->PlayerRef);

            player->currentPosition = filter.Transform->Position;

            kcc->AddLookRotation(input->LookRotationDelta.X, input->LookRotationDelta.Y);

            AdjustSpeed(player);

            if (player->isClimbing)
            {
                HandleClimbing(kcc, frame, ref filter, player, input);
            }
            else
            {
                EnvironmentProcessor.SetGravity(new FPVector3(0, -20, 0));
                HandleNormalMovement(kcc, input, ref filter);
            }


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
                    PerformDash(kcc, FPVector3.Forward, player->DashForce,player);
                    player->wTapCounter = 0; // Reset counter after dash
                }
                else
                {
                    player->wTapCounter = 1;
                }
            }

            // Handle dash logic for "S" key
            if (input->MoveDirection.Y < 0 && !player->lastSPressed)
            {
                if (player->sTapCounter > 0 && player->sTapCounter <= player->tapWindow)
                {
                    player->isDashing = true;
                    player->dashFrameTimer = player->dashFrameDuration;
                    PerformDash(kcc, FPVector3.Back, player->DashForce,player);
                    player->sTapCounter = 0; // Reset counter after dash
                }
                else
                {
                    player->sTapCounter = 1;
                }
            }

            // Handle dash logic for "D" key
            if (input->MoveDirection.X > 0 && !player->lastDPressed)
            {
                if (player->dTapCounter > 0 && player->dTapCounter <= player->tapWindow)
                {
                    player->isDashing = true;
                    player->dashFrameTimer = player->dashFrameDuration;
                    PerformDash(kcc, FPVector3.Right, player->DashForce,player);
                    player->dTapCounter = 0; // Reset counter after dash
                }
                else
                {
                    player->dTapCounter = 1;
                }
            }

            // Handle dash logic for "a" key
            if (input->MoveDirection.X < 0 && !player->lastAPressed)
            {
                if (player->aTapCounter > 0 && player->aTapCounter <= player->tapWindow)
                {
                    player->isDashing = true;
                    player->dashFrameTimer = player->dashFrameDuration;
                    PerformDash(kcc, FPVector3.Left, player->DashForce,player);
                    player->aTapCounter = 0; // Reset counter after dash
                }
                else
                {
                    player->aTapCounter = 1;
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


            if (!player->isDashing && kcc->IsGrounded)
            {
                HandleNormalMovement(kcc, input, ref filter);
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


            if (player->aTapCounter > 0 && player->aTapCounter <= player->tapWindow)
            {
                player->aTapCounter++;
            }

            if (player->sTapCounter > 0 && player->sTapCounter <= player->tapWindow)
            {
                player->sTapCounter++;
            }


            if (player->aTapCounter > player->tapWindow) player->aTapCounter = 0;
            if (player->sTapCounter > player->tapWindow) player->sTapCounter = 0;

            // Update last pressed states
            player->lastWPressed = input->MoveDirection.Y > 0;
            player->lastDPressed = input->MoveDirection.X > 0;
            player->lastAPressed = input->MoveDirection.X < 0;
            player->lastSPressed = input->MoveDirection.Y < 0;
        }
        
        public void OnAdded(Frame f, EntityRef entity, Player* player)
        {
            player->BaseSpeed = 5;
            player->CurrentSpeed = player->BaseSpeed;
            player->DashForce = 10000;
            player->tapWindow = 15; // Number of frames within which a double-tap is detected
            player->wTapCounter = 0;
            player->dTapCounter = 0;
            player->sTapCounter = 0;
            player->aTapCounter = 0;

            player->isDashing = false;
            player->dashFrameDuration = 10; // Number of frames the dash lasts
            player->dashFrameTimer = 0;

            player->lastWPressed = false; // Flags to track key press state
            player->lastDPressed = false;
            player->lastAPressed = false;
            player->lastSPressed = false;

            player->ClimbSpeed = 20;
            player->isClimbing = false;

            player->tempPosition = FPVector3.Zero;
        }

        public void OnCollisionPlayerHitClimbingSurface(Frame f, CollisionInfo3D info, Player* player)
        {
            player->isClimbing = true;
        }

        public void OnCollisionPlayerExitClimbingSurface(Frame f, ExitInfo3D info, Player* player)
        {
            player->isClimbing = false;
        }

        private void HandleClimbing(KCC* kcc, Frame frame, ref Filter filter, Player* player, Input* input)
        {
            EnvironmentProcessor.SetGravity(FPVector3.Zero);
            kcc->SetKinematicVelocity(FPVector3.Zero);

            if (input->MoveDirection.Y != 0)
            {
                filter.Transform->Position +=
                    FPVector3.Up * input->MoveDirection.Y * frame.DeltaTime * player->ClimbSpeed;
                player->tempPosition = filter.Transform->Position;
            }
            else if (input->MoveDirection.X != 0)
            {
                filter.Transform->Position.Y = player->tempPosition.Y;
                filter.Transform->Position +=
                    FPVector3.Forward * input->MoveDirection.X * frame.DeltaTime * player->ClimbSpeed;
                player->tempPosition = filter.Transform->Position;
            }
            else
            {
                filter.Transform->Position = player->tempPosition;
            }
        }

        private void PerformDash(KCC* kcc, FPVector3 desiredDirection, FP dashForce,Player* player)
        {
            var dashDirection = kcc->Data.TransformRotation * desiredDirection;
            kcc->AddExternalForce(dashDirection * dashForce);
        }

        private void HandleNormalMovement(KCC* kcc, Input* input, ref Filter filter)
        {
            FPVector3 movementDirection = kcc->Data.TransformRotation * input->MoveDirection.XOY;
            kcc->SetInputDirection(movementDirection * filter.Player->CurrentSpeed);
        }

        private void AdjustSpeed(Player* player)
        {
             FP speedReductionPerProjectile = FP._0_33;

            player->CurrentSpeed = player->BaseSpeed - (speedReductionPerProjectile * player->AttachedProjectilesCount);

            player->CurrentSpeed = FPMath.Max(player->CurrentSpeed, FP._0_25); // Minimum speed
        }
    }
}