using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

public partial class RTLoader : IDisposable 
{
	public void exportData(ref RTLoadObject rtlObj, string path, string name)
	{
		FileStream fs = new FileStream(path + "/" + name + ".bin", FileMode.CreateNew, FileAccess.Write);

		using(BinaryWriter bw = new BinaryWriter(fs))
		{
			bw.Write(rtlObj.rds.Count);
			for(int i = 0; i < rtlObj.rds.Count; i++)
			{
				bw.Write(rtlObj.rds[i].boneNum);
				bw.Write(rtlObj.rds[i].name);
				bw.Write(rtlObj.rds[i].parent);
				bw.Write(rtlObj.rds[i].localPosition.x);
				bw.Write(rtlObj.rds[i].localPosition.y);
				bw.Write(rtlObj.rds[i].localPosition.z);
				bw.Write(rtlObj.rds[i].localRotation.x);
				bw.Write(rtlObj.rds[i].localRotation.y);
				bw.Write(rtlObj.rds[i].localRotation.z);
				bw.Write(rtlObj.rds[i].localScale.x);
				bw.Write(rtlObj.rds[i].localScale.y);
				bw.Write(rtlObj.rds[i].localScale.z);
			}

			bw.Write(rtlObj.bindPose.Count);

			for(int i = 0; i < rtlObj.bindPose.Count; i++)
			{
				bw.Write(rtlObj.bindPose[i].m00);
				bw.Write(rtlObj.bindPose[i].m01);
				bw.Write(rtlObj.bindPose[i].m02);
				bw.Write(rtlObj.bindPose[i].m03);
				bw.Write(rtlObj.bindPose[i].m10);
				bw.Write(rtlObj.bindPose[i].m11);
				bw.Write(rtlObj.bindPose[i].m12);
				bw.Write(rtlObj.bindPose[i].m13);
				bw.Write(rtlObj.bindPose[i].m20);
				bw.Write(rtlObj.bindPose[i].m21);
				bw.Write(rtlObj.bindPose[i].m22);
				bw.Write(rtlObj.bindPose[i].m23);
				bw.Write(rtlObj.bindPose[i].m30);
				bw.Write(rtlObj.bindPose[i].m31);
				bw.Write(rtlObj.bindPose[i].m32);
				bw.Write(rtlObj.bindPose[i].m33);
			}

			bw.Write(rtlObj.rtMesh.Count);

			for(int i = 0; i < rtlObj.rtMesh.Count; i++)
			{
				bw.Write(rtlObj.rtMesh[i].vertices.Count);

				for(int j = 0; j < rtlObj.rtMesh[i].vertices.Count; j++)
				{
					bw.Write(rtlObj.rtMesh[i].vertices[j].x);
					bw.Write(rtlObj.rtMesh[i].vertices[j].y);
					bw.Write(rtlObj.rtMesh[i].vertices[j].z);
				}

				bw.Write(rtlObj.rtMesh[i].triangles.Count);

				for(int j = 0; j < rtlObj.rtMesh[i].triangles.Count; j++)
				{
					bw.Write(rtlObj.rtMesh[i].triangles[j]);
				}

				bw.Write(rtlObj.rtMesh[i].normals.Count);

				for(int j = 0; j < rtlObj.rtMesh[i].normals.Count; j++)
				{
					bw.Write(rtlObj.rtMesh[i].normals[j].x);
					bw.Write(rtlObj.rtMesh[i].normals[j].y);
					bw.Write(rtlObj.rtMesh[i].normals[j].z);
				}

				bw.Write(rtlObj.rtMesh[i].uvs.Count);

				for(int j = 0; j < rtlObj.rtMesh[i].uvs.Count; j++)
				{
					bw.Write(rtlObj.rtMesh[i].uvs[j].x);
					bw.Write(rtlObj.rtMesh[i].uvs[j].y);
				}

				bw.Write(rtlObj.rtMesh[i].bws.Count);

				for(int j = 0; j < rtlObj.rtMesh[i].bws.Count; j++)
				{
					bw.Write(rtlObj.rtMesh[i].bws[j].boneIndex0);
					bw.Write(rtlObj.rtMesh[i].bws[j].boneIndex1);
					bw.Write(rtlObj.rtMesh[i].bws[j].boneIndex2);
					bw.Write(rtlObj.rtMesh[i].bws[j].boneIndex3);

					bw.Write(rtlObj.rtMesh[i].bws[j].weight0);
					bw.Write(rtlObj.rtMesh[i].bws[j].weight1);
					bw.Write(rtlObj.rtMesh[i].bws[j].weight2);
					bw.Write(rtlObj.rtMesh[i].bws[j].weight3);
				}
			}

			bw.Close();
		}
		fs.Close();
	}
}
