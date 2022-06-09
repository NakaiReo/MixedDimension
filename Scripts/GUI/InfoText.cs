using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InfoText : MonoBehaviour
{
	public static InfoText ins = null;

	TextMeshProUGUI tmp;
	float value;

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
		tmp = GetComponent<TextMeshProUGUI>();
		value = 1;
    }

    void Update()
    {
		//徐々にフェードアウトさせる
		value = value + Time.deltaTime / fadeSpeed;
		value = Mathf.Min(value, 1);

		Color color = tmp.color;
		color.a = curve.Evaluate(value);
		tmp.color = color;
    }

	public void SetText(string text)
	{
		tmp.text = text;
		value = 0;
	}
}
