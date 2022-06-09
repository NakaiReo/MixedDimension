using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UnityEngine.InputSystem;

public class TitleDirector : MonoBehaviour
{
	public static TitleDirector ins = null;

	KeyBind input;

	[SerializeField] VirtualMouse virtualMouse;
	[SerializeField] StageDataFile stageData;
	[SerializeField] GameObject subMap;
	[SerializeField] Transform stageViewer;
	[Space]
 	[SerializeField] CanvasGroup canvasGroup;
	[SerializeField] GameObject fadeStart;
	[SerializeField] ParticleSystem kamihubuki;

	[Space]
	[SerializeField] GameObject playerObject;
	[SerializeField] ToggleButton closeOpenStageSelect;
	[SerializeField] Button continueMapButton;
	[SerializeField] Button startGameButton;
	public TextMeshProUGUI selectStageText;
	public TextMeshProUGUI clearPercentText;

	[Space]
	//UI切り替え時のオブジェクトの纏まり
	[SerializeField] GameObject titleImage;
	[SerializeField] GameObject mainObject;
	[SerializeField] GameObject startGameObject;
	[SerializeField] GameObject stageSelectObject;
	[SerializeField] GameObject manualObject;     

	public Transform subMapFile { get { return stageViewer; } }

	//UI切り替える時のグループ
	public enum TitlePanels
	{
		MainMenu,
		StartGame,
		StageSelect,
		Manual,
	}
	TitlePanels currentPanel = TitlePanels.MainMenu;

	private void Awake()
	{
		input = new KeyBind();

		if (ins == null)
		{
			ins = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
    {
		Time.timeScale = 1.0f;
		canvasGroup.blocksRaycasts = true;

		virtualMouse.EnableVirtualMouse();
		SoundDirector.ins.BGM.PlayLoop("Title");

		mainObject.SetActive(true);
		startGameObject.SetActive(true);
		stageSelectObject.SetActive(true);
		manualObject.SetActive(true);

		ChangePanel(TitlePanels.MainMenu, 0);

		//ステージセレクトボタンの初期化
		if (StageSelectButton.currentButton != null) StageSelectButton.currentButton.Close(0);
		StageSelectButton.currentButton = null;
		if (StageFolderSelectButton.currentButton != null) StageFolderSelectButton.currentButton.Close(0);
		StageFolderSelectButton.currentButton = null;

		SelectStageText.ins.ChangeText();

		//続きから出来るかどうか
		if (!PlayerPrefs.HasKey("StageIndex")) PlayerPrefs.SetInt("StageIndex", -1);
		continueMapButton.interactable = PlayerPrefs.GetInt("StageIndex") >= 0;

		fadeStart.SetActive(false);

		//ステージプレビューの作成
		StageSlectAreaCreate();

		//達成率の表示
		clearPercentText.text = "達成率 : " + (ClearPercent() * 100).ToString("F1") + "%";
		clearPercentText.color = new Color(1, 1, 1.0f - ClearPercent(), 1.0f);

		//完全クリアの時、紙吹雪を降らせる
		if (ClearPercent() >= 1.0f) kamihubuki.Play();

		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
		if (input.UI.Setting.triggered)
		{
			PushSettigButton();
		}

		bool b = StageSelectButton.currentButton != null;
		startGameButton.interactable = b;
	}

	/// <summary>
	/// ステージプレビューの生成
	/// </summary>
	void StageSlectAreaCreate()
	{
		for(int i = 0; i < stageData.data.Count; i++)
		{
			StageDataObject data = stageData.data[i];
			Transform file = Instantiate(subMap).transform;
			file.SetParent(stageViewer, false);
			file.transform.localPosition = new Vector3(data.stageNumber * -50, 0, (int)data.difficult * 50);
			file.GetComponent<SubMap>().stageData = data;

			GameObject ins = Instantiate(data.mapData);
			ins.transform.SetParent(file.GetChild(0), false);

			Transform mes = ins.transform.Find("Message");
			if (mes != null) mes.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// クリア率の計算
	/// </summary>
	/// <returns>クリア率</returns>
	public float ClearPercent()
	{
		int count = 0;
		for(int i = 0; i < ClearData.ins.data.isClear.Length; i++)
		{
			if (ClearData.ins.data.isClear[i]) count++;
		}

		float p = (float)count / ClearData.ins.data.isClear.Length;
		return p;
	}

	/// <summary>
	/// UIを切り替える関数
	/// </summary>
	/// <param name="panel">切り替えるUIのグループ</param>
	/// <param name="time">切り替える速度</param>
	public void ChangePanel(TitlePanels panel, float time = 0.5f)
	{
		closeOpenStageSelect.Flip(true);

		titleImage.transform.DOScaleX(panel != TitlePanels.StageSelect ? 1 : 0, time);
		DoScale(mainObject,        panel == TitlePanels.MainMenu,    time);
		DoScale(startGameObject,   panel == TitlePanels.StartGame,   time);
		DoScale(stageSelectObject, panel == TitlePanels.StageSelect, time);
		DoScale(manualObject     , panel == TitlePanels.Manual     , time);

		currentPanel = panel;
	}

	/// <summary>
	/// UIの大きさを変える関数
	/// </summary>
	/// <param name="objects">大きさを変えるUIの纏まり</param>
	/// <param name="isBool">1にするか0にするか</param>
	/// <param name="time">UIを切り替える速度</param>
	void DoScale(GameObject objects, bool isBool, float time)
	{
		for(int i = 0; i < objects.transform.childCount; i++)
		{
			objects.transform.GetChild(i).DOScaleX(isBool ? 1 : 0, time);
		}
	}

	/// <summary>
	/// Manualを押したとき
	/// </summary>
	public void PushManualButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		ChangePanel(TitlePanels.Manual);
	}

	/// <summary>
	/// Settingを押したとき
	/// </summary>
	public void PushSettigButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		SettingMenu.ins.ChangeActive(true);
	}

	/// <summary>
	/// StartGameを押したとき
	/// </summary>
	public void PushStartGameButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		ChangePanel(TitlePanels.StartGame);
	}

	/// <summary>
	/// StageSelectを押したとき
	/// </summary>
	public void PushStageSelectButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		ChangePanel(TitlePanels.StageSelect);
		SelectStageText.ins.ChangeText();
	}

	/// <summary>
	/// NewGameを押したとき
	/// </summary>
	public void PushNewGameButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		PlayerPrefs.SetInt("StageIndex", 0);
		LoadGameScene();
	}

	/// <summary>
	/// Continueを押したとき
	/// </summary>
	public void PushContinueButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		LoadGameScene();
	}

	/// <summary>
	/// StageSelectを押したとき
	/// </summary>
	public void PushStartMapButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		PlayerPrefs.SetInt("StageIndex", StageSelectButton.currentButton.stageIndex);
		LoadGameScene();
	}

	/// <summary>
	/// ShutDownを押したとき
	/// </summary>
	public void PushShutdownButton()
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
	    Application.Quit();
		#endif
	}

	/// <summary>
	/// Backを押したとき
	/// </summary>
	public void BackButton()
	{
		SoundDirector.ins.SE.Play("UISelect");
		ChangePanel(TitlePanels.MainMenu);

		if(StageSelectButton.currentButton != null)
			StageSelectButton.currentButton.Close(0);
		StageSelectButton.currentButton = null;

		if(StageFolderSelectButton.currentButton != null)
			StageFolderSelectButton.currentButton.Close(0);
		StageFolderSelectButton.currentButton = null;
		SelectStageText.ins.ChangeText();
	}

	/// <summary>
	/// ステージセレクトのタブを閉じる
	/// </summary>
	/// <param name="isOn"></param>
	/// <param name="time"></param>
	public void PushCloseOpenStageSelect(bool isOn, float time = 0.5f)
	{
		stageSelectObject.transform.GetChild(0).DOLocalMoveX(isOn ? -1000 : -1800, time);
	}

	/// <summary>
	/// ゲームシーンを読み込む
	/// </summary>
	void LoadGameScene()
	{
		canvasGroup.blocksRaycasts = false;
		Fade.ins.FadeIn("Game");
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();
}