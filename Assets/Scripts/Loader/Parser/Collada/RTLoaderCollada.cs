using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class RTLoader : IDisposable 
{
	private void daeGetMesh (ref List<string> list, ref RTLoadObject rtlObj) 
	{
		int verticeIndex = 0;
		int normalIndex = 0;
		int colorsIndex = 0;
		int uvIndex = 0;

		List<int> triangleIndex = new List<int>();

		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Contains("source id="))
			{
				if(list[i].Contains("positions"))
				{
					verticeIndex = i + 1;
				}
				else if(list[i].Contains("normal"))
				{
					normalIndex = i + 1;
				}
				else if(list[i].Contains("colors"))
				{
					colorsIndex = i + 1;
				}
				else if(list[i].Contains("map"))
				{
					uvIndex = i + 1;
				}
			}
			else if(list[i].Contains("triangles material"))
			{
				triangleIndex.Add(i + 4);
			}
		}

		using(RTMesh mesh = new RTMesh())
		{
			//----------------------------------------------------------------------------------------
			// Vertice
			//----------------------------------------------------------------------------------------

			string[] data = null;

			data = list[verticeIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

			for (int i = 0; i < data.Length; i = i + 3)
			{
				float x = float.Parse (data[i + 0]);
				float y = float.Parse (data[i + 1]);
				float z = float.Parse (data[i + 2]);
				mesh.vertices.Add (new Vector3 (-x, y, z) * this.scaleFactor);
			}

			progress = 0.2f;
			Debug.Log("ver Count = " + mesh.vertices.Count);

			if (mesh.vertices.Count > MaximumVertices) 
			{					
				Debug.LogWarning("16K Over Vertices");
			}

			//----------------------------------------------------------------------------------------
			// Triangle
			//----------------------------------------------------------------------------------------

			List<int> verticesPArray = new List<int>();
			List<int> norPArray = new List<int>();
			List<int> uvPArray = new List<int>();

			for(int triIndexCount = 0; triIndexCount < triangleIndex.Count; triIndexCount++)
			{
				data = list[triangleIndex[triIndexCount]].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

				for (int j = 0; j < data.Length; j = j + 3)
				{
					verticesPArray.Add(int.Parse (data[j + 0]));
					norPArray.Add(int.Parse (data[j + 1]));
					uvPArray.Add(int.Parse (data[j + 2]));
				}
			}
			mesh.triangles.AddRange(verticesPArray);

			progress = 0.3f;
			Debug.Log("tri Count = " + mesh.triangles.Count);

			//----------------------------------------------------------------------------------------
			// Normal
			//----------------------------------------------------------------------------------------

			data = list[normalIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

			List<Vector2> norTemp = new List<Vector2>();
			for (int i = 0; i < data.Length; i = i + 3)
			{
				float x = float.Parse (data[i + 0]);
				float y = float.Parse (data[i + 1]);
				float z = float.Parse (data[i + 2]);
				norTemp.Add (new Vector3 (-x, y, z));
			}

			for (int i = 0; i < norPArray.Count; i++) 
			{
				mesh.normals.Add (norTemp[norPArray[i]]);
			}

			progress = 0.4f;
			Debug.Log("normal Count = " + mesh.normals.Count);

			//----------------------------------------------------------------------------------------
			// Uv
			//----------------------------------------------------------------------------------------

			data = list[uvIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

			List<Vector2> uvTemp = new List<Vector2> ();

			for (int i = 0; i < data.Length; i = i + 2) 
			{
				float x = float.Parse (data[i]);
				float y = float.Parse (data[i + 1]);
				uvTemp.Add (new Vector2 (x, y));
			}

			for (int i = 0; i < uvPArray.Count; i++) 
			{
				mesh.uvs.Add (uvTemp[uvPArray[i]]);
			}

			progress = 0.5f;
			Debug.Log("uv Count = " + mesh.uvs.Count);


			//----------------------------------------------------------------------------------------
			// Swap
			//----------------------------------------------------------------------------------------
			for (int i = 0; i < mesh.triangles.Count; i = i + 3) 
			{
				int[] sw = new int[2];
				sw[0] = mesh.triangles[i + 0];
				sw[1] = mesh.triangles[i + 2];
				mesh.triangles[i + 2] = sw[0];
				mesh.triangles[i + 0] = sw[1];
			}
			if (mesh.normals.Count == mesh.triangles.Count) 
			{
				for (int i = 0; i < mesh.normals.Count; i = i + 3) 
				{
					Vector3[] sw = new Vector3[2];
					sw[0] = mesh.normals[i + 0];
					sw[1] = mesh.normals[i + 2];
					mesh.normals[i + 2] = sw[0];
					mesh.normals[i + 0] = sw[1];
				}
			}
			if (mesh.uvs.Count == mesh.triangles.Count) 
			{
				for (int i = 0; i < mesh.uvs.Count; i = i + 3) 
				{
					Vector2[] sw = new Vector2[2];
					sw[0] = mesh.uvs[i + 0];
					sw[1] = mesh.uvs[i + 2];
					mesh.uvs[i + 2] = sw[0];
					mesh.uvs[i + 0] = sw[1];
				}
			}

			rtlObj.vs.AddRange(mesh.vertices);
			rtlObj.ts.AddRange(mesh.triangles);
			rtlObj.ns.AddRange(mesh.normals);
			rtlObj.us.AddRange(mesh.uvs);

			//mesh.Dispose();
		}

		list.RemoveAt(verticeIndex);
		list.RemoveAt(normalIndex);
		list.RemoveAt(colorsIndex);
		list.RemoveAt(uvIndex);

		for(int i = 0; i < triangleIndex.Count; i++)
		{
			list.RemoveAt(triangleIndex[i]);
		}

		progress = 0.6f;
		Debug.Log("Get Mesh");
	}
		
	private void daeGetBindpose (ref List<string> list, ref RTLoadObject rtlObj) 
	{
		int bind_posesIndex = 0;

		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Contains("source id="))
			{
				if(list[i].Contains("bind_poses"))
				{
					bind_posesIndex = i + 1;
				}
			}
		}

		//----------------------------------------------------------------------------------------
		// BoneBindPose
		//----------------------------------------------------------------------------------------

		string[] data = null;

		data = list[bind_posesIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

		if((data.Length / 16) > 0 && (data.Length % 16) == 0)
		{
			for (int i = 0; i < data.Length; i = i + 16)
			{
				Matrix4x4 bindPoseMat = new Matrix4x4();
				List<float> matrixValue = new List<float>();

				for (int j = i; j < i + 16; j++) 
				{
					float bindPoseValue = -2;
					float.TryParse (data[j], out bindPoseValue);
					if (bindPoseValue != -2) 
						matrixValue.Add (bindPoseValue);
				}

				bindPoseMat.m00 = matrixValue[0];
				bindPoseMat.m01 = -1 * matrixValue[1];
				bindPoseMat.m02 = -1 * matrixValue[2];
				bindPoseMat.m03 = this.scaleFactor * -1 * matrixValue[3];
				bindPoseMat.m10 = -1 * matrixValue[4];
				bindPoseMat.m11 = matrixValue[5];
				bindPoseMat.m12 = matrixValue[6];
				bindPoseMat.m13 = this.scaleFactor * matrixValue[7];
				bindPoseMat.m20 = -1 * matrixValue[8];
				bindPoseMat.m21 = matrixValue[9];
				bindPoseMat.m22 = matrixValue[10];
				bindPoseMat.m23 = this.scaleFactor * matrixValue[11];
				bindPoseMat.m30 = matrixValue[12];
				bindPoseMat.m31 = matrixValue[13];
				bindPoseMat.m32 = matrixValue[14];
				bindPoseMat.m33 = matrixValue[15];

				rtlObj.bindPose.Add(bindPoseMat);
			}
		}

		list.RemoveAt(bind_posesIndex);

		progress = 0.7f;
		Debug.Log("Get BindPose");
	}

	private void daeGetBoneWeight (ref List<string> list, ref RTLoadObject rtlObj) 
	{
		int weightsIndex = 0;
		int vertexWeightsIndex = 0;

		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Contains("source id="))
			{
				if(list[i].Contains("weights"))
				{
					weightsIndex = i + 1;
				}
			}
			else if(list[i].Contains("vertex_weights count"))
			{
				vertexWeightsIndex = i + 3;
			}
		}

		//----------------------------------------------------------------------------------------
		// BoneWeight
		//----------------------------------------------------------------------------------------

		string[] data = null;

		data = list[weightsIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');
		string[] indexData = list[vertexWeightsIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');
		string[] boneIndexData = list[vertexWeightsIndex + 1].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

		rtlObj.bws = new BoneWeightData[rtlObj.vs.Count];

		for(int i = 0; i < indexData.Length; i++)
		{
			rtlObj.bws[i] = new BoneWeightData();

			int indexCount = int.Parse(indexData[i]);

			for(int j = 0; j < indexCount; j++)
			{
				rtlObj.bws[i].indexs.Add(int.Parse(boneIndexData[i * (indexCount * 2)  + (j * 2)]));
				rtlObj.bws[i].weights.Add(float.Parse(data[i * indexCount + j]));
			}
		}
			
		list.RemoveAt(weightsIndex);
		list.RemoveAt(vertexWeightsIndex);

		progress = 0.8f;
		Debug.Log("Get BoneWeight");
	}

	private void daeGetJointIndex (ref List<string> list, ref RTLoadObject rtlObj) 
	{
		int jointsIndex = 0;
		int skeletonIndex = 0;

		for(int i = 0; i < list.Count; i++)
		{
			if(list[i].Contains("source id="))
			{
				if(list[i].Contains("joints"))
				{
					jointsIndex = i + 1;
				}
			}
			else if(list[i].Contains("visual_scene id="))
			{
				skeletonIndex = i;
			}
		}

		//----------------------------------------------------------------------------------------
		// JointIndex
		//----------------------------------------------------------------------------------------

		string[] data = null;

		Dictionary<string, int> jointIndexData = new Dictionary<string, int>();

		data = list[jointsIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

		for(int i = 0; i < data.Length; i++)
		{
			jointIndexData.Add(data[i], i);
		}

		progress = 0.95f;
		Debug.Log("Set Joint Index");

		//----------------------------------------------------------------------------------------
		// Skeleton
		//----------------------------------------------------------------------------------------

		//jointsIndex
		List<string> parentName = new List<string>();
		parentName.Add("RootNode");

		while(!list[skeletonIndex].Contains("/visual_scene"))
		{
			skeletonIndex++;
			if(list[skeletonIndex].Equals(""))
			{
				continue ;
			}
			else
			{
				if(list[skeletonIndex].Contains("node id="))
				{
					RigData rigData = new RigData();
					rigData.localScale = Vector3.one;

					data = list[skeletonIndex].Split(new char[2] { '<', '>' })[1].Trim().Split(' ')[2].Trim().Split('=')[1].Split('\"');

					rigData.name = data[1].Trim();

					rigData.parent = parentName[parentName.Count - 1];
					parentName.Add(rigData.name);

					if(jointIndexData.ContainsKey(rigData.name))
					{
						rigData.boneNum = jointIndexData[rigData.name];
					}

					skeletonIndex++;

					if(list[skeletonIndex].Contains("translate sid="))
					{
						data = list[skeletonIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ');

						float x = float.Parse (data[0]);
						float y = float.Parse (data[1]);
						float z = float.Parse (data[2]);
						rigData.localPosition = new Vector3 (-x, y, z) * this.scaleFactor;
					}
					else
					{
						skeletonIndex--;
						continue ;
					}

					skeletonIndex++;

					if(list[skeletonIndex].Contains("rotate sid="))
					{
						float rotY = float.Parse(list[skeletonIndex].Split(new char[2] { '<', '>' })[2].Trim().Split(' ')[3]);
						float rotX = float.Parse(list[skeletonIndex + 1].Split(new char[2] { '<', '>' })[2].Trim().Split(' ')[3]);
						float rotZ = float.Parse(list[skeletonIndex + 2].Split(new char[2] { '<', '>' })[2].Trim().Split(' ')[3]);

						rigData.localRotation = new Vector3 (rotX, -rotY, -rotZ);
						skeletonIndex += 2;
					}
					else
					{
						skeletonIndex--;
						continue ;
					}

					rtlObj.rds.Add(rigData);
				}
				else if(list[skeletonIndex].Contains("/node"))
				{
					parentName.RemoveAt(parentName.Count - 1);
				}
			}
		}
			
		list.RemoveAt(jointsIndex);
		list.RemoveAt(skeletonIndex);

		progress = 1.0f;
		Debug.Log("Get Skeleton");
	}
}
