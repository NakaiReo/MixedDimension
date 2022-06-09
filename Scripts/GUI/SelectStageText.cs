using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class SelectStageText : MonoBehaviour
{
	public static SelectStageText ins = null;

	RectTransform rect;
	TextMeshProUGUI textComponent;

	float width;
	float height;

	private void Awake()
	{
		if(ins == null)
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
		rect = GetComponent<RectTransform>();
		textComponent = GetComponent<TextMeshProUGUI>();

		width = rect.sizeDelta.x;
		height = rect.sizeDelta.y;
	}

	/// <summary>
	/// 選択されているステージを表示
	/// </summary>
    public void ChangeText()
	{
		if(StageSelectButton.currentButton != null)
		{
			string stageName = StageSelectButton.currentButton.stageNameComponent.text;

			rect.DOSizeDelta(new Vector2(width, height), 0.25f);
			textComponent.text = stageName;
		}
		else
		{
			rect.DOSizeDelta(new Vector2(0, height), 0.25f);
			textComponent.text = "未選択";
		}
	}
}
