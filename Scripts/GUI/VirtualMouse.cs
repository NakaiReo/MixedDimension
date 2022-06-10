using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;


public class VirtualMouse : MonoBehaviour
{
	[SerializeField] private PlayerInput playerInput;       //PlayerInputのコンポーネント
	[SerializeField] private RectTransform cursorTransform; //カーソルの位置 
	[SerializeField] private Canvas canvas;                 //カーソル用のキャンバス
	[SerializeField] private RectTransform canvasRectTransform; //キャンバスの位置
	[SerializeField] private float cursorSpeed = 1000f; //カーソルの移動速度
	[SerializeField] private float scroolSpeed = 20.0f; //スクロール速度
	[SerializeField] private float padding = 35f;       //画面外からのデッドゾーン

	private bool previousMouseState;
	private Mouse virtualMouse;
	private Mouse currentMouse;
	private Camera mainCamera;

	private string previousControlScheme = "";
	private const string gamepadScheme = "Gamepad";
	private const string mouseScheme = "Keyboard&Mouse";

	public bool isActive { get; private set; }
	[SerializeField] string check;

	/// <summary>
    /// 仮想マウスを有効化
    /// </summary>
	public void EnableVirtualMouse()
	{
		isActive = true;
		previousControlScheme = "";
		OnControlsChanged();
	}

	/// <summary>
    /// 仮想マウスを無効化
    /// </summary>
	public void DisableVirtualMouse()
	{
		isActive = false;
		HideCursor();
	}

	private void Awake()
	{
		isActive = true;
	}

	/// <summary>
    /// コンポーネントが有効時の処理
    /// </summary>
	private void OnEnable()
	{
		mainCamera = Camera.main;
		currentMouse = Mouse.current;

		//仮想マウスのクラスが作られてなければ新しく作る
		if(virtualMouse == null)
		{
			virtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
		}
		//存在はするが、追加されてない場合追加する
		else if (!virtualMouse.added)
		{
			InputSystem.AddDevice(virtualMouse);
		}

		//バーチャルマウスを操作方法の一環として登録する
		InputUser.PerformPairingWithDevice(virtualMouse, playerInput.user);

		//カーソルを仮想マウスの位置に
		if(cursorTransform != null)
		{
			Vector2 postion = cursorTransform.anchoredPosition;
			InputState.Change(virtualMouse.position, postion);
		}

		//コールバックを登録
		InputSystem.onAfterUpdate += UpdateMotion;
		//playerInput.onControlsChanged += OnControlsChanged;
	}

	/// <summary>
    /// コンポーネントが無効かされた時
    /// </summary>
	private void OnDisable()
	{
		//仮想マウスをPlayerInputから削除
		if (virtualMouse != null && virtualMouse.added)
		{
			playerInput.user.UnpairDevice(virtualMouse);
			InputSystem.RemoveDevice(virtualMouse);
		}

		//コールバックを削除
		InputSystem.onAfterUpdate -= UpdateMotion;
		//playerInput.onControlsChanged -= OnControlsChanged;
	}

	/// <summary>
    /// 動きがあったときに呼ばれるコールバック
    /// </summary>
	private void UpdateMotion()
	{
		//バーチャルマウスが無効化され、ゲームパッドも存在しないとき、実行しない
		if (isActive == false) return;
		if (virtualMouse == null || Gamepad.current == null) return;

		//左スティックの分だけカーソルを移動させる
		Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
		deltaValue *= cursorSpeed * Time.unscaledDeltaTime;

		Vector2 currentPostion = virtualMouse.position.ReadValue();
		Vector2 newPostion = currentPostion + deltaValue;

		//右スティックでマウススクロールを扱う
		float scroolDelta = Gamepad.current.rightStick.ReadValue().y * scroolSpeed * -1;

		//デッドゾーンを超えないように調整
		newPostion.x = Mathf.Clamp(newPostion.x, padding, Screen.width  - padding);
		newPostion.y = Mathf.Clamp(newPostion.y, padding, Screen.height - padding);

		//新しい値を代入
		InputState.Change(virtualMouse.position, newPostion);
		InputState.Change(virtualMouse.delta, deltaValue);
		InputState.Change(virtualMouse.scroll, scroolDelta);

		//決定キーが押された時の処理
		bool aButtonIsPressed = Gamepad.current.aButton.isPressed;
		if(previousMouseState != aButtonIsPressed)
		{
			virtualMouse.CopyState<MouseState>(out var mouseState);
			mouseState.WithButton(MouseButton.Left, aButtonIsPressed);
			InputState.Change(virtualMouse, mouseState);
			previousMouseState = aButtonIsPressed;
		}

		//カーソル位置更新
		AnchorCursor(newPostion);
	}

	/// <summary>
    /// カーソルの位置を更新
    /// </summary>
    /// <param name="postion">位置</param>
	private void AnchorCursor(Vector2 postion)
	{
		Vector2 anchoredPostion;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, postion, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out anchoredPostion);
		cursorTransform.anchoredPosition = anchoredPostion;
	}

	/// <summary>
    /// 入力方法が変わったときに呼ばれる
    /// </summary>
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

		//現在の操作方法と新しい操作方法が違う場合デバイスを更新する
		if(previousControlScheme != playerInput.currentControlScheme && isActive)
		{
			OnControlsChanged();
		}

		//現在の操作方法を新しい操作方法に
		previousControlScheme = playerInput.currentControlScheme;

		//現在の操作方法がマウスの場合カーソルを表示
		if (previousControlScheme == "") return;
		Cursor.visible = previousControlScheme == mouseScheme;
	}

	/// <summary>
    /// 仮想カーソルを隠す
    /// </summary>
	private void HideCursor()
	{
		cursorTransform.gameObject.SetActive(false);
		Cursor.visible = true;
		currentMouse.WarpCursorPosition(virtualMouse.position.ReadValue());
		previousControlScheme = mouseScheme;
	}

	/// <summary>
    /// 仮想カーソルを表示
    /// </summary>
	private void ViewCursor()
	{
		cursorTransform.gameObject.SetActive(true);
		Cursor.visible = false;
		InputState.Change(virtualMouse.position, currentMouse.position.ReadValue());
		AnchorCursor(currentMouse.position.ReadValue());
		previousControlScheme = gamepadScheme;
	}
}
