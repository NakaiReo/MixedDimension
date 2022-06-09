using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StageSelectButton : MonoBehaviour
{
	public static StageSelectButton currentButton = null;

	public int stageIndex;

	[Space(10)]
	[SerializeField] RectTransform rectTransform;
	[SerializeField] RectTransform hedderRect;
	[SerializeField] RectTransform listContentRect;

	[Space]
	public StageDataFile stageDataFile;
	public TextMeshProUGUI stageNameComponent;
	public TextMeshProUGUI stageClearComponent;
	//public Image stagePreviewImage;

	Vector2 GetSize(float listContentScale)
	{
		float height = hedderRect.sizeDelta.y * hedderRect.localScale.y + listContentRect.sizeDelta.y * listContentScale;
		return new Vector2(rectTransform.sizeDelta.x, height);
	}

    void Start()
    {
		OnValidate();
		Close(0);

		ClearText();
	}

	public void ClearText()
	{
		stageNameComponent.color = ClearData.ins.data.isClear[stageIndex] ? Color.yellow : Color.white;
		stageClearComponent.text = ClearData.ins.data.isClear[stageIndex] ? "クリア済み" : "未クリア";
		stageClearComponent.color = ClearData.ins.data.isClear[stageIndex] ? Color.yellow : Color.white;
	}

	public void PushButton()
	{
		var backCurrentButton = currentButton;
		currentButton = this;

		if (backCurrentButton == null)
		{
			CameraActive(currentButton.stageIndex, true);
			currentButton.Open(0.25f);
			SelectStageText.ins.ChangeText();
			SoundDirector.ins.SE.Play("UISelect");
			return;
		}
		if(backCurrentButton == currentButton)
		{
			CameraActive(currentButton.stageIndex, false);
			currentButton.Close(0.25f);
			currentButton = null;
			SoundDirector.ins.SE.Play("UICancel");
			SelectStageText.ins.ChangeText();
			return;
		}
		else
		{
			CameraActive(backCurrentButton.stageIndex, false);
			backCurrentButton.Close(0.25f);
			CameraActive(currentButton.stageIndex, true);
			currentButton.Open(0.25f);
			SoundDirector.ins.SE.Play("UISelect");
			SelectStageText.ins.ChangeText();
		}

		Canvas.ForceUpdateCanvases();

		//StageFolderSelectButton.currentButton.Open(0.25f);
	}

	public void CameraActive(int index, bool enable)
	{
		TitleDirector.ins.subMapFile.GetChild(index).GetComponent<SubMap>().viewCamera.SetActive(enable);
	}

	public void Open(float time)
	{
		rectTransform.DOSizeDelta(GetSize(1), time);
		listContentRect.DOScaleY(1, time);
	}

	public void Close(float time)
	{
		rectTransform.DOSizeDelta(GetSize(0), time);
		listContentRect.DOScaleY(0, time);
	}

	private void OnValidate()
	{
		if (stageDataFile == null) return;
		if (stageIndex < 0 || stageIndex >= stageDataFile.data.Count) return;

		StageDataObject dataBase = stageDataFile.data[stageIndex];
		stageNameComponent.text = dataBase.mapName;
	}
}
