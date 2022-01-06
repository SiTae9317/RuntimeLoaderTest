using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public enum ButtonStatus
{
	BtnNone = 0,
	BtnChar,
	BtnAni,
	BtnDress
}

[RequireComponent (typeof(RTLoadImport))]
public class RTManager : MonoBehaviour
{
	private GUISkin guiSkin;
	public GUIText calcTimeGUI;

	private string url = null;

	[HideInInspector]
	public RTLoadImport rtImporter = null;
	[HideInInspector]
	public Material[] useMat;
	public RuntimeAnimatorController[] aniController;
	public GameObject[] hatObj;
	public GameObject[] glassObj;
	public GameObject[] shirtObj;
	public GameObject[] pantsObj;

	private FollowCamera fCam = null;

	private System.DateTime calcTime;

	private ButtonStatus btnStat = ButtonStatus.BtnNone;

	private Rect fileBrowerBtnRect;
	private Rect aniBtnRect;
	private Rect dressBtnRect;

	private GUIStyle mainBtnStyle;

	private Vector2 scroll;

	void Awake()
	{
		// GUI Init
		float width = Screen.width;
		float height = Screen.height;

		fileBrowerBtnRect = new Rect(width * 1 / 6 - Screen.width / 12, height * 44 / 48 - Screen.height / 48, Screen.width / 6, Screen.height / 12);
		aniBtnRect = new Rect(width * 3 / 6 - Screen.width / 12, height * 44 / 48 - Screen.height / 48, Screen.width / 6, Screen.height / 12);
		dressBtnRect = new Rect(width * 5 / 6 - Screen.width / 12, height * 44 / 48 - Screen.height / 48, Screen.width / 6, Screen.height / 12);
	}

	void Start ()
	{		
		url = Application.dataPath + "/Resources/Meshes/Models/";//"C:/Models/";
		#if UNITY_ANDROID
		url = AndroidPath.filePath;
		#endif

		fCam = gameObject.GetComponent<FollowCamera>();
		rtImporter = gameObject.GetComponent<RTLoadImport>();
	}

	IEnumerator loadItemCorutine(ModelInfo modelInfo)
	{
		Debug.Log(modelInfo.name + " Loading...");
		Debug.Log(modelInfo.haveTex);

		if(rtImporter)
		{
			calcTime = System.DateTime.Now;
			calcTimeGUI.text = "";
			rtImporter.load(modelInfo);
		}
		while(!rtImporter.canLoad())
		{
			yield return null;
		}

		System.DateTime endTime = System.DateTime.Now;
		calcTimeGUI.text = (endTime - calcTime).Minutes.ToString() + "m" + (endTime - calcTime).Seconds.ToString() + "s" + (endTime - calcTime).Milliseconds.ToString() + "ms";
		applyAnimation(aniController[0]);
	}

	void OnGUI ()
	{
		if(rtImporter != null && rtImporter.canLoad())
		{
			mainBtnStyle = GUI.skin.button;
			mainBtnStyle.normal.textColor = Color.white;
			mainBtnStyle.fontSize = 40;

			if(GUI.Button(fileBrowerBtnRect, "Character", mainBtnStyle))
			{
				btnStat = (btnStat == ButtonStatus.BtnChar)? ButtonStatus.BtnNone : ButtonStatus.BtnChar;
			}
			if(GUI.Button(aniBtnRect, "Animation", mainBtnStyle))
			{
				btnStat = (btnStat == ButtonStatus.BtnAni)? ButtonStatus.BtnNone : ButtonStatus.BtnAni;
			}
			if(rtImporter.getLoadObjects().Count > 0)
			{
				if(rtImporter.getLoadObject(0).name.Contains("Model_Dress") && GUI.Button(dressBtnRect, "Dress", mainBtnStyle))
				{
					btnStat = (btnStat == ButtonStatus.BtnDress)? ButtonStatus.BtnNone : ButtonStatus.BtnDress;
				}
			}
		}
		else
		{
			return ;
		}
		
		if(btnStat == ButtonStatus.BtnChar)
		{
			Rect winRect = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2);
			fCam.setChkWinRect(winRect);
			GUIStyle guiStyle = GUI.skin.window;
			guiStyle.fontSize = 70;
			GUILayout.Window(1, winRect, characterWindow, "Character", guiStyle); 
		}
		else if(btnStat == ButtonStatus.BtnAni)
		{
			Rect winRect = new Rect(Screen.width / 4, Screen.height / 4, Screen.width / 2, Screen.height / 2);
			fCam.setChkWinRect(winRect);
			GUIStyle guiStyle = GUI.skin.window;
			guiStyle.fontSize = 70;
			GUILayout.Window(1, winRect, animationWindow, "Animation", guiStyle); 
		}
		else if(btnStat == ButtonStatus.BtnDress)
		{
			Rect winRect = new Rect(0, Screen.height * 3 / 4, Screen.width, Screen.height / 8);
			fCam.setChkWinRect(winRect);
			GUIStyle guiStyle = GUI.skin.window;
			guiStyle.fontSize = 70;
			GUILayout.Window(1, winRect, dressWindow, "Dress", guiStyle); 
		}
		else
		{
			fCam.setChkWinRect(new Rect(0,0,0,0));
		}
	}

	void applyAnimation(RuntimeAnimatorController rac)
	{
		if(rtImporter != null)
		{
			foreach(GameObject go in rtImporter.getLoadObjects())
			{
				go.GetComponent<Animator>().runtimeAnimatorController = rac;
			}
		}
	}

	void dressWindow(int windowID) 
	{ 
		openDressWindow();
	} 

	void openDressWindow() 
	{ 
		GUIStyle fileStyle = GUI.skin.button;
		fileStyle.fontSize = 50;
		fileStyle.normal.textColor = Color.white;

		scroll = GUILayout.BeginScrollView(scroll);
		scroll += fCam.getMovePos() * 50.0f;
		GUILayout.Label("\r\n\r\n\r\n\r\n");
		GUILayout.BeginHorizontal(); 

		for(int i = 0; i < hatObj.Length; i++)
		{
			GUILayout.BeginHorizontal(); 
			if(GUILayout.Button(("\r\n" + hatObj[i].name + "\r\n"), fileStyle) ) 
			{  
				if((rtImporter != null) && rtImporter.getLoadObjects().Count > 0)
				{
					hatObj[i].SetActive(!hatObj[i].activeSelf);
					hatObj[i].GetComponent<Dress>().setTracking(rtImporter.getLoadObject(0));

					for(int j = 0; j < hatObj.Length; j++)
					{
						if(i != j)
						{
							hatObj[j].SetActive(false);
						}
					}
				}
				
				btnStat = ButtonStatus.BtnNone;
			} 
			GUILayout.Box("", new GUIStyle());

			GUILayout.EndHorizontal(); 
		}

		for(int i = 0; i < glassObj.Length; i++)
		{
			GUILayout.BeginHorizontal(); 
			if(GUILayout.Button(("\r\n" + glassObj[i].name + "\r\n"), fileStyle) ) 
			{  
				if((rtImporter != null) && rtImporter.getLoadObjects().Count > 0)
				{
					glassObj[i].SetActive(!glassObj[i].activeSelf);
					glassObj[i].GetComponent<Dress>().setTracking(rtImporter.getLoadObject(0));

					for(int j = 0; j < glassObj.Length; j++)
					{
						if(i != j)
						{
							glassObj[j].SetActive(false);
						}
					}
				}

				btnStat = ButtonStatus.BtnNone;
			} 
			GUILayout.Box("", new GUIStyle());

			GUILayout.EndHorizontal(); 
		}

		for(int i = 0; i < shirtObj.Length; i++)
		{
			GUILayout.BeginHorizontal(); 
			if(GUILayout.Button(("\r\n" + shirtObj[i].name + "\r\n"), fileStyle) ) 
			{  
				if((rtImporter != null) && rtImporter.getLoadObjects().Count > 0)
				{
					shirtObj[i].SetActive(!shirtObj[i].activeSelf);
					shirtObj[i].GetComponent<Dress>().setTracking(rtImporter.getLoadObject(0));

					for(int j = 0; j < shirtObj.Length; j++)
					{
						if(i != j)
						{
							shirtObj[j].SetActive(false);
						}
					}
				}

				btnStat = ButtonStatus.BtnNone;
			} 
			GUILayout.Box("", new GUIStyle());

			GUILayout.EndHorizontal(); 
		}

		for(int i = 0; i < pantsObj.Length; i++)
		{
			GUILayout.BeginHorizontal(); 
			if(GUILayout.Button(("\r\n" + pantsObj[i].name + "\r\n"), fileStyle) ) 
			{  
				if((rtImporter != null) && rtImporter.getLoadObjects().Count > 0)
				{
					pantsObj[i].SetActive(!pantsObj[i].activeSelf);
					pantsObj[i].GetComponent<Dress>().setTracking(rtImporter.getLoadObject(0));

					for(int j = 0; j < pantsObj.Length; j++)
					{
						if(i != j)
						{
							pantsObj[j].SetActive(false);
						}
					}
				}

				btnStat = ButtonStatus.BtnNone;
			} 
			GUILayout.Box("", new GUIStyle());

			GUILayout.EndHorizontal(); 
		}
		GUILayout.EndHorizontal();
		GUILayout.EndScrollView();
	} 

	
	void animationWindow(int windowID) 
	{ 
		openAniWindow();
	} 

	void openAniWindow() 
	{ 
		GUIStyle fileStyle = GUI.skin.button;
		fileStyle.fontSize = 50;
		fileStyle.normal.textColor = Color.white;

		scroll = GUILayout.BeginScrollView (scroll); 
		scroll += fCam.getMovePos() * 50.0f;
		GUILayout.BeginVertical(); 
		GUILayout.Label("\r\n\r\n\r\n\r\n");

		for(int i = 0; i < aniController.Length; i++)
		{
			GUILayout.BeginVertical(); 
			if(GUILayout.Button(("\r\n" + aniController[i].name + "\r\n"), fileStyle) ) 
			{  
				applyAnimation(aniController[i]);
				btnStat = ButtonStatus.BtnNone;
			} 
			GUILayout.EndVertical(); 
			GUILayout.Label("", new GUIStyle());
		}

		GUILayout.EndVertical(); 
		GUILayout.EndScrollView (); 
	} 
	
	void characterWindow(int windowID) 
	{ 
		openCharWindow(url);
	} 
	
	void openCharWindow(string path) 
	{ 
		scroll = GUILayout.BeginScrollView (scroll); 
		scroll += fCam.getMovePos() * 50.0f;
		GUILayout.BeginVertical(); 
		GUILayout.Label("\r\n\r\n\r\n\r\n");
		characterSelect(path, 0, 0);    
		GUILayout.EndVertical(); 
		GUILayout.EndScrollView (); 
	} 
	
	void characterSelect(string path, int spaceNum, int index) 
	{ 
		FileInfo fileSelection; 
		DirectoryInfo directoryInfo; 

		directoryInfo = new DirectoryInfo(path); 
		
		GUILayout.BeginVertical();  
		
		fileSelection = selectList(directoryInfo.GetFiles(), null, null, spaceNum) as FileInfo;  

		GUILayout.EndVertical();   
		
		if(fileSelection != null)  
		{
			ModelInfo tempModelinfo = new ModelInfo();
			string[] infoData = fileSelection.Name.Split('.');
			tempModelinfo.name = infoData[0].Trim();
			tempModelinfo.format = infoData[1].Trim();
			tempModelinfo.directory = fileSelection.Directory.FullName.Replace("\\", "/");

			foreach(FileInfo fi in directoryInfo.GetFiles("*.bin"))
			{
				if(fi.Name.ToLower().Equals(tempModelinfo.name.ToLower() + ".bin"))
				{
					tempModelinfo.format = "bin";
					break;
				}
			}

			foreach(FileInfo fi in directoryInfo.GetFiles("*.jpg"))
			{
				if(fi.Name.ToLower().Equals(tempModelinfo.name.ToLower() + ".jpg"))
				{
					tempModelinfo.haveTex = true;
					break;
				}
			}

			for(int i = 0; i < hatObj.Length; i++)
			{
				hatObj[i].SetActive(false);
			}

			for(int i = 0; i < glassObj.Length; i++)
			{
				glassObj[i].SetActive(false);
			}

			for(int i = 0; i < shirtObj.Length; i++)
			{
				shirtObj[i].SetActive(false);
			}

			for(int i = 0; i < pantsObj.Length; i++)
			{
				pantsObj[i].SetActive(false);
			}

			if((rtImporter != null) && rtImporter.getLoadObjects().Count > 0)
			{
				rtImporter.unload();
			}

			StartCoroutine(loadItemCorutine(tempModelinfo));

			btnStat = ButtonStatus.BtnNone;
		}
	} 
	
	private object selectList( ICollection list, object selected, Texture image, int spaceNum)
	{ 
		GUIStyle fileStyle = GUI.skin.button;
		fileStyle.fontSize = 50;
		fileStyle.normal.textColor = Color.white;

		foreach( object item in list ) 
		{ 
			FileSystemInfo info = item as FileSystemInfo; 

			if(info.Extension.ToLower().Contains("dae") ||
				info.Extension.ToLower().Contains("obj") ||
			  	info.Extension.ToLower().Contains("fbx"))
			{
				GUILayout.BeginHorizontal();
				GUILayout.Space(spaceNum);

				if( GUILayout.Button(("\r\n" + info.Name + "\r\n"), fileStyle) ) 
				{  
					selected = item;  
				} 
				
				GUILayout.EndHorizontal();  
				GUILayout.Label("", new GUIStyle());
			}
		}  
		
		return selected; 
	} 
}