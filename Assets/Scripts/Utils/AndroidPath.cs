using UnityEngine;
using System.Collections;

public class AndroidPath : MonoBehaviour
{
	private string filesDir;
	private string cacheDir;
	private string externalFilesDir;
	private string externalCacheDir;
	public static string filePath;
	
	// Use this for initialization
	void Awake ()
	{
		#if !UNITY_EDITOR && UNITY_ANDROID
		using( AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer") )
		{
			using( AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity") )
			{
				using( AndroidJavaObject filesDir = currentActivity.Call<AndroidJavaObject>( "getFilesDir" ) )
				{
					filesDir = filesDir.Call<string>( "getCanonicalPath" );
				}
				
				using( AndroidJavaObject cacheDir = currentActivity.Call<AndroidJavaObject>( "getCacheDir" ) )
				{
					cacheDir = cacheDir.Call<string>( "getCanonicalPath" );
				}
				
				using( AndroidJavaObject externalFilesDir = currentActivity.Call<AndroidJavaObject>("getExternalFilesDir",null ) )
				{
					filePath = externalFilesDir = externalFilesDir.Call<string>("getCanonicalPath");
				}
				
				using( AndroidJavaObject externalCacheDir = currentActivity.Call<AndroidJavaObject>("getExternalCacheDir" ) )
				{
					externalCacheDir = externalCacheDir.Call<string>("getCanonicalPath");
				}
			}
		}
		#endif
	}
}
