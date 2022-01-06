using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestScript : MonoBehaviour 
{
	// Use this for initialization
	void Start () 
	{
		StartCoroutine(vertexTest());
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	IEnumerator vertexTest()
	{
		int i = 0;
		List<Vector3> tempVec = new List<Vector3>();
		Mesh mesh = gameObject.GetComponent<MeshFilter>().mesh;
		tempVec.AddRange(mesh.vertices);

		while(i < 10)
		{
			yield return null;

			/*
			Vector3 pos = gameObject.GetComponent<MeshFilter>().mesh.vertices[i];

			GameObject tempGame = new GameObject();
			tempGame.name = i.ToString();

			Instantiate(tempGame, pos * gameObject.transform.lossyScale.x, Quaternion.identity);

			Destroy(tempGame);
			*/
			i++;
		}

		gameObject.GetComponent<MeshFilter>().mesh.vertices = tempVec.ToArray();
	}
}
