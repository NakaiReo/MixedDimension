using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;
using Trisibo;

public class GameDirector : MonoBehaviour
{
	public static GameDirector ins = null;
	bool isFade = false;

	KeyBind input;
	StageDataFile stageDataFile = null;

	public GameObject player;				   //プレイヤーのオブジェクト
	public PlayerController playerController;  //プレイヤーコントローラ
	public GameObject checkPoint;              //プレイヤーの前の位置

	[Space]
	public VirtualMouse virtualMouse;
	[SerializeField] GameObject infoText; //情報テキスト
	[SerializeField] GameObject dimensionText; //現在の次元テキスト
	[SerializeField] TextMeshProUGUI stageClearText; //ステージクリアテキスト
	[SerializeField] RectTransform stageClearPanel;  //ステージクリアパネル
	//[SerializeField] Button firstSelect; //最初に選ばれるButton

	[Space]
	public GameObject cameraChangePanel; //カメラ変更パネル
	public GameObject dimensionChangePanel; //次元変更パネル
	public EPOOutline.Outliner outliner; //アウトライン
	public CameraController cameraController;　

	[HideInInspector] public bool isGoal; //ゴールしたか
	[HideInInspector] public GameObject TutorialText = null;

	private void Awake()
	{
		if (ins == null)
		{
			ins = this;
			input = new KeyBind();
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
    {
		SoundDirector.ins.BGM.PlayLoop("Game");

		virtualMouse.DisableVirtualMouse();
		checkPoint.SetActive(false);

		SettingMenu.ins.Setup();
		isGoal = false;
		isFade = false;

		//ステージ読み込み
		stageDataFile = Resources.Load("StageDataFile") as StageDataFile;

		//クリア画面を非表示
		stageClearText.gameObject.SetActive(false);
		stageClearPanel.gameObject.SetActive(false);

		Time.timeScale = 1.0f;

		Resources.UnloadUnusedAssets();
	}

	private void Update()
	{
		//if (Time.timeScale <= 0.0f) return;
		//if(isGoal == false && virtualMouse.isActive == true)  virtualMouse.DisableVirtualMouse();
		//if(isGoal == true  && virtualMouse.isActive == false) virtualMouse.EnableVirtualMouse();
	}

	/// <summary>
	/// ゴールをしたときの処理
	/// </summary>
	public void Goal()
	{
		StartCoroutine(I_Goal());
	}

	IEnumerator I_Goal()
	{
		GameDirector.ins.isGoal = true; //ゴールをした情報を残す
		KeyInfoGUI.ins.SetActive(false); //キー情報を非表示に
		ClearData.ins.SetClearFlag(PlayerPrefs.GetInt("StageIndex")); //クリアデータを残す
		SoundDirector.ins.SE.Play("Goal");

		if (TutorialText != null) TutorialText.SetActive(false); //チュートリアルメッセージを非表示に

		if (infoText != null)  infoText.gameObject.SetActive(false); //情報テキストを非表示に
		if (dimensionText != null) dimensionText.gameObject.SetActive(false); //次元テキストを非表示に

		//クリア演出の初期化
		player.GetComponent<CameraController>().CameraViewChange(CameraController.CameraType.PlayerView); //カメラをプレイヤーの正面へ
		stageClearText.gameObject.SetActive(true);
		stageClearPanel.gameObject.SetActive(true);
		stageClearText.text = "Stage Clear";
		stageClearText.DOFade(0, 0);
		stageClearPanel.transform.localScale = new Vector3(0, 1, 1);

		yield return new WaitForSeconds(1.5f);

		DOTweenTMPAnimator tmproAnimator = new DOTweenTMPAnimator(stageClearText);

		player.GetComponent<Animator>().SetBool("Crab", true);
		player.GetComponent<Animator>().SetBool("egao", true);

		//クリアテキストを順次に表示させる
		int length = tmproAnimator.textInfo.characterCount;
		float time = 0.2f;
		for (int i = 0; i < length; ++i)
		{
			tmproAnimator.DOScaleChar(i, 0.7f, 0);
			Vector3 currCharOffset = tmproAnimator.GetCharOffset(i);
			DOTween.Sequence()
				.Append(tmproAnimator.DOOffsetChar(i, currCharOffset + new Vector3(0, 30, 0), 0.4f).SetEase(Ease.OutFlash, 2))
				.Join(tmproAnimator.DOFadeChar(i, 1, 0.4f))
				.Join(tmproAnimator.DOScaleChar(i, 1, 0.4f).SetEase(Ease.OutElastic))
				.AppendInterval((length + 1) * time)
				.Append(tmproAnimator.DOColorChar(i, new Color(1f, 1f, 0.8f), 0.2f).SetLoops(2, LoopType.Yoyo))
				.SetDelay(time * i);
		}

		yield return new WaitForSeconds(length * time + 1.0f);

		virtualMouse.EnableVirtualMouse();
		stageClearPanel.DOScaleX(1.0f, 1.0f).SetEase(Ease.OutBack);
		//firstSelect.Select();

		//GameClearAnimator.SetTrigger("GameClear");

		yield break;
	}

	/// <summary>
	/// 次のステージへ
	/// </summary>
	public void NextStageButton()
	{
		//フェード中じゃないか
		if (isFade == true) return;

		isFade = true;
		int index = PlayerPrefs.GetInt("StageIndex") + 1; //次のステージのIDを取得

		//最後のステージだったらタイトルへ
		if (index >= stageDataFile.data.Count)
		{
			Fade.ins.FadeIn("Title");
			return;
		}

		PlayerPrefs.SetInt("StageIndex", index);
		Fade.ins.FadeIn("Game");
	}

	/// <summary>
	/// タイトルへ
	/// </summary>
	public void GoTitleSelect()
	{
		//フェード中じゃないか
		if (isFade == true) return;

		int index = PlayerPrefs.GetInt("StageIndex") + 1;  //次のステージのIDを取得

		//次のステージのIDがはみ出ていないか
		if (index >= stageDataFile.data.Count)
		{
			PlayerPrefs.SetInt("StageIndex", stageDataFile.data.Count - 1);
		}

		isFade = true;
		Fade.ins.FadeIn("Title");
	}

	private void OnEnable() => input.Enable();
	private void OnDisable() => input.Disable();
	private void OnDestroy() => input.Dispose();
}
