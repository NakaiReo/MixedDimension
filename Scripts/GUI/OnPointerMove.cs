using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;

/// <summary>
/// ポインターが乗っている時一定量動かす
/// </summary>
public class OnPointerMove : MonoBehaviour , IPointerEnterHandler , IPointerExitHandler,ISelectHandler , IDeselectHandler
{
	RectTransform rect;

	bool isSelect = false;

	Vector2 startScale;
	[SerializeField] Vector2 scale;
	[SerializeField] float time;

	private void Start()
	{
		rect = GetComponent<RectTransform>();

		startScale = rect.sizeDelta;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		isSelect = true;
		SoundDirector.ins.SE.Play("UIMove");
		rect.DOSizeDelta(startScale + scale, time);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isSelect = false;
		rect.DOSizeDelta(startScale, time);
	}

	public void OnSelect(BaseEventData eventData)
	{
		isSelect = true;
		SoundDirector.ins.SE.Play("UIMove");
		rect.DOSizeDelta(startScale + scale, time);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		isSelect = false;
		rect.DOSizeDelta(startScale, time);
	}
}
