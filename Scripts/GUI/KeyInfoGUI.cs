using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.DualShock;
using TMPro;
using DG.Tweening;

/// <summary>
/// ゲーム中の左上のキー情報UIを扱うクラス
/// </summary>
public class KeyInfoGUI : MonoBehaviour
{
	public static KeyInfoGUI ins = null; //シングルトン

	[System.Serializable]
	public class TypeOfKeyImage
	{
		public string scheme;
		public Sprite movement;
		public Sprite camera;
		public Sprite jump;
		public Sprite tellport;
		public Sprite cameraChange;
		public Sprite dimensionChange;
	}

	private void Awake()
	{
		if (ins == null)
		{
			ins = this;
		}
		else
		{
			//Destroy(gameObject);
		}
	}

	private void Start()
	{
		SettingMenu.ins.GamePlaySetup(GameDirector.ins);

		OnGUI();
	}

	public GameObject keyInfoGUIObject;

	TypeOfKeyImage backContorlKeyImage = null;
	public TypeOfKeyImage currentContorlKeyImage
	{
		get
		{
			if(Gamepad.current != null)
			{
				if(Gamepad.current is XInputController)
				{
					return xboxKeyImage;
				}
				if(Gamepad.current is DualShockGamepad)
				{
					return ps4KeyImage;
				}
				else
				{
					return pcKeyImage;
				}
			}
			else
			{
				return pcKeyImage;
			}
		}
	}

	[Space]
	public Image topDownImage;
	public Image sideViewImage;
	public Image playerViewImage;
	public Image secondDimensionViewImage;
	public Image toTopDownArrowImage;
	public Image toSideViewArrowImage;
	public Image toPlayerViewArrowImage;
	public Image toSecondDimensionArrowImage;
	public Image toThirdDimensionArrowImage;
	public Image cameraChangeKeyImage;
	public Image dimensionChangeKeyImage;

	[Space]
	public GameObject portalPanel;		//テレポートのパネル
	public TextMeshProUGUI portalText;  //テレポートの情報テキスト
	public Slider portalSlider;         //テレポートの時間表示用

	[Space]
	[SerializeField] TypeOfKeyImage pcKeyImage;
	[SerializeField] TypeOfKeyImage ps4KeyImage;
	[SerializeField] TypeOfKeyImage xboxKeyImage;

	public void SetActive(bool enable)
	{
		keyInfoGUIObject.SetActive(enable);
	}

	private void OnGUI()
	{
		if (backContorlKeyImage == currentContorlKeyImage) return;

		backContorlKeyImage = currentContorlKeyImage;

		cameraChangeKeyImage.sprite = currentContorlKeyImage.cameraChange;
		dimensionChangeKeyImage.sprite = currentContorlKeyImage.dimensionChange;
	}

	/// <summary>
	/// UIを更新
	/// </summary>
	/// <param name="cameraType">現在のカメラタイプ</param>
	public void SetGUI(CameraController.CameraType cameraType)
	{
		float animTime = 0.5f;
		float delayTime = 0.25f;

		float hideAlpha = 0.1f;
		float nextAlpha = 0.8f;
		float currentAlpha = 1.0f;

		topDownImage.DOFade(hideAlpha, animTime);
		sideViewImage.DOFade(hideAlpha,  animTime);
		playerViewImage.DOFade(hideAlpha,  animTime);
		secondDimensionViewImage.DOFade(hideAlpha,  animTime);
		toTopDownArrowImage.DOFade(hideAlpha,  animTime);
		toSideViewArrowImage.DOFade(hideAlpha,  animTime);
		toPlayerViewArrowImage.DOFade(hideAlpha,  animTime);
		toSecondDimensionArrowImage.DOFade(hideAlpha,  animTime);
		toThirdDimensionArrowImage.DOFade(hideAlpha,  animTime);
		cameraChangeKeyImage.DOFade(hideAlpha,  animTime);
		dimensionChangeKeyImage.DOFade(hideAlpha,  animTime);

		switch (cameraType)
		{
			case CameraController.CameraType.Player:
				playerViewImage.DOFade(currentAlpha, animTime);
				toTopDownArrowImage.DOFade(nextAlpha, animTime).SetDelay(delayTime);

				cameraChangeKeyImage.DOFade(currentAlpha, animTime);
				break;

			case CameraController.CameraType.TopDown:
				topDownImage.DOFade(currentAlpha, animTime);
				toSideViewArrowImage.DOFade(nextAlpha, animTime).SetDelay(delayTime);

				cameraChangeKeyImage.DOFade(currentAlpha, animTime);
				break;

			case CameraController.CameraType.SideView:
				sideViewImage.DOFade(currentAlpha, animTime);
				toPlayerViewArrowImage.DOFade(nextAlpha, animTime).SetDelay(delayTime); ;
				toSecondDimensionArrowImage.DOFade(nextAlpha, animTime).SetDelay(delayTime);

				cameraChangeKeyImage.DOFade(currentAlpha, animTime);
				dimensionChangeKeyImage.DOFade(currentAlpha, animTime);
				break;

			case CameraController.CameraType.SecondDimention:
				secondDimensionViewImage.DOFade(currentAlpha, animTime);
				toThirdDimensionArrowImage.DOFade(nextAlpha, animTime).SetDelay(delayTime);

				cameraChangeKeyImage.DOFade(hideAlpha, animTime);
				dimensionChangeKeyImage.DOFade(currentAlpha, animTime);
				break;

			default:
				break;
		}
	}

	public void Teleport() { /*ChangeGUI(teleportPortal, GameDirector.ins.checkPoint.activeSelf);*/ }

	/// <summary>
	/// 現在使用できないキーの場合半透明にする
	/// </summary>
	/// <param name="cg">色を変えるグループ</param>
	/// <param name="enable">使用可能か?</param>
	void ChangeGUI(CanvasGroup cg, bool enable)
	{
		cg.alpha = enable ? 1 : 0.5f;
	}
}
