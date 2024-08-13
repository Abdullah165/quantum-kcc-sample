namespace Quantum
{
	using UnityEngine;
	using UnityEngine.InputSystem;
	using Photon.Deterministic;

	/// <summary>
	/// Handles player input.
	/// </summary>
	[DefaultExecutionOrder(-10)]
	public sealed class PlayerInput : MonoBehaviour
	{
		public static float LookSensitivity = 4.0f;

		/// <summary>
		/// This is a special accumulator which accepts mouse/touch delta and returns smoothed,
		/// frame-aligned look rotation delta. It is essential to get super snappy butter-smooth experience.
		/// </summary>
		public Vector2Accumulator LookRotationAccumulator => _lookRotationAccumulator;

		private Quantum.Input      _accumulatedInput;
		private Vector2Accumulator _lookRotationAccumulator = new Vector2Accumulator(0.02f, true);
		private bool               _resetAccumulatedInput;
		private int                _lastAccumulateFrame;
		private InputTouches       _inputTouches = new InputTouches();
		private InputTouch         _moveTouch;
		private InputTouch         _lookTouch;
		private bool               _jumpTouch;
		private float              _jumpTime;

		private void OnEnable()
		{
			QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));

			_inputTouches.TouchStarted  = OnTouchStarted;
			_inputTouches.TouchFinished = OnTouchFinished;
		}

		private void OnDisable()
		{
			_inputTouches.TouchStarted  = null;
			_inputTouches.TouchFinished = null;
		}

		private void Update()
		{
			AccumulateInput();
		}

		private void AccumulateInput()
		{
			if (_lastAccumulateFrame == Time.frameCount)
				return;

			_lastAccumulateFrame = Time.frameCount;

			if (_resetAccumulatedInput == true)
			{
				_resetAccumulatedInput = false;
				_accumulatedInput = default;
			}

			if (Application.isMobilePlatform == true && Application.isEditor == false)
			{
				_inputTouches.Update();

				ProcessMobileInput();
			}
			else
			{
				ProcessStandaloneInput();
			}
		}

		private void ProcessStandaloneInput()
		{
			// Enter key is used for locking/unlocking cursor in game view.
			Keyboard keyboard = Keyboard.current;
			if (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
			{
				if (Cursor.lockState == CursorLockMode.Locked)
				{
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
				else
				{
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}

			// Accumulate input only if the cursor is locked.
			if (Cursor.lockState != CursorLockMode.Locked)
				return;

			Mouse mouse = Mouse.current;
			if (mouse != null)
			{
				Vector2 mouseDelta = mouse.delta.ReadValue();

				Vector2 lookRotationDelta = new Vector2(-mouseDelta.y, mouseDelta.x);
				lookRotationDelta *= LookSensitivity / 60f;
				_lookRotationAccumulator.Accumulate(lookRotationDelta);
			}

			if (keyboard != null)
			{
				Vector2 moveDirection = Vector2.zero;

				if (keyboard.wKey.isPressed) { moveDirection += Vector2.up;    }
				if (keyboard.sKey.isPressed) { moveDirection += Vector2.down;  }
				if (keyboard.aKey.isPressed) { moveDirection += Vector2.left;  }
				if (keyboard.dKey.isPressed) { moveDirection += Vector2.right; }

				_accumulatedInput.MoveDirection = moveDirection.normalized.ToFPVector2();

				_accumulatedInput.Jump |= keyboard.spaceKey.isPressed;
			}
		}

		private void ProcessMobileInput()
		{
			Vector2 moveDirection     = Vector2.zero;
			Vector2 lookRotationDelta = Vector2.zero;

			if (_lookTouch != null && _lookTouch.IsActive == true)
			{
				lookRotationDelta = new Vector2(-_lookTouch.Delta.Position.y, _lookTouch.Delta.Position.x);
				lookRotationDelta *= LookSensitivity / 15f;
			}

			_lookRotationAccumulator.Accumulate(lookRotationDelta);

			if (_moveTouch != null && _moveTouch.IsActive == true && _moveTouch.GetDelta().Position.Equals(default) == false)
			{
				float screenSizeFactor = 8.0f / Mathf.Min(Screen.width, Screen.height);

				moveDirection = new Vector2(_moveTouch.GetDelta().Position.x, _moveTouch.GetDelta().Position.y) * screenSizeFactor;
				if (moveDirection.sqrMagnitude > 1.0f)
				{
					moveDirection.Normalize();
				}
			}

			_accumulatedInput.Jump         |= _jumpTouch;
			_accumulatedInput.MoveDirection = moveDirection.ToFPVector2();
		}

		private void OnTouchStarted(InputTouch touch)
		{
			if (_moveTouch == null && touch.Start.Position.x < Screen.width * 0.5f)
			{
				_moveTouch = touch;
			}

			if (_lookTouch == null && touch.Start.Position.x > Screen.width * 0.5f)
			{
				_lookTouch = touch;
				_jumpTouch = default;

				if (_jumpTime > Time.realtimeSinceStartup - 0.25f)
				{
					_jumpTouch = true;
				}

				_jumpTime = Time.realtimeSinceStartup;
			}
		}

		private void OnTouchFinished(InputTouch touch)
		{
			if (_moveTouch == touch) { _moveTouch = default; }
			if (_lookTouch == touch) { _lookTouch = default; _jumpTouch = default; }
		}

		public void PollInput(CallbackPollInput callback)
		{
			AccumulateInput();

			_resetAccumulatedInput = true;

			Vector2   consumeLookRotation = _lookRotationAccumulator.ConsumeFrameAligned(callback.Game);
			FPVector2 pollLookRotation    = consumeLookRotation.ToFPVector2();

			_lookRotationAccumulator.Add(consumeLookRotation - pollLookRotation.ToUnityVector2());

			_accumulatedInput.LookRotationDelta = pollLookRotation;

			callback.SetInput(_accumulatedInput, DeterministicInputFlags.Repeatable);
		}
	}
}
