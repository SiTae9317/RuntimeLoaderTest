using UnityEngine;
using System.Collections;

public class RigData
{
	public RigData()
	{
		boneNum = -1;
		name = null;
		localPosition = Vector3.zero;
		localRotation = Vector3.zero;
		localScale = Vector3.zero;
	}
	public Vector3 localPosition;
	public Vector3 localRotation;
	public Vector3 localScale;
	public string name;
	public string parent;
	public int boneNum;
}