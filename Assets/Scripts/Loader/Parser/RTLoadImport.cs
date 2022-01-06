using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum FileFormat
{
	None = 0,
	Bin,
	Dae,
	Fbx
}	

[RequireComponent (typeof(LoaderThread))]
[RequireComponent (typeof(GenericToHumanoid))]
public class RTLoadImport : MonoBehaviour 
{
	private Rect winRect;
	[HideInInspector]

	private List<GameObject> loadObjects = null;
	private Texture2D loadTex = null;

	private string windowTitle = "";
	private float progress = 0.0f;

	private bool doLoad = true;
	private bool done = false;

	public bool isDone 
	{
		get 
		{
			return done;
		}
		set 
		{
			done = value;
		}
	}

	void Start ( ) 
	{
		loadObjects = new List<GameObject>();
	}

	public void load (ModelInfo modelInfo) 
	{
		if (doLoad == true) 
		{
			doLoad = false;
			isDone = false;
			StartCoroutine (this.iLoad (modelInfo));
		}
	}

	IEnumerator iLoad(ModelInfo modelInfo) 
	{
		//string s = null;
		byte[] b = null;

		FileFormat fileFormat = FileFormat.None;

		if(modelInfo.format.ToLower().Contains("obj"))
		{
			GameObject arGO = new GameObject("arGO");
			arGO.AddComponent<AutoRigging>();

			AutoRigging ar = arGO.GetComponent<AutoRigging>();

			ar.autoRiggingRequest(modelInfo.getObjectFullName());

			while(ar.getAutoriggingStatus() != AutoRiggingStatus.Finish)
			{
				yield return null;
				windowTitle = ar.getAutoriggingStatus().ToString();
				progress = ar.getProgress() * 100.0f;
			}
			b = ar.getAutoriggingData();

			Destroy(arGO);
			fileFormat = FileFormat.Dae;
		}
		else if(modelInfo.format.ToLower().Contains("dae"))
		{
			fileFormat = FileFormat.Dae;
			b = System.IO.File.ReadAllBytes(modelInfo.getObjectFullName());
			//s = System.Text.Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(modelInfo.getObjectFullName()));
		}
		else if(modelInfo.format.ToLower().Contains("fbx"))
		{
			fileFormat = FileFormat.Fbx;
			b = System.IO.File.ReadAllBytes(modelInfo.getObjectFullName());
			//s = System.Text.Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(modelInfo.getObjectFullName()));
		}
		else if(modelInfo.format.ToLower().Contains("bin"))
		{
			fileFormat = FileFormat.Bin;
			b = System.IO.File.ReadAllBytes(modelInfo.getObjectFullName());
			//s = System.Text.Encoding.UTF8.GetString(System.IO.File.ReadAllBytes(modelInfo.getObjectFullName()));
		}

		windowTitle = modelInfo.name + " Loading...";


		if (b != null) 
		{
			using (RTLoader rtLoader = new RTLoader(b, 0.01f)) 
			{
				rtLoader.load (fileFormat);

				while (!rtLoader.isDone) 
				{
					progress = (rtLoader.progress * 100.0f);
					yield return null;
				}
				progress = (rtLoader.progress * 100.0f);

				if(modelInfo.haveTex)
				{
					loadTex = new Texture2D(1024, 1024);
					loadTex.LoadImage(System.IO.File.ReadAllBytes(modelInfo.getTextureFullName()));
				}

				if(fileFormat != FileFormat.Bin)
				{
					rtLoader.exportData(ref rtLoader.rtlObj, modelInfo.directory, modelInfo.name);
				}

				loadObjects.Add(loadToScene(ref rtLoader.rtlObj, modelInfo.name));

				loadTex = null;

				//rtLoader.Dispose ();
			}
		}
		else 
		{
			Debug.Log(modelInfo.name + " could not found");
		}

		b = null;
		doLoad = true;
	}

	public List<GameObject> getLoadObjects ( ) 
	{
		return loadObjects;
	}

	public GameObject getLoadObject(int index) 
	{
		if(index >= 0 && index < loadObjects.Count)
		{
			return loadObjects[index];
		}
		else
		{
			return null;
		}
	}

	public void unload ( ) 
	{
		int objCount = loadObjects.Count;
		for(int i = objCount - 1; i >= 0; i--)
		{
			Destroy (loadObjects[i]);
			loadObjects.RemoveAt(i);
		}
	}

	public bool canLoad()
	{
		return doLoad;
	}

	private GameObject loadToScene(ref RTLoadObject rtLoadObj, string name)
	{		
		GameObject loadObject = new GameObject();
		loadObject.name = name;

		rtLoadObj.skeletons = new List<GameObject>();

		for(int i = 0; i < rtLoadObj.rds.Count; i++)
		{
			GameObject baseObj = new GameObject();
			GameObject skelObject = Instantiate(baseObj) as GameObject;
			skelObject.name = rtLoadObj.rds[i].name;
			rtLoadObj.skeletons.Add(skelObject);
			DestroyObject(baseObj);
		}

		for(int i = 0; i < rtLoadObj.rds.Count; i++)
		{
			for(int j = 0; j < rtLoadObj.skeletons.Count; j++)
			{
				if(rtLoadObj.rds[i].parent.Equals(rtLoadObj.skeletons[j].name))
				{
					rtLoadObj.skeletons[i].transform.parent = rtLoadObj.skeletons[j].transform;
					break;
				}
				else if(rtLoadObj.rds[i].parent.Equals("RootNode"))
				{
					rtLoadObj.skeletons[i].transform.parent = loadObject.transform;
				}
			}
		}

		for(int i = 0; i < rtLoadObj.skeletons.Count; i++)
		{				
			rtLoadObj.skeletons[i].transform.localPosition = rtLoadObj.rds[i].localPosition;
			rtLoadObj.skeletons[i].transform.localRotation = Quaternion.Euler (rtLoadObj.rds[i].localRotation);
			rtLoadObj.skeletons[i].transform.localScale = rtLoadObj.rds[i].localScale;
		}

		for(int i = 0; i < rtLoadObj.bindPose.Count; i++)
		{
			for(int j = 0; j < rtLoadObj.rds.Count; j++)
			{
				if(rtLoadObj.rds[j].boneNum == i)
				{
					rtLoadObj.skinBone.Add(rtLoadObj.skeletons[j].transform);
				}
			}
		}

		for(int j = 0; j < rtLoadObj.rtMesh.Count; j++)
		{
			//yield return null;
			Mesh newMesh = new Mesh();
			newMesh.vertices = rtLoadObj.rtMesh[j].vertices.ToArray();
			newMesh.normals = rtLoadObj.rtMesh[j].normals.ToArray();
			newMesh.triangles = rtLoadObj.rtMesh[j].triangles.ToArray();
			newMesh.uv = rtLoadObj.rtMesh[j].uvs.ToArray();
			newMesh.bindposes = rtLoadObj.bindPose.ToArray();
			newMesh.boneWeights = rtLoadObj.rtMesh[j].bws.ToArray();

			GameObject createObj = new GameObject();
			GameObject MeshObject = Instantiate (createObj, transform.position, transform.rotation) as GameObject;
			MeshObject.name = "MeshObject_";
			MeshObject.name += j;
			MeshObject.transform.parent = loadObject.transform;
			DestroyObject(createObj);
			newMesh.RecalculateBounds();
			newMesh.RecalculateNormals();
			MeshObject.AddComponent<SkinnedMeshRenderer>();
			MeshObject.GetComponent<SkinnedMeshRenderer>().bones = rtLoadObj.skinBone.ToArray();
			MeshObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = newMesh;

			for(int i = 0; i < rtLoadObj.skeletons.Count; i++)
			{
				if(rtLoadObj.skeletons[i].name.Equals("JtRoot"))
				{
					rtLoadObj.skeletons[i].transform.localScale = new Vector3(100.0f, 100.0f, 100.0f);
					MeshObject.GetComponent<SkinnedMeshRenderer>().rootBone = rtLoadObj.skeletons[i].transform;
					break;
				}
			}
		}

		loadObject.AddComponent<RTTexture>();
		loadObject.GetComponent<RTTexture>().setTexture(loadTex);

		loadObject.AddComponent<Animator>();
		loadObject.transform.localPosition += new Vector3(0, 0, 0);

		GenericToHumanoid gth = gameObject.GetComponent<GenericToHumanoid>();
		gth.setGenericObject(loadObject);
		gth.setRootNode(loadObject.transform.Find("JtRoot").gameObject);
		gth.humanAni();

		return loadObject;
	}

	/*
	public static Texture2D ResizeTexture(Texture2D source, Vector2 size)
	{
		Color[] aSourceColor = source.GetPixels(0);
		Vector2 vSourceSize = new Vector2(source.width, source.height);

		float xWidth = size.x;
		float xHeight = size.y;

		Texture2D oNewTex = new Texture2D((int)xWidth, (int)xHeight, TextureFormat.RGBA32, false);

		int xLength = (int)xWidth * (int)xHeight;
		Color[] aColor = new Color[xLength];

		Vector2 vPixelSize = new Vector2(vSourceSize.x / xWidth, vSourceSize.y / xHeight);

		Vector2 vCenter = new Vector2();
		for(int ii= 0; ii < xLength; ii++)
		{
			float xX = (float)ii % xWidth;
			float xY = Mathf.Floor((float)ii / xWidth);

			vCenter.x = (xX / xWidth) * vSourceSize.x;
			vCenter.y = (xY / xHeight) * vSourceSize.y;

			int xXFrom = (int)Mathf.Max(Mathf.Floor(vCenter.x - (vPixelSize.x * 0.5f)), 0);
			int xXTo = (int)Mathf.Min(Mathf.Ceil(vCenter.x + (vPixelSize.x * 0.5f)), vSourceSize.x);
			int xYFrom = (int)Mathf.Max(Mathf.Floor(vCenter.y - (vPixelSize.y * 0.5f)), 0);
			int xYTo = (int)Mathf.Min(Mathf.Ceil(vCenter.y + (vPixelSize.y * 0.5f)), vSourceSize.y);

			Vector4 oColorTotal = new Vector4();
			Color oColorTemp = new Color();
			float xGridCount = 0;

			for(int iy = xYFrom; iy < xYTo; iy++)
			{
				for(int ix = xXFrom; ix < xXTo; ix++)
				{
					oColorTemp += aSourceColor[(int)(((float)iy * vSourceSize.x) + ix)];

					xGridCount++;
				}
			}

			aColor[ii] = oColorTemp / (float) xGridCount;
		}

		oNewTex.SetPixels(aColor);
		oNewTex.Apply();

		return oNewTex;
	}
	*/

	void OnGUI()
	{
		if(!doLoad)
		{
			GUIStyle guiStyle = GUI.skin.window;
			guiStyle.fontSize = 50;
			winRect = new Rect(Screen.width / 4, Screen.height / 2 - Screen.width / 8, Screen.width / 2, Screen.width / 4);
			GUILayout.Window(2, winRect, progressWindow, "\r\n" + windowTitle, guiStyle); 
		}
	}

	void progressWindow(int windId)
	{
		GUILayout.BeginVertical(); 
		GUILayout.Label("\r\n\r\n\r\n\r\n");

		GUIStyle guiStyle = GUI.skin.box;
		guiStyle.alignment = TextAnchor.MiddleCenter;
		guiStyle.fontSize = 40;

		GUI.Box(new Rect(winRect.width / 10, winRect.height * 3 / 5, (winRect.width * 0.8f), winRect.height / 4), ((int)progress).ToString() + "%", guiStyle);
		GUI.Box(new Rect(winRect.width / 10, winRect.height * 3 / 5, (winRect.width * 0.8f) * (progress / 100f), winRect.height / 4), "");

		GUILayout.EndVertical(); 
	}
}
