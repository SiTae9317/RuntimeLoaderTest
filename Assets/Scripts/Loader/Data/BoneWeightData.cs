using System.Collections.Generic;
using System;

public class BoneWeightData : IDisposable
{
	public BoneWeightData()
	{
		indexs = new List<int>();
		weights = new List<float>();
	}

	public void Dispose()
	{
		removeData();
		GC.SuppressFinalize(this);
	}

	private void removeData()
	{
		indexs.RemoveRange(0, indexs.Count);
		indexs = null;

		weights.RemoveRange(0, weights.Count);
		weights = null;
	}

	public List<int> indexs;
	public List<float> weights;
}
