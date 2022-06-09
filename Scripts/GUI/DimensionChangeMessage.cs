using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DimensionChangeMessage : MonoBehaviour
{
	public static DimensionChangeMessage ins = null;

	[SerializeField] TextMeshProUGUI tmp;
	float value;

	[SerializeField] float fontSize;
	[SerializeField] float fadeSpeed;
	[SerializeField] AnimationCurve curve;

	private void Awake()
	{
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
		value = 1;
	}

	void Update()
	{
		//徐々に大きさをもとに戻す
		value = value + Time.deltaTime / fadeSpeed;
		value = Mathf.Min(value, 1);

		tmp.fontSize = (curve.Evaluate(value) + 1) * fontSize;
	}

	/// <summary>
	/// テキストを更新
	/// </summary>
	/// <param name="text"></param>
	public void SetText(string text)
	{
		tmp.text = text;
		value = 0;
	}
}
