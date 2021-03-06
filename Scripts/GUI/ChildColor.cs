using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// 子のオブジェクトの色を変える
/// </summary>
public class ChildColor : MonoBehaviour
{
	Button button;
	List<Graphic> targets;

	void Start()
	{
		button = GetComponent<Button>();
		targets = GetComponentsInChildren<Graphic>().Where(c => c.gameObject != gameObject).ToList();
	}

	void Update()
	{
		targets.ForEach(t => t.color = button.targetGraphic.canvasRenderer.GetColor());
	}
}
