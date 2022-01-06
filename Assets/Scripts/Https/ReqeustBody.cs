using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

public class RequestBody : IDisposable
{
	string charSet;
	string boundary;
	string twoHypens = "--";
	string lineEnd = "\r\n";
	List<byte> body;

	public RequestBody(string charset = "UTF-8")
	{
		body = new List<byte>();
		charSet = charset;
		boundary = "*****" + HttpUtil.currentTimeMillis() + "*****";
	}
	
	~RequestBody()
	{
		Dispose();
	}

	public void Dispose()
	{
		body.RemoveRange(0, body.Count);
		body = null;
	}
	
	public void addHeaderFiled(string name, string value)
	{
		string bodyData = name + ": " + value + lineEnd;
		
		body.AddRange(DataConvertUtil.convertStringToByteArray(bodyData));
	}
	
	public void addFormField(string name, string value)
	{
		string bodyData = "";
		bodyData += twoHypens + boundary + lineEnd;
		bodyData += "Content-Disposition: form-data; name=\"" + name + "\"" + lineEnd;
		bodyData += "Content-Type: text/plain; charset=" + charSet + lineEnd;
		bodyData += lineEnd;
		bodyData += value + lineEnd;
		
		body.AddRange(DataConvertUtil.convertStringToByteArray(bodyData));
	}
	
	public void addFromFile(string path)
	{
		string[] filename = path.Split('/');

		string bodyData = "";
		bodyData += twoHypens + boundary + lineEnd;
		bodyData += "Content-Disposition: form-data; name=\"file[]\";filename=\"" + filename[filename.Length -1].Trim() + "\"" + lineEnd;
		bodyData += "Content-Type: null" + lineEnd;
		bodyData += lineEnd;

		body.AddRange(DataConvertUtil.convertStringToByteArray(bodyData));

		//FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		//byte[] readByte = new byte[fs.Length];
		//fs.Read(readByte, 0, (int)fs.Length);
		
		body.AddRange(File.ReadAllBytes(path));

		bodyData = lineEnd;
		bodyData += twoHypens + boundary + twoHypens + lineEnd;

		body.AddRange(DataConvertUtil.convertStringToByteArray(bodyData));
		//fs.Close();
	}
	
	public string getBodyToString()
	{
		return DataConvertUtil.convertByteArrayToString(body.ToArray());
	}
	
	public byte[] getBodyToBytes()
	{
		return body.ToArray();
	}
	
	public string getBoundary()
	{
		return boundary;
	}
}