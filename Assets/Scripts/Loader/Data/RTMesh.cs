using UnityEngine;
using System.Collections.Generic;
using System;

public class RTMesh : IDisposable
{
	public RTMesh()
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();
		normals = new List<Vector3>();
		uvs = new List<Vector2>();
		materials = new List<int>();
		bws = new List<BoneWeight>();
	}

	public void Dispose()
	{
		removeDatas();
	}

	private void removeDatas()
	{
		if(vertices.Count > 0)
		{
			vertices.RemoveRange(0, vertices.Count);
			vertices.Clear();
		}
		vertices = null;

		if(triangles.Count > 0)
		{
			triangles.RemoveRange(0, triangles.Count);
			triangles.Clear();
		}
		triangles = null;

		if(normals.Count > 0)
		{
			normals.RemoveRange(0, normals.Count);
			normals.Clear();
		}
		normals = null;

		if(uvs.Count > 0)
		{
			uvs.RemoveRange(0, uvs.Count);
			uvs.Clear();
		}
		uvs = null;

		if(materials.Count > 0)
		{
			materials.RemoveRange(0, materials.Count);
			materials.Clear();
		}
		materials = null;

		if(bws.Count > 0)
		{
			bws.RemoveRange(0, bws.Count);
			bws.Clear();
		}
		bws = null;
	}

	public List<Vector3> vertices;
	public List<int> triangles;
	public List<Vector3> normals;
	public List<Vector2> uvs;
	public List<int> materials;
	public List<BoneWeight> bws;
}