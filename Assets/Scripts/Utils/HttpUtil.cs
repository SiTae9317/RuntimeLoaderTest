using System.Collections;

public static class HttpUtil
{
	public static readonly string lineEnd = "\r\n";
	public static readonly string twoHypens = "--";

	public static readonly string UTF_8 = "UTF-8";

	public static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
	
	public static long currentTimeMillis() 
	{
		return (long) (System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;	
	}
}
