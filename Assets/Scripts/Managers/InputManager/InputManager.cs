using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : PersistentMonoSingleton<InputManager>
{
	// Action Maps
	private const string PlayerActionMap = "Player";

	// References
	[Tooltip("The Input Action Asset containing all player and UI actions.")]
	public InputActionAsset InputActions;

	// Events
	public event Action<Vector2> OnMovement;
	public event Action OnConfirm;
	public event Action OnCancel;
	public event Action OnStart;
	public event Action OnSelect;

	private InputAction _movementAction;
	private List<ActionBinding> _bindings = new List<ActionBinding>();

	/** Start Methods **/
	protected override void OnInitialized()
	{
		base.OnInitialized();

		SetupInputActions();
	}

	private void OnEnable()
	{
		EnablePlayerInput();
		SubscribeEvents();
	}

	private void OnDisable()
	{
		if (IsActiveInstance)
		{
			DisablePlayerInput();
		}
		UnsubscribeEvents();
	}

	private void SetupInputActions()
	{
		_bindings.Clear();
		_movementAction = InputActions.FindAction("Movement");

		BindAction("Confirm", performed: () => OnConfirm?.Invoke());
		BindAction("Cancel", performed: () => OnCancel?.Invoke());
		BindAction("Start", performed: () => OnStart?.Invoke());
		BindAction("Select", performed: () => OnSelect?.Invoke());
	}

	private void SubscribeEvents()
	{
		foreach (ActionBinding binding in _bindings)
		{
			binding.Subscribe();
		}
	}

	private void UnsubscribeEvents()
	{
		foreach (ActionBinding binding in _bindings)
		{
			binding.Unsubscribe();
		}
	}

	/** Update Methods **/
	private void Update()
	{
		UpdateContinuousInputs();
	}

	private void UpdateContinuousInputs()
	{
		Vector3 readVector = _movementAction.ReadValue<Vector3>();
		OnMovement?.Invoke(new Vector2(readVector.x, readVector.z));
	}

	/// <summary>
	/// Enable Player Input
	/// </summary>
	public void EnablePlayerInput()
	{
		InputActions.FindActionMap(PlayerActionMap).Enable();
	}

	/// <summary>
	/// Disable Player Input
	/// </summary>
	public void DisablePlayerInput()
	{
		InputActions.FindActionMap(PlayerActionMap).Disable();
	}

	/** Action Binding System **/
	private class ActionBinding
	{
		private readonly Action _performedEvent;
		private readonly Action _canceledEvent;
		private readonly InputAction _action;

		public ActionBinding(InputAction action, Action performed, Action canceled)
		{
			_action = action;
			_performedEvent = performed;
			_canceledEvent = canceled;
		}

		public void Subscribe()
		{
			_action.performed += OnPerformed;
			_action.canceled += OnCanceled;
		}

		public void Unsubscribe()
		{
			_action.performed -= OnPerformed;
			_action.canceled -= OnCanceled;
		}

		private void OnPerformed(InputAction.CallbackContext context) => _performedEvent?.Invoke();

		private void OnCanceled(InputAction.CallbackContext context) => _canceledEvent?.Invoke();
	}

	private void BindAction(string actionName, Action performed = null, Action canceled = null)
	{
		InputAction action = InputActions.FindAction(actionName);
		_bindings.Add(new ActionBinding(action, performed, canceled));
	}
}
