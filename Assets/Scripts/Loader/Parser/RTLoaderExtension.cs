using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class RTLoader : IDisposable 
{
	private void splitSkinnedMesh(ref RTLoadObject rtlObj)
	{				
		int splitValue = MaximumVertices;

		int verCount = rtlObj.vs.Count;
		int triCount = rtlObj.ts.Count;
		int norCount = rtlObj.ns.Count;
		int uvCount = rtlObj.us.Count;

		Debug.Log(verCount + " " + triCount + " " + norCount + " " + uvCount);

		int splitCount = 0;
		int remainTri = 0;

		if(triCount > 0)
		{
			splitCount = triCount / (splitValue * 3);

			remainTri = triCount % (splitValue * 3);

			if(remainTri > 0)
			{
				splitCount += 1;
			}
		}
		Debug.Log("split = " + splitCount + " remain = " + remainTri);

		int startIndex = 0;
		int endIndex = 0;

		for(int s = 0; s < splitCount; s++)
		{
			RTMesh newMesh = new RTMesh();
			Dictionary<int, List<int>> resortTriDic = new Dictionary<int, List<int>>();

			if(s == splitCount - 1)
			{
				endIndex = startIndex + remainTri;
			}
			else
			{
				endIndex = startIndex + splitValue * 3;
			}

			Debug.Log("start = " + startIndex + " end = " + endIndex);

			for(int i = startIndex; i < endIndex; i++)
			{
				if(!resortTriDic.ContainsKey(rtlObj.ts[i]))
				{
					List<int> triNums = new List<int>();
					triNums.Add(i);
					resortTriDic.Add(rtlObj.ts[i], triNums);
				}
				else
				{
					resortTriDic[rtlObj.ts[i]].Add(i);
				}
			}
			Debug.Log("sortTri Count = " + resortTriDic.Count);

			Dictionary<int, List<int>>.Enumerator tempEnumerator = resortTriDic.GetEnumerator();

			int newVerCount = 0;
			bool triEqualNor = false;
			if(rtlObj.ts.Count == rtlObj.ns.Count)
			{
				triEqualNor = true;
			}

			Debug.Log("T == N ? " + triEqualNor);

			while(tempEnumerator.MoveNext())
			{
				int verNum = tempEnumerator.Current.Key;
				newMesh.vertices.Add(rtlObj.vs[verNum]);
				if(triEqualNor)
				{
					newMesh.normals.Add(rtlObj.ns[tempEnumerator.Current.Value[0]]);
				}
				else
				{
					newMesh.normals.Add(rtlObj.ns[verNum]);
				}
				newMesh.uvs.Add(rtlObj.us[tempEnumerator.Current.Value[0]]);

				BoneWeight calcBoneWeight = new BoneWeight();

				for(int emptyIndex = rtlObj.bws[verNum].indexs.Count; emptyIndex < 4; emptyIndex++)
				{
					rtlObj.bws[verNum].indexs.Add(0);
				}

				for(int emptyIndex = rtlObj.bws[verNum].weights.Count; emptyIndex < 4; emptyIndex++)
				{
					rtlObj.bws[verNum].weights.Add(0);
				}

				calcBoneWeight.boneIndex0 = rtlObj.bws[verNum].indexs[0];
				calcBoneWeight.boneIndex1 = rtlObj.bws[verNum].indexs[1];
				calcBoneWeight.boneIndex2 = rtlObj.bws[verNum].indexs[2];
				calcBoneWeight.boneIndex3 = rtlObj.bws[verNum].indexs[3];

				calcBoneWeight.weight0 = rtlObj.bws[verNum].weights[0];
				calcBoneWeight.weight1 = rtlObj.bws[verNum].weights[1];
				calcBoneWeight.weight2 = rtlObj.bws[verNum].weights[2];
				calcBoneWeight.weight3 = rtlObj.bws[verNum].weights[3];

				newMesh.bws.Add(calcBoneWeight);

				for(int triCalNum = 0; triCalNum < tempEnumerator.Current.Value.Count; triCalNum++)
				{
					rtlObj.ts[tempEnumerator.Current.Value[triCalNum]] = newVerCount;
				}
				newVerCount++;
			}

			int MAX_VERTICES = newMesh.vertices.Count - 1;
			int MAX = MAX_VERTICES;
			int skip = 1;

			for (int i = startIndex; i < endIndex; i++) 
			{
				if(triEqualNor)
				{
					if (rtlObj.ns[i] != newMesh.normals[rtlObj.ts[i]] || rtlObj.us[i] != newMesh.uvs[rtlObj.ts[i]]) 
					{
						if (newMesh.vertices.Count > (skip * MAX_VERTICES)) 
							skip++;
						newMesh.vertices.Add(newMesh.vertices[rtlObj.ts[i]]);
						newMesh.bws.Add(newMesh.bws[rtlObj.ts[i]]);
						newMesh.normals.Add(rtlObj.ns[i]);
						newMesh.uvs.Add(rtlObj.us[i]);
						rtlObj.ns[i] = newMesh.normals[rtlObj.ts[i]];
						rtlObj.us[i] = newMesh.uvs[rtlObj.ts[i]];
						int inc = MAX + 1;
						rtlObj.ts[i] = inc;
						MAX = inc;
					}
				}
				else
				{
					if (rtlObj.us[i] != newMesh.uvs[rtlObj.ts[i]]) 
					{
						newMesh.vertices.Add(newMesh.vertices[rtlObj.ts[i]]);
						newMesh.bws.Add(newMesh.bws[rtlObj.ts[i]]);
						if (newMesh.normals.Count > 0) 
							newMesh.normals.Add(newMesh.normals[rtlObj.ts[i]]);
						newMesh.uvs.Add(rtlObj.us[i]);
						int inc = MAX + 1;
						rtlObj.ts[i] = inc;
						MAX = inc;
					}
				}
				newMesh.triangles.Add(rtlObj.ts[i]);
			}

			startIndex = endIndex;
			rtlObj.rtMesh.Add(newMesh);
		}

		this.progress = 0.9f;
		Debug.Log("Set MeshInfo");
	}

	public static int getIndex (ref List<string> list, string str_match) 
	{
		int f = -1;
		int c = 0;
		foreach (string s in list) 
		{
			if (s.Contains (str_match)) 
			{ 
				f = c; 
				break; 
			}
			c++;
		}
		return f;
	}

	public static string trimmed (string str_line) 
	{
		string s = "";
		for (int i = 0; i < str_line.Length; i++) 
		{
			if (str_line[i] == ' ') 
				continue;
			if (str_line[i] == ':') 
				continue;
			if (str_line[i] == '\"') 
				continue;
			if (str_line[i] == '{') 
				continue;
			if (str_line[i] == '}') 
				continue;
			if (str_line[i] == '*') 
				continue;
			s += str_line[i].ToString();
		}
		return s;
	}

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
}
