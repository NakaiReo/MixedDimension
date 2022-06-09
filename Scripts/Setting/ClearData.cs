using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class ClearDataObject
{
	public bool[] isClear;
}

/// <summary>
/// クリア状況のデータ
/// </summary>
public class ClearData : MonoBehaviour
{
	public static ClearData ins = null;
	
	StageDataFile stageDataFile; //ステージデータファイル
	public ClearDataObject data; //クリア状況

	private void Awake()
	{
		if(ins == null)
		{
			ins = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
    {
		stageDataFile = Resources.Load("StageDataFile") as StageDataFile;

		//セーブデータが存在しているかどうか、なければ作る
		if (!Directory.Exists(Application.dataPath + "/Config"))
		{
			Directory.CreateDirectory(Application.dataPath + "/Config");
		}
		if (!Directory.Exists(Application.dataPath + "/Config/SaveData"))
		{
			Directory.CreateDirectory(Application.dataPath + "/Config/SaveData");
		}
		if (!CheckFile("Clear"))
		{
			data = new ClearDataObject();
			data.isClear = new bool[stageDataFile.data.Count];
			WriteJson("Clear", data);
		}

		data = ReadJson<ClearDataObject>("Clear");
	}

	/// <summary>
	/// クリアしたという情報を残す
	/// </summary>
	/// <param name="index">ステージインデックス</param>
	public void SetClearFlag(int index)
	{
		data.isClear[index] = true;
		WriteJson("Clear", data);
	}

	/// <summary>
	/// ファイルの書き込み
	/// </summary>
	void WriteJson<ClassT>(string name, ClassT data)
	{
		StreamWriter writer;

		string jsonstr = JsonUtility.ToJson(data);

		writer = new StreamWriter(Application.dataPath + "/Config/SaveData/" + name + ".json", false);
		writer.Write(jsonstr);
		writer.Flush();
		writer.Close();
	}

	/// <summary>
	/// ファイルの読み込み
	/// </summary>
	ClassT ReadJson<ClassT>(string name)
	{
		string path = Application.dataPath + "/Config/SaveData/" + name + ".json";

		if (File.Exists(path))
		{
			string datastr = "";
			StreamReader reader;
			reader = new StreamReader(path);
			datastr = reader.ReadToEnd();
			reader.Close();

			return JsonUtility.FromJson<ClassT>(datastr);
		}

		return JsonUtility.FromJson<ClassT>("");
	}

	/// <summary>
	/// ファイルのチェック
	/// </summary>
	bool CheckFile(string name)
	{
		string path = Application.dataPath + "/Config/SaveData/" + name + ".json";

		return File.Exists(path);
	}
}
