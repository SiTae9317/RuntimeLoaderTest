using UnityEngine;
using System.Collections;

public class RTTexture : MonoBehaviour
{
	private Texture2D useTexture = null;

	public void setTexture(Texture2D setText)
	{
		useTexture = setText;

		IEnumerator iEnum = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().GetEnumerator();

		while(iEnum.MoveNext())
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)iEnum.Current;
				
			if(useTexture != null)
			{
				smr.material = new Material(Shader.Find("Unlit/Texture"));
			}
			else
			{
				smr.material = new Material(Shader.Find("Mobile/Diffuse"));
			}
			smr.material.mainTexture = useTexture;
		}

		iEnum.Reset();
		iEnum = null;
	}

	public void OnDestroy()
	{

		IEnumerator iEnum = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>().GetEnumerator();

		while(iEnum.MoveNext())
		{
			SkinnedMeshRenderer smr = (SkinnedMeshRenderer)iEnum.Current;
			Destroy(smr.sharedMesh);
			smr.material.mainTexture = null;
			smr.material = null;
			Destroy(smr);
		}

		Destroy(useTexture);
		useTexture = null;
	}
}
