using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;


public class VirtualMouse : MonoBehaviour
{
	[SerializeField] private PlayerInput playerInput;
	[SerializeField] private RectTransform cursorTransform;
	[SerializeField] private Canvas canvas;
	[SerializeField] private RectTransform canvasRectTransform;
	[SerializeField] private float cursorSpeed = 1000f;
	[SerializeField] private float scroolSpeed = 20.0f;
	[SerializeField] private float padding = 35f;

	private bool previousMouseState;
	private Mouse virtualMouse;
	private Mouse currentMouse;
	private Camera mainCamera;

	private string previousControlScheme = "";
	private const string gamepadScheme = "Gamepad";
	private const string mouseScheme = "Keyboard&Mouse";

	public bool isActive { get; private set; }
	[SerializeField] string check;

	public void EnableVirtualMouse()
	{
		isActive = true;
		previousControlScheme = "";
		OnControlsChanged();
	}

	public void DisableVirtualMouse()
	{
		isActive = false;
		HideCursor();
	}

	private void Awake()
	{
		isActive = true;
	}

	private void OnEnable()
	{
		mainCamera = Camera.main;
		currentMouse = Mouse.current;

		if(virtualMouse == null)
		{
			virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
		}
		else if (!virtualMouse.added)
		{
			InputSystem.AddDevice(virtualMouse);
		}

		InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

		if(cursorTransform != null)
		{
			Vector2 postion = cursorTransform.anchoredPosition;
			InputState.Change(virtualMouse.position, postion);
		}

		InputSystem.onAfterUpdate += UpdateMotion;
		//playerInput.onControlsChanged += OnControlsChanged;
	}

	private void OnDisable()
	{
		if (virtualMouse != null && virtualMouse.added)
		{
			playerInput.user.UnpairDevice(virtualMouse);
			InputSystem.RemoveDevice(virtualMouse);
		}

		InputSystem.onAfterUpdate -= UpdateMotion;
		//playerInput.onControlsChanged -= OnControlsChanged;
	}

	private void UpdateMotion()
	{
		if (isActive == false) return;
		if (virtualMouse == null || Gamepad.current == null) return;

		Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
		deltaValue *= cursorSpeed * Time.unscaledDeltaTime;

		Vector2 currentPostion = virtualMouse.position.ReadValue();
		Vector2 newPostion = currentPostion + deltaValue;

		float scroolDelta = Gamepad.current.rightStick.ReadValue().y * scroolSpeed * -1;

		newPostion.x = Mathf.Clamp(newPostion.x, padding, Screen.width  - padding);
		newPostion.y = Mathf.Clamp(newPostion.y, padding, Screen.height - padding);

		InputState.Change(virtualMouse.position, newPostion);
		InputState.Change(virtualMouse.delta, deltaValue);
		InputState.Change(virtualMouse.scroll, scroolDelta);

		bool aButtonIsPressed = Gamepad.current.aButton.isPressed;
		if(previousMouseState != aButtonIsPressed)
		{
			virtualMouse.CopyState<MouseState>(out var mouseState);
			mouseState.WithButton(MouseButton.Left, aButtonIsPressed);
			InputState.Change(virtualMouse, mouseState);
			previousMouseState = aButtonIsPressed;
		}

		AnchorCursor(newPostion);
	}

	private void AnchorCursor(Vector2 postion)
	{
		Vector2 anchoredPostion;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, postion, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out anchoredPostion);
		cursorTransform.anchoredPosition = anchoredPostion;
	}

	private void OnControlsChanged()
	{
		cursorSpeed = 500.0f * PlayerPrefs.GetFloat("VMS");

		if (playerInput.currentControlScheme == mouseScheme && previousControlScheme != mouseScheme)
		{
			HideCursor();
		}
		else if(playerInput.currentControlScheme == gamepadScheme && previousControlScheme != gamepadScheme)
		{
			ViewCursor();
		}
	}

	public void Update()
	{
		check = previousControlScheme;

		if(previousControlScheme != playerInput.currentControlScheme && isActive)
		{
			OnControlsChanged();
		}
		previousControlScheme = playerInput.currentControlScheme;

		if (previousControlScheme == "") return;
		Cursor.visible = previousControlScheme == mouseScheme;
	}

	private void HideCursor()
	{
		cursorTransform.gameObject.SetActive(false);
		Cursor.visible = true;
		currentMouse.WarpCursorPosition(virtualMouse.position.ReadValue());
		previousControlScheme = mouseScheme;
	}

	private void ViewCursor()
	{
		cursorTransform.gameObject.SetActive(true);
		Cursor.visible = false;
		InputState.Change(virtualMouse.position, currentMouse.position.ReadValue());
		AnchorCursor(currentMouse.position.ReadValue());
		previousControlScheme = gamepadScheme;
	}
}
