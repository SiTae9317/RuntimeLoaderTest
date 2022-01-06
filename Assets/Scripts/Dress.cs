using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dress : MonoBehaviour 
{
	private List<string> keepBone = null;
	private string keepRootBone = null;

	void OnEnable()
	{
		if(keepBone == null)
		{
			keepBone = new List<string>();

			for(int i = 0; i < gameObject.GetComponent<SkinnedMeshRenderer>().bones.Length; i++)
			{
				keepBone.Add(gameObject.GetComponent<SkinnedMeshRenderer>().bones[i].name);
			}
		}
		if(keepRootBone == null)
		{
			keepRootBone = gameObject.GetComponent<SkinnedMeshRenderer>().rootBone.name;
		}
	}

	public void setTracking(GameObject trackingObj)
	{
		foreach(Transform go in trackingObj.GetComponentsInChildren<Transform>())
		{
			if(keepRootBone.Equals(go.name))
			{
				gameObject.GetComponent<SkinnedMeshRenderer>().rootBone = go;
				break;
			}
		}

		List<Transform> newBones = new List<Transform>();

		for(int i = 0; i < keepBone.Count; i++)
		{
			foreach(Transform go in trackingObj.GetComponentsInChildren<Transform>())
			{
				if(keepBone[i].Equals(go.name))
				{
					newBones.Add(go);
					break;
				}
			}
		}

		gameObject.GetComponent<SkinnedMeshRenderer>().bones = newBones.ToArray();
	}
}
