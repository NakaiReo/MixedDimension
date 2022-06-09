using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpdatePanel : MonoBehaviour
{
	RectTransform rect;

	void Start()
	{
		rect = GetComponent<RectTransform>();
	}

	void Update()
	{
		LayoutRebuilder.MarkLayoutForRebuild(rect);
	}
}
