using System.Collections;

public class ModelInfo
{
	public string name;
	public string format;
	public string directory;
	public bool haveTex = false;

	public string getObjectFullName()
	{
		return directory + "/" + name + "." + format;
	}
	public string getTextureFullName()
	{
		return directory + "/" + name + ".jpg";
	}
}