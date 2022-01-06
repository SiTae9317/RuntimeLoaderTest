using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;
using System.IO;

public partial class RTLoader : IDisposable 
{
	public RTLoadObject rtlObj = null;
	public float progress = 0.0f;
	private float scaleFactor = 0.01f;

	private bool done = false;

	private string filename;
	private string contents;
	private byte[] byteContents;

	private Boolean disposed;

	~RTLoader ( ) 
	{
		this.Dispose (false);
	}

	protected virtual void Dispose (bool disposing) 
	{
		if (!this.disposed)
		{
			if (disposing)
			{
				rtlObj = null;
			}
		}
		this.disposed = true;
	}

	public RTLoader()
	{
		progress = 0.0f;
	}

	public RTLoader (string text)
	{
		this.contents = text;
		progress = 0.0f;
	}

	public RTLoader (string text, float scaleFactor)
	{
		this.contents = text;
		this.scaleFactor = scaleFactor;
		progress = 0.0f;
	}

	public RTLoader (byte[] byteContent, float scaleFactor)
	{
		this.byteContents = byteContent;
		this.scaleFactor = scaleFactor;
		progress = 0.0f;
	}

	public void Dispose ( ) 
	{
		if(rtlObj != null)
		{
			rtlObj.Dispose();
		}
		rtlObj = null;

		byteContents = null;
		contents = null;
	}

	public void load (FileFormat fileFormat) 
	{
		switch(fileFormat)
		{
			case FileFormat.Bin :
				LoaderThread.runAsync (this.binDoWorkAsync);
				break;

			case FileFormat.Fbx :
				LoaderThread.runAsync (this.fbxDoWorkAsync);
				break;

			case FileFormat.Dae :
				LoaderThread.runAsync (this.daeDoWorkAsync);
				break;
		}
	}

	private void binDoWorkAsync ( ) 
	{
		this.rtlObj = new RTLoadObject();

		MemoryStream ms = new MemoryStream(byteContents);
		using(BinaryReader br = new BinaryReader(ms))
		{
			int rdsCount = br.ReadInt32();
			for(int i = 0; i < rdsCount; i++)
			{
				RigData rd = new RigData();
				rd.boneNum = br.ReadInt32();
				rd.name = br.ReadString();
				rd.parent = br.ReadString();

				rd.localPosition = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				rd.localRotation = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
				rd.localScale = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());

				this.rtlObj.rds.Add(rd);
			}

			this.progress = 0.1f;

			int bindposeCount = br.ReadInt32();
			for(int i = 0; i < bindposeCount; i++)
			{
				Matrix4x4 bpData = new Matrix4x4();
				bpData.m00 = br.ReadSingle();
				bpData.m01 = br.ReadSingle();
				bpData.m02 = br.ReadSingle();
				bpData.m03 = br.ReadSingle();
				bpData.m10 = br.ReadSingle();
				bpData.m11 = br.ReadSingle();
				bpData.m12 = br.ReadSingle();
				bpData.m13 = br.ReadSingle();
				bpData.m20 = br.ReadSingle();
				bpData.m21 = br.ReadSingle();
				bpData.m22 = br.ReadSingle();
				bpData.m23 = br.ReadSingle();
				bpData.m30 = br.ReadSingle();
				bpData.m31 = br.ReadSingle();
				bpData.m32 = br.ReadSingle();
				bpData.m33 = br.ReadSingle();

				this.rtlObj.bindPose.Add(bpData);
			}

			this.progress = 0.2f;

			int rtMeshCount = br.ReadInt32();
			for(int i = 0; i < rtMeshCount; i++)
			{
				RTMesh rtMesh = new RTMesh();

				int verticesCount = br.ReadInt32();
				for(int j = 0; j < verticesCount; j++)
				{
					rtMesh.vertices.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
				}

				int triangleCount = br.ReadInt32();
				for(int j = 0; j < triangleCount; j++)
				{
					rtMesh.triangles.Add(br.ReadInt32());
				}

				int normalsCount = br.ReadInt32();
				for(int j = 0; j < normalsCount; j++)
				{
					rtMesh.normals.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
				}

				int uvsCount = br.ReadInt32();
				for(int j = 0; j < uvsCount; j++)
				{
					rtMesh.uvs.Add(new Vector2(br.ReadSingle(), br.ReadSingle()));
				}

				int bwsCount = br.ReadInt32();
				for(int j = 0; j < bwsCount; j++)
				{
					BoneWeight bw = new BoneWeight();

					bw.boneIndex0 = br.ReadInt32();
					bw.boneIndex1 = br.ReadInt32();
					bw.boneIndex2 = br.ReadInt32();
					bw.boneIndex3 = br.ReadInt32();
					bw.weight0 = br.ReadSingle();
					bw.weight1 = br.ReadSingle();
					bw.weight2 = br.ReadSingle();
					bw.weight3 = br.ReadSingle();

					rtMesh.bws.Add(bw);
				}

				this.rtlObj.rtMesh.Add(rtMesh);

				this.progress += ((0.7f / rtMeshCount));
			}

			br.Close();

			this.progress = 0.9f;
		}
		ms.Close();

		this.isDone = true;
	}	

	private void fbxDoWorkAsync ( ) 
	{
		this.contents = Encoding.UTF8.GetString(byteContents);

		this.rtlObj = new RTLoadObject();

		List<string> list = new List<string> ();
		list.AddRange (contents.Split (new char[2] { '{', '}' }));

		this.progress = 0.1f;

		this.getMeshes (list, ref rtlObj);

		this.getClusters (list, ref rtlObj);

		this.getModels (list, ref rtlObj);

		this.splitSkinnedMesh (ref rtlObj);

		int f = -1;
		int c = 0;
		foreach (string s in list) 
		{
			if (s.Contains ("C: \"OO\",")) 
			{
				f = c;
				break;
			}
			c++;
		}
		string[] lines = list[f].Split (new char[] { '\r', '\n' });

		int boneNumber = -1;

		for (int i = 0; i < lines.Length; i = i + 1) 
		{
			string[] data = lines[i].Split (new char[] {',', ' '});

			if(data.Length < 3)
			{
				continue;
			}

			if(data[0].Trim().Contains("Model::") && data[2].Trim().Contains("Model::"))
			{
				// rootnode
				string[] nodeChild = data[0].Split(':');
				string[] nodeParents = data[2].Split(':');

				string childName = nodeChild[nodeChild.Length - 1].Trim();
				string parentsName = nodeParents[nodeParents.Length - 1].Trim();

				for(int j = 0; j < this.rtlObj.rds.Count; j++)
				{
					if(this.rtlObj.rds[j].name.Trim().Equals(childName))
					{
						this.rtlObj.rds[j].parent = parentsName;
						break;
					}
				}
			}

			if(data[0].Trim().Contains("Model::") && data[2].Trim().Contains("SubDeformer::"))
			{
				string[] node = data[0].Split(':');

				string nodeName = node[node.Length - 1].Trim();

				for(int j = 0; j < this.rtlObj.rds.Count; j++)
				{
					if(this.rtlObj.rds[j].name.Equals(nodeName))
					{
						boneNumber++;
						this.rtlObj.rds[j].boneNum = boneNumber;
						break;
					}
				}
			}
		}
		this.isDone = true;
	}	

	private void daeDoWorkAsync ( ) 
	{
		this.contents = Encoding.UTF8.GetString(byteContents);

		rtlObj = new RTLoadObject();

		List<string> list = new List<string> ();
		list.AddRange (contents.Split (new char[2] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

		/*
		int objScaleIndex = 0;
		int geomeryIdIndex = 0;

		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Contains("unit meter"))
			{
				objScaleIndex = i;
			}
			else if(list[i].Contains("geometry id="))
			{
				geomeryIdIndex = i;
			}
		}
		*/

		progress = 0.1f;
		Debug.Log("Parsing Split");

		this.daeGetMesh(ref list, ref rtlObj);

		this.daeGetBindpose(ref list, ref rtlObj);

		this.daeGetBoneWeight(ref list, ref rtlObj);

		this.splitSkinnedMesh (ref rtlObj);

		this.daeGetJointIndex(ref list, ref rtlObj);

		this.isDone = true;
	}
}