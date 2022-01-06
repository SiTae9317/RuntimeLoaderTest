using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System;

public enum AutoRiggingStatus
{
	StandBy = 0,
	ServerUpload,
	ServerProcessing,
	ServerDownload,
	Finish
}

public class AutoRiggingArg
{
	public AutoRiggingArg ()
	{
		rotX = 0.0;
		rotY = 0.0;
		rotZ = 0.0;
		height = 1.7;
		rig = "SmartBodyRig.dae";
		gender = "male";
		method = "harmonic";
	}

	public double rotX = 0.0, rotY = 0.0, rotZ = 0.0, height = 0.0;
	public string rig = null, gender = null, method = null;
}

public class AutoRigging : MonoBehaviour
{
	private const string requestUrl = "https://animservice.ict.usc.edu/webservices/php/Autorigging/autoRigging.php";

	private AutoRiggingStatus arStatus = AutoRiggingStatus.StandBy;
	private byte[] autoriggingData = null;
	private float progress = 0.0f;

	public void autoRiggingRequest(string path)
	{
		if(arStatus == AutoRiggingStatus.StandBy)
		{
			arStatus = AutoRiggingStatus.ServerUpload;
			StartCoroutine(postRequest(path));
		}
	}

	public void OnDestroy()
	{
		autoriggingData = null;
	}

	public float getProgress()
	{
		return progress;
	}

	public byte[] getAutoriggingData()
	{
		return autoriggingData;
	}

	public AutoRiggingStatus getAutoriggingStatus()
	{
		return arStatus;
	}

	IEnumerator postRequest(string path)
	{		
		string responseJson = null;

		using(RequestBody requestBody = new RequestBody())
		{
			AutoRiggingArg arArg = new AutoRiggingArg();

			requestBody.addHeaderFiled("User-Agent", "Android Multipart HTTP Client 1.0");
			requestBody.addFormField("rotX", arArg.rotX + "");
			requestBody.addFormField("rotY", arArg.rotY + "");
			requestBody.addFormField("rotZ", arArg.rotZ + "");
			requestBody.addFormField("height", arArg.height + "");
			requestBody.addFormField("rig", arArg.rig);
			requestBody.addFormField("gender", arArg.gender);
			requestBody.addFormField("interp", arArg.method);

			requestBody.addFromFile(path);

			Dictionary<string, string> header = new Dictionary<string, string>();

			header.Add("Connection", "Keep-Alive");
			header.Add("User-Agent", "Android Multipart HTTP Client 1.0");
			header.Add("Content-Type", "multipart/form-data;boundary=" + requestBody.getBoundary());
			header.Add("Accept-Encoding", "gzip");

			WWW www = new WWW(requestUrl, requestBody.getBodyToBytes(), header);
			
			while(!www.isDone)
			{
				yield return null;
				progress = www.uploadProgress;
				if(progress == 1.0f)
				{
					arStatus = AutoRiggingStatus.ServerProcessing;
				}
			}

			responseJson = www.text;
			Debug.Log(responseJson);

			www.Dispose();
			//requestBody.Dispose();
		}

		if(responseJson != null)
		{
			string[] responseData = responseJson.Split(new char[] { '{', '}' })[1].Trim().Split(',');
			
			Dictionary<string, string> jsonObj = new Dictionary<string, string>();
			
			for(int i = 0; i < responseData.Length; i++)
			{
				string[] jsonData = responseData[i].Trim().Split('"');
				
				jsonObj.Add(jsonData[1].Trim(), jsonData[3].Trim());
			}

			arStatus = AutoRiggingStatus.ServerDownload;
			StartCoroutine(getRequest(path, jsonObj["dir"], jsonObj["dae"]));
		}
		else
		{
			arStatus = AutoRiggingStatus.Finish;
		}
	}
	
	IEnumerator getRequest(string path, string dir, string dae)
	{		
		WWW www = new WWW(requestUrl + "?" + "dir=" + dir + "&name=" + dae);
		
		while(!www.isDone)
		{
			yield return null;
			progress = www.progress;
		}

		autoriggingData = www.bytes;
		arStatus = AutoRiggingStatus.Finish;

		/*
		FileStream fs = new FileStream(path + dae, FileMode.CreateNew, FileAccess.Write);
		fs.Write(www.bytes, 0, www.bytes.Length);
		fs.Close();
		fs = null;
		*/

		www.Dispose();
		www = null;
	}
}
