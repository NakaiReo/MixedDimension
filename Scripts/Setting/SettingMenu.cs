using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

/// <summary>
/// 設定のセーブデータ
/// </summary>
[Serializable]
public class SettingData
{
	public Screen screen;
	public System system;
	public GamePlay gamePlay;
	public Sound sound;

	[Serializable]
	public class Screen
	{
		/// <summary>
		/// 解像度
		/// </summary>
		public static Vector2Int[] Size = new Vector2Int[]
		{
			new Vector2Int(640,400),
			new Vector2Int(960,600),
			new Vector2Int(1280,800),
			new Vector2Int(1440,900),
			new Vector2Int(1680,1050),
			new Vector2Int(1920,1200),
		};

		public int screenType; //ウィンドウタイプ
		public int screenSizeIndex; //解像度Index
	}

	[Serializable]
	public class System
	{
		public bool isViewUI; //UIを表示させるか
		public bool isAutoDash; //オートダッシュ
		public bool isUseOutline; //アウトラインを表示させるか
	}

	[Serializable]
	public class GamePlay
	{
		public float masterSensitivity; //全体の感度
		public float topViewSensitivity; //トップダウンの感度
		public float sideViewSensitivity; //サイドビューの感度
		public float playerSensitivity; //プレイヤーTPSの感度
		public float virtualCursorSensitivity; //仮想カーソル感度
	}

	[Serializable]
	public class Sound
	{
		public float master; //全体の音量
		public float bgm; //BGMの音量
		public float se; //SEの音量
	}

	public SettingData()
	{
		screen = new Screen();
		system = new System();
		gamePlay = new GamePlay();
		sound = new Sound();
	}
}

/// <summary>
/// インスペクター上のオブジェクト
/// </summary>
[Serializable]
public class SettingInspector
{
	public ScreenInspector screen;
	public SystemInspector system;
	public GamePlayInspector gamePlay;
	public SoundInspector sound;

	GameDirector gameDirector;

	[Serializable]
	public class ScreenInspector
	{
		public TMP_Dropdown screenType;
		public TMP_Dropdown resolution;
		public Button applyButton;
	}

	[Serializable]
	public class SystemInspector
	{
		public Toggle viewUi;
		public Toggle autoDash;
		public Toggle useOutline;
	}

	[Serializable]
	public class GamePlayInspector
	{
		public TMP_InputField masterSensitivity;
		public TMP_InputField topViewSensitivity;
		public TMP_InputField sideViewSensitivity;
		public TMP_InputField playerSensitivity;
		public TMP_InputField virtualCursorSensitivity;
	}

	[Serializable]
	public class SoundInspector
	{
		public Slider master;
		public Slider bgm;
		public Slider se;
	}
}

/// <summary>
/// 設定を扱うクラス
/// </summary>
public class SettingMenu : MonoBehaviour
{
	public static SettingMenu ins = null;

	[SerializeField] GameObject panel;

	[Space]
	public SettingData data;		 //設定の情報
	public SettingInspector content; //設定の状態

	public bool isOpen = false; //設定メニューが開いているか

	private void Awake()
	{
		if (ins == null)
		{
			ins = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
    {
		data = new SettingData();

		//ファイルが生成されているか
		if(!Directory.Exists(Application.dataPath + "/Config"))
		{
			Directory.CreateDirectory(Application.dataPath + "/Config");
		}
        if(!CheckFile("Screen") || !CheckFile("System") || !CheckFile("GamePlay") || !CheckFile("Sound"))
		{
			SaveData();
		}

		//設定を読み込む
		LoadData();
		ChangeActive(false);
    }

	/// <summary>
	/// クリック時の処理
	/// </summary>
	public void Click()
	{
		ChangeActive(!isOpen);
	}

	/// <summary>
	/// メニューの切り替え
	/// </summary>
	/// <param name="enable">ON/OFF</param>
	public void ChangeActive(bool enable)
	{
		GameDirector gd = GameDirector.ins ? GameDirector.ins : null;

		isOpen = enable;
		panel.SetActive(isOpen);
		Time.timeScale = isOpen ? 0 : 1;
		if(gd)
		{
			if (enable) gd.virtualMouse.EnableVirtualMouse();
			else gd.virtualMouse.DisableVirtualMouse();
		}
	}

	/// <summary>
	/// 閉じるときの処理
	/// </summary>
	public void CloseButton()
	{
		SaveData();
		Setup();
		ChangeActive(false);
	}

	/// <summary>
	/// 設定を保存する
	/// </summary>
	public void SaveData()
	{
		data.screen.screenType = content.screen.screenType.value;
		data.screen.screenSizeIndex = content.screen.resolution.value;

		data.system.isViewUI = content.system.viewUi.isOn;
		data.system.isAutoDash = content.system.autoDash.isOn;
		data.system.isUseOutline = content.system.useOutline.isOn;

		if (PlayerPrefs.HasKey("VMS")) PlayerPrefs.SetFloat("VMS", 1.0f);

		if (!float.TryParse(content.gamePlay.masterSensitivity.text,   out data.gamePlay.masterSensitivity))
		{
			data.gamePlay.masterSensitivity = 10.0f;
		}
		if(!float.TryParse(content.gamePlay.topViewSensitivity.text,  out data.gamePlay.topViewSensitivity))
		{
			data.gamePlay.topViewSensitivity = 10.0f;
		}
		if(!float.TryParse(content.gamePlay.sideViewSensitivity.text, out data.gamePlay.sideViewSensitivity))
		{
			data.gamePlay.sideViewSensitivity = 10.0f;
		}
		if(!float.TryParse(content.gamePlay.playerSensitivity.text,   out data.gamePlay.playerSensitivity))
		{
			data.gamePlay.playerSensitivity = 10.0f;
		}
		if (!float.TryParse(content.gamePlay.virtualCursorSensitivity.text, out data.gamePlay.virtualCursorSensitivity))
		{
			data.gamePlay.playerSensitivity = 1.0f;
		}

		data.sound.master = content.sound.master.value;
		data.sound.bgm = content.sound.bgm.value;
		data.sound.se = content.sound.se.value;

		//設定をJsonで書き出す
		WriteJson("Screen", data.screen);
		WriteJson("System", data.system);
		WriteJson("GamePlay", data.gamePlay);
		WriteJson("Sound", data.sound);
	}

	/// <summary>
	/// 設定を読み込む
	/// </summary>
	public void LoadData()
	{
		//Jsonから設定を読み込む
		data.screen   = ReadJson<SettingData.Screen>("Screen");
		data.system   = ReadJson<SettingData.System>("System");
		data.gamePlay = ReadJson<SettingData.GamePlay>("GamePlay");
		data.sound    = ReadJson<SettingData.Sound>("Sound");

		content.screen.screenType.value = data.screen.screenType;
		content.screen.resolution.value = data.screen.screenSizeIndex;

		content.system.viewUi.isOn = data.system.isViewUI;
		content.system.autoDash.isOn = data.system.isAutoDash;
		content.system.useOutline.isOn = data.system.isUseOutline;

		content.gamePlay.masterSensitivity.text = data.gamePlay.masterSensitivity.ToString();
		content.gamePlay.topViewSensitivity.text = data.gamePlay.topViewSensitivity.ToString();
		content.gamePlay.sideViewSensitivity.text = data.gamePlay.sideViewSensitivity.ToString();
		content.gamePlay.playerSensitivity.text = data.gamePlay.playerSensitivity.ToString();
		content.gamePlay.virtualCursorSensitivity.text = data.gamePlay.virtualCursorSensitivity.ToString();

		content.sound.master.value = data.sound.master;
		content.sound.bgm.value = data.sound.bgm;
		content.sound.se.value = data.sound.se;

		//設定を反映させる
		Setup();
	}

	/// <summary>
	/// 設定の反映
	/// </summary>
	public void Setup()
	{
		GameDirector gd = GameDirector.ins ? GameDirector.ins : null;

		ChangeScreen();
		SystemSetup(gd);
		GamePlaySetup(gd);
		SoundSetup();
	}

	/// <summary>
	/// システムの設定を反映させる
	/// </summary>
	/// <param name="gd"></param>
	public void SystemSetup(GameDirector gd)
	{
		if (gd == null) return;

		gd.cameraChangePanel.SetActive(data.system.isViewUI);
		gd.dimensionChangePanel.SetActive(data.system.isViewUI);
		gd.outliner.enabled = data.system.isUseOutline;
	}

	/// <summary>
	/// ゲームの設定を反映させる
	/// </summary>
	/// <param name="gd"></param>
	public void GamePlaySetup(GameDirector gd)
	{
		if (gd == null) return;

		float publicSensitive = data.gamePlay.masterSensitivity;
		gd.cameraController.topDownCamera.sensitive = data.gamePlay.topViewSensitivity * publicSensitive;
		gd.cameraController.sideViewCamera.sensitive = data.gamePlay.sideViewSensitivity * publicSensitive;
		gd.cameraController.playerCamera.sensitive = data.gamePlay.playerSensitivity * publicSensitive * 0.5f;
		PlayerPrefs.SetFloat("VMS", data.gamePlay.virtualCursorSensitivity);
	}

	/// <summary>
	/// 音源の設定を反映させる
	/// </summary>
	public void SoundSetup()
	{
		PlayerPrefs.SetFloat("Master", data.sound.master);
		PlayerPrefs.SetFloat("BGM", data.sound.bgm);
		PlayerPrefs.SetFloat("SE", data.sound.se);
		SoundDirector.ins.Volume();
	}

	public void SoundTest(bool soundTest)
	{
		PlayerPrefs.SetFloat("Master", content.sound.master.value);
		PlayerPrefs.SetFloat("BGM", content.sound.bgm.value);
		PlayerPrefs.SetFloat("SE", content.sound.se.value);
		SoundDirector.ins.Volume();

		PlaySound();
	}

	/// <summary>
	/// Jsonの書き出し
	/// </summary>
	/// <typeparam name="ClassT">クラス</typeparam>
	/// <param name="name">保存名</param>
	/// <param name="data">クラス</param>
	void WriteJson<ClassT>(string name, ClassT data)
	{
		StreamWriter writer;

		string jsonstr = JsonUtility.ToJson(data);

		writer = new StreamWriter(Application.dataPath + "/Config/" + name + ".json", false);
		writer.Write(jsonstr);
		writer.Flush();
		writer.Close();
	}

	/// <summary>
	/// Jsonの読み込み
	/// </summary>
	/// <typeparam name="ClassT">クラス</typeparam>
	/// <param name="name">パスの名前</param>
	/// <returns>クラス</returns>
	ClassT ReadJson<ClassT>(string name)
	{
		string path = Application.dataPath + "/Config/" + name + ".json";

		if (File.Exists(path))
		{
			string datastr = "";
			StreamReader reader;
			reader = new StreamReader(path);
			datastr = reader.ReadToEnd();
			reader.Close();

			return JsonUtility.FromJson<ClassT>(datastr);
		}

		return JsonUtility.FromJson<ClassT>("");
	}


	/// <summary>
	/// ファイルが存在するかどうか
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	bool CheckFile(string name)
	{
		string path = Application.dataPath + "/Config/" + name + ".json";

		return File.Exists(path);
	}

	public void PlaySound()
	{
		//SoundDirector.ins.SE.Play("UISelect");
	}

	/// <summary>
	/// ウィンドウの更新
	/// </summary>
	public void ChangeScreen()
	{
		FullScreenMode mode;

		switch (content.screen.screenType.value)
		{
			case 0:
				mode = FullScreenMode.FullScreenWindow;
				break;
			case 1:
				mode = FullScreenMode.MaximizedWindow;
				break;
			default:
				mode = FullScreenMode.Windowed;
				break;
		}

		Vector2Int size = SettingData.Screen.Size[content.screen.resolution.value];

		Screen.SetResolution(size.x, size.y, mode);
	}
}
