using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// トグルボタン
/// </summary>
public class ToggleButton : MonoBehaviour
{
	[SerializeField] Toggle button;
	[SerializeField] RectTransform rect;

    public void Flip()
	{
		TitleDirector.ins.PushCloseOpenStageSelect(button.isOn);
		rect.transform.localScale = new Vector3(button.isOn ? 1 : -1, 1, 1);
	}

	public void Flip(bool isOn)
	{
		button.isOn = isOn;
		TitleDirector.ins.PushCloseOpenStageSelect(button.isOn);
		rect.transform.localScale = new Vector3(button.isOn ? 1 : -1, 1, 1);
	}
}
