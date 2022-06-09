using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CircleLayout : MonoBehaviour
{
	public float radius;
	public float offsetAngle;

	public void Layout()
	{
		RectTransform origin = GetComponent<RectTransform>();
		RectTransform[] rectTransform = new RectTransform[transform.childCount];

		for(int i = 0; i < transform.childCount; i++)
		{
			rectTransform[i] = transform.GetChild(i).GetComponent<RectTransform>();

			float addAngle = 360.0f / transform.childCount;
			Vector2 direction = Vector2.zero;
			direction.x = Mathf.Cos((addAngle * i * -1 + offsetAngle) * Mathf.Deg2Rad);
			direction.y = Mathf.Sin((addAngle * i * -1 + offsetAngle) * Mathf.Deg2Rad);

			rectTransform[i].position = (Vector2)origin.position + direction * radius;
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(CircleLayout))]
public class CircleLayoutEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		CircleLayout script = target as CircleLayout;

		EditorGUILayout.Space();
		if (GUILayout.Button("整列"))
		{
			script.Layout();
		}
	}
}
#endif