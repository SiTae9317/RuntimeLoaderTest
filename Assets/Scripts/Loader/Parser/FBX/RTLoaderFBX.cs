using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class RTLoader : IDisposable 
{
	private void getMeshes (List<string> list, ref RTLoadObject rtlObj) 
	{
		while (true) 
		{
			int f = getIndex(ref list, RTLoader.Vertices);
			if (f >= 0) 
			{
				for (int i = 1; i < 5; i++) 
				{
					if (list[f - i].Contains ("Geometry:")) 
					{
						break;
					}
				}

				using(RTMesh tempMesh = this.getMesh(list, f))
				{
					rtlObj.vs.AddRange(tempMesh.vertices);
					rtlObj.ts.AddRange(tempMesh.triangles);
					rtlObj.ns.AddRange(tempMesh.normals);
					rtlObj.us.AddRange(tempMesh.uvs);

					list.RemoveAt (f);

					//tempMesh.Dispose();
				}
			}
			else 
			{ 
				break; 
			}
		}

		this.progress = 0.3f;
		Debug.Log("GetMeshes");
	}

	private RTMesh getMesh (List<string> list, int n) 
	{
		RTMesh mesh = new RTMesh();

		//----------------------------------------------------------------------------------------
		// Vertices
		//----------------------------------------------------------------------------------------
		string[] data = list[n + 1].Split (',');
		data[0] = data[0].Split (':')[1];
		Debug.Log("ver = " + data.Length);

		for (int i = 0; i < data.Length; i = i + 3)
		{
			float x = float.Parse (data[i + 0]);
			float y = float.Parse (data[i + 1]);
			float z = float.Parse (data[i + 2]);
			mesh.vertices.Add (new Vector3 (-x, y, z) * this.scaleFactor);
		}

		Debug.Log("ver list = " + mesh.vertices.Count);

		if (mesh.vertices.Count > MaximumVertices) 
		{
			Debug.LogWarning("16K Over Vertices");
		}


		//----------------------------------------------------------------------------------------
		// Triangles
		//----------------------------------------------------------------------------------------
		int dnTriangle = 0;
		int dnNormal = 0;
		int dnUV = 0;
		int dnUVIndex = 0;
		int dnMaterial = 0;
		for (int i = 1; i < list.Count - n; i++) 
		{
			string s = list[n + i];
			if (s.Contains ("PolygonVertexIndex: *")) 
			{
				dnTriangle = i + 1;
			}
			else if (s.Contains ("Normals: *")) 
			{
				dnNormal = i + 1;
			}
			else if (s.Contains ("UV: *")) 
			{
				dnUV = i + 1;
			}
			else if (s.Contains ("UVIndex: *")) 
			{
				dnUVIndex = i + 1;
			}
			else if (s.Contains ("Materials: *")) 
			{
				dnMaterial = i + 1;
			}
			else if (s.Contains ("Layer:")) 
			{
				break;
			}
		}


		//----------------------------------------------------------------------------------------
		// Polygons or Quads only
		//----------------------------------------------------------------------------------------
		List<int> kfbxt = new List<int> ();
		List<int> quads = new List<int> ();
		List<int> del = new List<int> ();
		data = list[n + dnTriangle].Split (',');
		data[0] = data[0].Split (':')[1];
		int q = 0;
		Debug.Log("tri = " + data.Length);
		for (int i = 0; i < data.Length; i = i + 1) 
		{
			q++;
			int t = int.Parse (data[i]);
			mesh.triangles.Add (t);

			if (t < 0 && q == 3) 
			{
				q = 0;
				t = -t - 1;
				mesh.triangles[i] = t;
			}
			else if (t < 0 && q == 4) 
			{
				Debug.Log("t = " + t + " q = " + q);
				q = 0;
				t = -t - 1;
				mesh.triangles[i] = t;
				kfbxt.Add (mesh.triangles[i - 3]);
				kfbxt.Add (mesh.triangles[i - 2]);
				kfbxt.Add (mesh.triangles[i - 0]);
				quads.Add (i - 3);
				quads.Add (i - 2);
				quads.Add (i - 0);
				del.Add (i - 3);
			}
			else if (q > 4) 
			{
				Debug.Log("NGons not supported");
				return new RTMesh ();
			}
		}
		Debug.Log("tri list = " + mesh.triangles.Count);
		mesh.triangles.AddRange (kfbxt);
		int k = 0;
		for (int i = 0; i < del.Count; i++) 
			mesh.triangles.RemoveAt (del[i] - (k++));


		//----------------------------------------------------------------------------------------
		// Normals
		//----------------------------------------------------------------------------------------
		data = list[n + dnNormal].Split (',');
		data[0] = data[0].Split (':')[1];
		for (int i = 0; i < data.Length; i = i + 3) 
		{
			float x = float.Parse (data[i]);
			float y = float.Parse (data[i + 1]);
			float z = float.Parse (data[i + 2]);
			mesh.normals.Add (new Vector3 (-x, y, z));
		}
		bool isByVertice = list[n + dnNormal - 1].Contains ("ByVertice");
		if (del.Count > 0 && isByVertice == false) 
		{
			k = 0;
			for (int i = 0; i < quads.Count; i++) 
				mesh.normals.Add (mesh.normals[quads[i]]);
			for (int i = 0; i < del.Count; i++) 
				mesh.normals.RemoveAt (del[i] - (k++));
		}


		//----------------------------------------------------------------------------------------
		// UV by uvIndex
		//----------------------------------------------------------------------------------------
		data = list[n + dnUV].Split (',');
		data[0] = data[0].Split (':')[1];
		for (int i = 0; i < data.Length; i = i + 2) 
		{
			float x = float.Parse (data[i]);
			float y = float.Parse (data[i + 1]);
			mesh.uvs.Add (new Vector2 (x, y));
		}
		List<Vector2> uvTemp = new List<Vector2> ();
		data = list[n + dnUVIndex].Split (',');
		data[0] = data[0].Split (':')[1];
		for (int i = 0; i < data.Length; i++) 
		{
			uvTemp.Add (mesh.uvs[int.Parse (data[i])]);
		}
		if (del.Count > 0) 
		{
			k = 0;
			for (int i = 0; i < quads.Count; i++)
				uvTemp.Add (uvTemp[quads[i]]);
			for (int i = 0; i < del.Count; i++) 
				uvTemp.RemoveAt (del[i] - (k++));
		}
		mesh.uvs = uvTemp;


		//----------------------------------------------------------------------------------------
		// Materials
		//----------------------------------------------------------------------------------------
		data = list[n + dnMaterial].Split (',');
		data[0] = data[0].Split (':')[1];
		if (data.Length == 1) 
		{
			Debug.Log("mat data length = 1");
			int mtl = 0;
			int.TryParse (data[0], out mtl);
			for (int jj = 0; jj < mesh.triangles.Count / 3; jj++) 
				mesh.materials.Add (mtl);
		}
		else 
		{
			Debug.Log("mat data length > 1");
			for (int jj = 0; jj < data.Length; jj++) 
			{
				mesh.materials.Add (int.Parse (data[jj]));
			}
			if (del.Count > 0) 
			{
				int l = (mesh.materials.Count - del.Count) / 2;
				for (int jj = 0; jj < l; jj++) 
					mesh.materials.RemoveAt (mesh.materials.Count - 1);
				mesh.materials.AddRange (mesh.materials);
			}
		}
		if (mesh.materials.Count == 0) 
		{
			Debug.Log("mat count = 0");
			for (int i = 0; i < mesh.triangles.Count / 3; i++) 
				mesh.materials.Add (0);
		}

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

		return mesh;
	}

	private void getClusters (List<string> list, ref RTLoadObject rtlObj) 
	{
		int verticeCount = rtlObj.vs.Count;

		rtlObj.bws = new BoneWeightData[verticeCount];

		for(int i = 0; i < verticeCount; i++)
		{
			rtlObj.bws[i] = new BoneWeightData();
		}

		while (true) 
		{
			int f = getIndex(ref list, RTLoader.Subdeformers);

			if (f >= 0) 
			{
				List<int> boneIndexes = new List<int> ();

				for (int i = f; i < list.Count; i++) 
				{
					if (list[i].Contains ("Indexes:")) 
					{
						string s = "";

						for (int j = i + 1; j < list.Count; j++) 
						{
							if (list[j].Contains ("Weights: *")) 
							{
								break;
							}
							else 
							{
								s += list[j];
							}
						}

						s = s.Split (':')[1];
						string[] sumData = s.Split (new char[] { '\r', '\n' });

						s.Remove(0, s.Length);
						s = "";
						for(int sumI = 0; sumI < sumData.Length; sumI++)
						{
							s += sumData[sumI];
						}

						string[] data = s.Split (',');

						for (int ii = 0; ii < data.Length; ii++) 
						{
							int index = -1;
							int.TryParse (data[ii], out index);
							if (index != -1) 
							{
								boneIndexes.Add (index);
							}
						}
						s.Remove(0, s.Length);
					}

					if (list[i].Contains ("Weights:")) 
					{
						string s = "";
						for (int j = i + 1; j < list.Count; j++) 
						{
							if (list[j].Contains ("Transform: *")) 
							{
								break;
							}
							else 
							{
								s += list[j];
							}
						}

						s = s.Split (':')[1];
						string[] sumData = s.Split (new char[] { '\r', '\n' });

						s.Remove(0, s.Length);
						s = "";
						for(int sumI = 0; sumI < sumData.Length; sumI++)
						{
							s += sumData[sumI];
						}

						string[] data = s.Split (',');
						s.Remove(0, s.Length);

						for (int ii = 0; ii < data.Length; ii++) 
						{
							float weight = -1;
							float.TryParse (data[ii], out weight);
							if (weight != -1) 
							{
								int weightCount = rtlObj.bws[boneIndexes[ii]].weights.Count;
								int weightIndex = weightCount;
								float inputNum = weight;
								for(int iii = weightCount - 1; iii >= 0; iii--)
								{
									if(rtlObj.bws[boneIndexes[ii]].weights[iii].CompareTo(inputNum) < 0)
									{
										weightIndex = iii;
									}
									else
									{
										break;
									}
								}

								rtlObj.bws[boneIndexes[ii]].indexs.Insert(weightIndex, rtlObj.bindPose.Count);
								rtlObj.bws[boneIndexes[ii]].weights.Insert(weightIndex, weight);
							}
						}
					}

					if (list[i].Contains ("Transform:")) 
					{
						string s = "";
						for (int j = i + 1; j < list.Count; j++) 
						{
							if (list[j].Contains ("TransformLink: *")) 
							{
								break;
							}
							else 
							{
								s += list[j];
							}
						}

						s = s.Split (':')[1];

						string[] sumData = s.Split (new char[] { '\r', '\n' });

						s.Remove(0, s.Length);
						s = "";
						for(int sumI = 0; sumI < sumData.Length; sumI++)
						{
							s += sumData[sumI].Trim();
						}

						string[] data = s.Split (',');
						s.Remove(0, s.Length);

						if(data.Length == 16)
						{
							Matrix4x4 bindPoseMat = new Matrix4x4();
							List<float> matrixValue = new List<float>();

							for (int ii = 0; ii < data.Length; ii++) 
							{
								float bindPoseValue = -2;
								float.TryParse (data[ii], out bindPoseValue);
								if (bindPoseValue != -2) 
									matrixValue.Add (bindPoseValue);
							}

							bindPoseMat.m00 = matrixValue[0];
							bindPoseMat.m01 = -1 * matrixValue[4];
							bindPoseMat.m02 = -1 * matrixValue[8];
							bindPoseMat.m03 = this.scaleFactor * -1 * matrixValue[12];
							bindPoseMat.m10 = -1 * matrixValue[1];
							bindPoseMat.m11 = matrixValue[5];
							bindPoseMat.m12 = matrixValue[9];
							bindPoseMat.m13 = this.scaleFactor * matrixValue[13];
							bindPoseMat.m20 = -1 * matrixValue[2];
							bindPoseMat.m21 = matrixValue[6];
							bindPoseMat.m22 = matrixValue[10];
							bindPoseMat.m23 = this.scaleFactor * matrixValue[14];
							bindPoseMat.m30 = matrixValue[3];
							bindPoseMat.m31 = matrixValue[7];
							bindPoseMat.m32 = matrixValue[11];
							bindPoseMat.m33 = matrixValue[15];

							rtlObj.bindPose.Add(bindPoseMat);
						}
						break;
					}
				}
				list.RemoveAt (f);
			}
			else 
			{ 
				break; 
			}
		}
		this.progress = 0.5f;
	}

	private void getModels (List<string> list, ref RTLoadObject rtlObj) 
	{
		while (true) 
		{
			int f = getIndex (ref list, RTLoader.Models);

			if (f >= 0) 
			{
				string[] data = list[f].Split (new char[] { ':', ',' });
				data[4] = trimmed(data[4]);
				data[5] = trimmed(data[5]);

				Vector3 lclPosition = Vector3.zero;
				Vector3 lclRotation = Vector3.zero;
				Vector3 lclScaling = Vector3.one;

				string[] trsData = list[f+2].Split (new char[] { '\r', '\n' });

				for (int i = 1; i < trsData.Length - 1; i++) 
				{
					if (trsData[i].Contains ("Lcl Translation")) 
					{
						string[] s = trsData[i].Split (',');
						float x = float.Parse (s[s.Length - 3]);
						float y = float.Parse (s[s.Length - 2]);
						float z = float.Parse (s[s.Length - 1]);
						lclPosition = new Vector3 (-x, y, z);
					}
					else if (trsData[i].Contains ("Lcl Rotation")) 
					{
						string[] s = trsData[i].Split (',');
						float x = float.Parse (s[s.Length - 3]);
						float y = float.Parse (s[s.Length - 2]);
						float z = float.Parse (s[s.Length - 1]);
						lclRotation = new Vector3 (x, -y, -z);
					}
					else if (trsData[i].Contains ("Lcl Scaling")) 
					{
						string[] s = trsData[i].Split (',');
						float x = float.Parse (s[s.Length - 3]);
						float y = float.Parse (s[s.Length - 2]);
						float z = float.Parse (s[s.Length - 1]);
						lclScaling = new Vector3 (x, y, z);
					}
				}
				lclPosition *= this.scaleFactor;

				if(data[5].Equals("LimbNode"))
				{
					RigData rigData = new RigData();
					rigData.name = data[4];
					rigData.localPosition = lclPosition;
					rigData.localRotation = lclRotation;
					rigData.localScale = lclScaling;

					rtlObj.rds.Add(rigData);
				}

				list.RemoveAt (f);
			}
			else 
			{ 
				break; 
			}
		}

		this.progress = 0.7f;
		Debug.Log("GetModels");
	}
}
