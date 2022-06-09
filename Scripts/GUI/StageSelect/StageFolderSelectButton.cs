using System.Collections;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class StageFolderSelectButton : MonoBehaviour
{
	public static StageFolderSelectButton currentButton = null;

	[SerializeField] string titleName;

	[Space(10)]
	[SerializeField] RectTransform rectTransform;
	[SerializeField] RectTransform hedderRect;
	[SerializeField] RectTransform listContentRect;

	[Space]
	public TextMeshProUGUI titleNameComponent;

	Vector2 GetSize(float listContentScale)
	{
		float height = hedderRect.sizeDelta.y * hedderRect.localScale.y + listContentRect.sizeDelta.y * listContentScale;
		return new Vector2(rectTransform.sizeDelta.x, height);
	}

	void Start()
	{
		Close(0);

		titleNameComponent.color = AllClear() ? Color.yellow : Color.white;
	}

	bool AllClear()
	{
		for (int i = 0; i < listContentRect.childCount; i++)
		{
			int index = listContentRect.GetChild(i).GetComponent<StageSelectButton>().stageIndex;

			if (ClearData.ins.data.isClear[index] != true) return false;
		}

		return true;
	}

	public void MoveScale(float offsetHeight)
	{
	}

	public void PushButton()
	{
		var backCurrentButton = currentButton;
		currentButton = this;

		if (backCurrentButton == null)
		{
			currentButton.Open(0.25f);
			SoundDirector.ins.SE.Play("UISelect");
			return;
		}
		if (backCurrentButton == currentButton)
		{
			currentButton.Close(0.25f);
			currentButton = null;
			SoundDirector.ins.SE.Play("UICancel");
			return;
		}
		else
		{
			backCurrentButton.Close(0.25f);
			currentButton.Open(0.25f);
			SoundDirector.ins.SE.Play("UISelect");
		}

		if(StageSelectButton.currentButton != null)
			StageSelectButton.currentButton.Close(0.25f);
		StageSelectButton.currentButton = null;

		Canvas.ForceUpdateCanvases();
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
		titleNameComponent.text = titleName;
	}
}
