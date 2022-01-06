using System.Text;

public static class DataConvertUtil
{
	public static byte[] convertStringToByteArray(string str)
	{
		return Encoding.UTF8.GetBytes(str);
	}
	
	public static string convertByteArrayToString(byte[] byteArray)
	{
		return Encoding.Default.GetString(byteArray);
	}
}
