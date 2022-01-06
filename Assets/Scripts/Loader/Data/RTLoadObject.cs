using UnityEngine;
using System.Collections.Generic;
using System;

public class RTLoadObject : IDisposable
{
	public RTLoadObject()
	{
		rtMesh = new List<RTMesh>();
		bindPose = new List<Matrix4x4>();
		skinBone = new List<Transform>();
		rds = new List<RigData>();
		skeletons = new List<GameObject>();

		vs = new List<Vector3>();
		ts = new List<int>();
		ns = new List<Vector3>();
		us = new List<Vector2>();
	}

	public void Dispose()
	{
		removeDatas();
	}

	private void removeDatas()
	{
		if((vs != null) && vs.Count > 0)
		{
			vs.RemoveRange(0, vs.Count);
			vs.Clear();
		}
		vs = null;

		if((ts != null) && ts.Count > 0)
		{
			ts.RemoveRange(0, ts.Count);
			ts.Clear();
		}
		ts = null;

		if((ns != null) && ns.Count > 0)
		{
			ns.RemoveRange(0, ns.Count);
			ns.Clear();
		}
		ns = null;

		if((us != null) && us.Count > 0)
		{
			us.RemoveRange(0, us.Count);
			us.Clear();
		}
		us = null;

		if((rtMesh != null) && rtMesh.Count > 0)
		{
			for(int i = 0; i < rtMesh.Count; i++)
			{
				rtMesh[i].Dispose();
				rtMesh[i] = null;
			}
			rtMesh.RemoveRange(0, rtMesh.Count);
			rtMesh.Clear();
		}
		rtMesh = null;

		if((bindPose != null) && bindPose.Count > 0)
		{
			bindPose.RemoveRange(0, bindPose.Count);
			bindPose.Clear();
		}
		bindPose = null;

		if((skinBone != null) && skinBone.Count > 0)
		{
			skinBone.RemoveRange(0, skinBone.Count);
			skinBone.Clear();
		}
		skinBone = null;

		if((rds != null) && rds.Count > 0)
		{
			rds.RemoveRange(0, rds.Count);
			rds.Clear();
		}
		rds = null;

		if((skeletons != null) && skeletons.Count > 0)
		{
			skeletons.RemoveRange(0, skeletons.Count);
			skeletons.Clear();
		}
		skeletons = null;

		if((bws != null) && bws.Length > 0)
		{
			for(int i = 0; i < bws.Length; i++)
			{
				bws[i].Dispose();
				bws[i] = null;
			}
		}
		bws = null;
	}

	public List<Vector3> vs;
	public List<int> ts;
	public List<Vector3> ns;
	public List<Vector2> us;

	public List<RTMesh> rtMesh;
	public List<Matrix4x4> bindPose;
	public List<Transform> skinBone;
	public List<RigData> rds;
	public List<GameObject> skeletons;
	public BoneWeightData[] bws;
}
