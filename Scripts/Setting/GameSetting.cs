using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゲーム中の設定画面
/// </summary>
public class GameSetting : MonoBehaviour
{
	KeyBind input;
	GameObject settingPanel;

	[SerializeField] VirtualMouse virtualMouse;

	bool isOpen = false;
	bool isFade = false;

	private void Awake()
	{
		input = new KeyBind();
	}

	void Start()
    {
		isOpen = false;
		isFade = false;

		settingPanel = transform.GetChild(0).gameObject;
		ChangeActive(false);
    }

	private void Update()
	{
		if (Fade.ins.isFade == true) return;

		if (input.UI.Setting.triggered)
		{
			if (!SettingMenu.ins.isOpen)
			{
				ChangeActive(!isOpen);
			}
		}
	}

	/// <summary>
	/// パネルの変更
	/// </summary>
	/// <param name="enable">ON/OFF</param>
	private void ChangeActive(bool enable)
	{
		if (Fade.ins.isFade == true) return;

		isOpen = enable;
		settingPanel.SetActive(isOpen);
		Time.timeScale = isOpen ? 0 : 1;

		if (enable) virtualMouse.EnableVirtualMouse();
		else virtualMouse.DisableVirtualMouse();
	}
	
	/// <summary>
	/// ゲームに戻る
	/// </summary>
	public void PushReturnTheGame()
	{
		if (Fade.ins.isFade == true) return;
		Time.timeScale = 1.0f;
		ChangeActive(false);
	}

	/// <summary>
	/// 設定を開く
	/// </summary>
	public void PushSetting()
	{
		if (Fade.ins.isFade == true) return;
		ChangeActive(false);
		SettingMenu.ins.ChangeActive(true);
	}

	/// <summary>
	/// ゲームをリスタートする
	/// </summary>
	public void PushRestart()
	{
		if (Fade.ins.isFade == true) return;
		Time.timeScale = 1.0f;
		Fade.ins.FadeIn("Game");
	}

	/// <summary>
	/// タイトルに戻る
	/// </summary>
	public void PushReturnTheTitle()
	{
		if (Fade.ins.isFade == true) return;
		Time.timeScale = 1.0f;
		Fade.ins.FadeIn("Title");
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();
}
