using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(RTLoadImport))]
public class GenericToHumanoid : MonoBehaviour
{
	[HideInInspector]
	public GameObject tPoseRootNode;

	private GameObject genericObj;
	private GameObject rootNode;
	
	private List<HumanBone> humanBones;
	public List<SkeletonBone> skeletonBones;

	private GameObject jtUpperArmTwistARt;
	private GameObject jtUpperArmTwistALf;
	private GameObject jtWristRt;
	private GameObject jtWristLf;
	private GameObject neckA;
	private GameObject neckB;

	private GameObject jtKneeRt;
	private GameObject jtKneeLf;
	private GameObject jtToeRt;
	private GameObject jtToeLf;

	void resetData()
	{
		humanBones.RemoveRange(0, humanBones.Count);
		humanBones.Clear();
		humanBones = null;

		skeletonBones.RemoveRange(0, skeletonBones.Count);
		skeletonBones.Clear();
		skeletonBones = null;
	}

	void addSkeletonFromSearchNode(GameObject parentObj)
	{
		for(int i = 0; i < parentObj.transform.childCount; i++)
		{
			addSkeleton(parentObj.transform.GetChild(i).gameObject);
		}
	}

	void addSkeletonFromSearchNodeName(GameObject parentObj, string nodeName)
	{
		if(nodeName.Equals(parentObj.name))
		{
			addSkeleton(parentObj);
			return ;
		}
		else
		{
			for(int i = 0; i < parentObj.transform.childCount; i++)
			{
				addSkeletonFromSearchNodeName(parentObj.transform.GetChild(i).gameObject, nodeName);
			}
		}
	}
	
	void matchingTRSFromNode(GameObject baseObj, GameObject TPoseObj)
	{
		Transform[] basePoseData = baseObj.GetComponentsInChildren<Transform>();
		Transform[] TPoseData = TPoseObj.GetComponentsInChildren<Transform>();

		baseObj.transform.localRotation = TPoseObj.transform.localRotation;

		for(int i = 0; i < basePoseData.Length; i++)
		{
			for(int j = 0; j < TPoseData.Length; j++)
			{
				if(basePoseData[i].name.Equals(TPoseData[j].name))
				{
					//basePoseData[i].position = TPoseData[j].position;
					basePoseData[i].localRotation = TPoseData[j].localRotation;
					//basePoseData[i].localScale = TPoseData[j].localScale;
					break;
				}
			}
		}
	}
	
	void createAvatar() 
	{
		HumanDescription desc = setupHumanDescription ();
		genericObj.GetComponent<Animator>().avatar = AvatarBuilder.BuildHumanAvatar(genericObj, desc);
		genericObj.GetComponent<Animator>().avatar.name = "TestAvatar";
		Debug.Log(genericObj.GetComponent<Animator>().isHuman);
	}
	
	void humanBoneSetting()
	{
		Dictionary<string, string> boneName = new System.Collections.Generic.Dictionary<string, string>();
				
		//boneName["Root"] = "Generic";
		
		boneName["Hips"] = "JtRoot";
		boneName["Spine"] = "JtSpineA";
		boneName["Chest"] = "JtSpineB";
		
		boneName["Neck"] = "JtNeckA";
		boneName["Head"] = "JtSkullA";
		boneName["LeftEye"] = "JtEyeLf";
		boneName["RightEye"] = "JtEyeRt";
		boneName["Jaw"] = "JtJaw";
		
		boneName["LeftShoulder"] = "JtClavicleLf";
		boneName["LeftUpperArm"] = "JtShoulderLf";
		boneName["LeftLowerArm"] = "JtElbowLf";
		boneName["LeftHand"] = "JtWristLf";
		
		boneName["RightShoulder"] = "JtClavicleRt";
		boneName["RightUpperArm"] = "JtShoulderRt";
		boneName["RightLowerArm"] = "JtElbowRt";
		boneName["RightHand"] = "JtWristRt";
		
		boneName["LeftUpperLeg"] = "JtHipLf";
		boneName["LeftLowerLeg"] = "JtKneeLf";
		boneName["LeftFoot"] = "JtAnkleLf";
		boneName["LeftToes"] = "JtBallLf";
		
		boneName["RightUpperLeg"] = "JtHipRt";
		boneName["RightLowerLeg"] = "JtKneeRt";
		boneName["RightFoot"] = "JtAnkleRt";
		boneName["RightToes"] = "JtBallRt";
		
		string[] humanName = HumanTrait.BoneName;
		
		for(int i = 0; i < humanName.Length; i++)
		{
			HumanBone humanBone = new HumanBone();
			humanBone.boneName = null;
			humanBone.humanName = humanName[i];
			humanBone.limit = new HumanLimit();
			humanBone.limit.useDefaultValues = true;
			
			if(boneName.ContainsKey(humanName[i]))
			{
				humanBone.boneName = boneName[humanName[i]];
				humanBone.limit.useDefaultValues = true;
				humanBones.Add(humanBone);
				boneName.Remove(humanName[i]);
			}
		}

		boneName.Clear();
		boneName = null;
	}

	void addSkeleton(GameObject PoseObj)
	{
		SkeletonBone tempSkelBone = new SkeletonBone();
		tempSkelBone.name = PoseObj.transform.name;
		tempSkelBone.position = PoseObj.transform.localPosition;
		tempSkelBone.rotation = PoseObj.transform.localRotation;
		tempSkelBone.scale = PoseObj.transform.localScale;
		
		skeletonBones.Add(tempSkelBone);
	}

	void skeletonBoneSetting ()
	{	
		skeletonBones = new List<SkeletonBone>();

		for(int i = 0; i < humanBones.Count; i++)
		{
			addSkeletonFromSearchNodeName(rootNode, humanBones[i].boneName);
		}

		addSkeletonFromSearchNodeName(rootNode, "JtPelvis");
		addSkeletonFromSearchNodeName(rootNode, "JtSpineC");
		addSkeletonFromSearchNodeName(rootNode, "JtUpperArmTwistBLf");
		addSkeletonFromSearchNodeName(rootNode, "JtForearmTwistBLf");
		addSkeletonFromSearchNodeName(rootNode, "JtUpperArmTwistBRt");
		addSkeletonFromSearchNodeName(rootNode, "JtForearmTwistBRt");
		addSkeletonFromSearchNodeName(rootNode, "JtNeckB");
		addSkeletonFromSearchNodeName(rootNode, "JtLowerFaceParent");
		addSkeletonFromSearchNodeName(rootNode, "JtUpperFaceParent");
		
		addSkeleton(genericObj);
	}
	
	private HumanDescription setupHumanDescription() 
	{
		humanBones = new List<HumanBone>();
		humanBoneSetting();
		skeletonBoneSetting();
		
		HumanDescription desc = new HumanDescription ();
		
		desc.human = humanBones.ToArray();
		Debug.Log(skeletonBones.Count);
		desc.skeleton = skeletonBones.ToArray();
		desc.upperArmTwist = 0.5f;
		desc.lowerArmTwist = 0.5f;
		desc.upperLegTwist = 0.5f;
		desc.lowerLegTwist = 0.5f;
		desc.armStretch = 0.05f;
		desc.legStretch = 0.05f;
		desc.feetSpacing = 0.0f;
		
		return desc;
	}

	private void tPoseGenerate()
	{
		Transform[] basePoseData = rootNode.GetComponentsInChildren<Transform>();
		
		for(int i = 0; i < basePoseData.Length; i++)
		{
			if(basePoseData[i].name.Equals("JtUpperArmTwistALf"))
			{
				jtUpperArmTwistALf = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtUpperArmTwistARt"))
			{
				jtUpperArmTwistARt = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtNeckA"))
			{
				neckA = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtNeckB"))
			{
				neckB = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtWristLf"))
			{
				jtWristLf = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtWristRt"))
			{
				jtWristRt = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtKneeLf"))
			{
				jtKneeLf = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtKneeRt"))
			{
				jtKneeRt = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtToeLf"))
			{
				jtToeLf = basePoseData[i].parent.gameObject;
			}
			else if(basePoseData[i].name.Equals("JtToeRt"))
			{
				jtToeRt = basePoseData[i].parent.gameObject;
			}
		}

		float neckYpos = (neckA.transform.position.y + neckB.transform.position.y) / 2.0f;
		float neckZpos = (neckA.transform.position.z + neckB.transform.position.z) / 2.0f;

		bool yValue = false;
		bool zValue = false;

		while(!yValue || !zValue)
		{
			if(neckYpos - jtWristLf.transform.position.y > 0)
			{
				jtUpperArmTwistALf.transform.localEulerAngles += Vector3.down;
			}
			else
			{
				yValue = true;
			}

			if(neckZpos - jtWristLf.transform.position.z > 0)
			{
				jtUpperArmTwistALf.transform.localEulerAngles += Vector3.left;// * 9;
				jtUpperArmTwistALf.transform.localEulerAngles += Vector3.back * 3;
			}
			else
			{
				zValue = true;
			}
		}

		yValue = false;
		zValue = false;

		while(!yValue || !zValue)
		{
			if(neckYpos - jtWristRt.transform.position.y > 0)
			{
				jtUpperArmTwistARt.transform.localEulerAngles += Vector3.down;
			}
			else
			{
				yValue = true;
			}

			if(neckZpos - jtWristRt.transform.position.z > 0)
			{
				jtUpperArmTwistARt.transform.localEulerAngles += Vector3.left;// * 9;
				jtUpperArmTwistARt.transform.localEulerAngles += Vector3.back * 3;
			}
			else
			{
				zValue = true;
			}
		}

		yValue = false;
		zValue = false;

		float delta = Time.deltaTime;

		while(!yValue || !zValue)
		{

			if(jtUpperArmTwistALf.transform.position.x - jtToeLf.transform.position.x > 0.02f)
			{
				jtKneeLf.transform.localEulerAngles += Vector3.left * 0.016f * delta;
				jtKneeLf.transform.localEulerAngles += Vector3.forward * 0.008f * delta;
			}
			else
			{
				yValue = true;
			}

			if(jtToeRt.transform.position.x - jtUpperArmTwistARt.transform.position.x > 0.02f)
			{
				jtKneeRt.transform.localEulerAngles += Vector3.right * 0.016f * delta;
				jtKneeRt.transform.localEulerAngles += Vector3.forward * 0.004f * delta;
			}
			else
			{
				zValue = true;
			}
		}
	}

	public void humanAni()
	{
		matchingTRSFromNode(rootNode, tPoseRootNode);
		tPoseGenerate();
		createAvatar();
		resetData();
	}

	public void setRootNode(GameObject rootNode)
	{
		this.rootNode = rootNode;
	}

	public void setGenericObject(GameObject genericObj)
	{
		this.genericObj = genericObj;
	}
}
