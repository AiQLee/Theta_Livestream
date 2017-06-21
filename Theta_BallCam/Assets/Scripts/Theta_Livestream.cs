/*
 @By Zhengqing Li
 @2016
*/


using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Text;

public class Theta_Livestream : MonoBehaviour 
{
	public Material material_preview;

	string url, jsonStr, sessionId;
	BinaryReader reader;
	List<byte> imageBytes;
	bool isLoadStart;
	byte byteData;
	int frame;
	Texture2D preview;
	//RawImage rawImage;

	// Use this for initialization
	void Start () {
		Debug.Log("Test begins!! =v=");

		//Request for the Theta status
		url = "http://192.168.1.1:80/osc/state";
		jsonStr = "{}";
		post_HTTP (url, jsonStr);

		//Request for the Theta ID
		url = "http://192.168.1.1:80/osc/commands/execute";
		jsonStr = "{\"name\": \"camera.startSession\"}";
		JsonNode json = JsonNode.Parse(post_HTTP (url, jsonStr));
		sessionId = json["results"]["sessionId"].Get<string>();
		Debug.Log("2 " + sessionId);

		jsonStr = "{\"name\": \"camera._getLivePreview\", \"parameters\": {\"sessionId\": \"" + sessionId + "\"}}";
		post_HTTP_livestream (url, jsonStr);

		imageBytes = new List<byte> ();
		isLoadStart = false; 


	}
	/*
	// Update is called once per frame
	void Update(){


	}

	*/
	void Update () {
	//	Debug.Log("Update");
		byteData = reader.ReadByte ();

		while( true ) { 
			
		//byte byteData = reader.ReadByte ();
		byte byteData_next = reader.ReadByte ();

		if (!isLoadStart) {
			if (byteData == 0xFF && byteData_next == 0xD8){
				// Keep loop until get the last byte of the MJPEG
				isLoadStart = true;
				imageBytes.Add(byteData);

			}
		} else {
			imageBytes.Add(byteData);
			// When read the last byte of the MJPEG
			if(byteData == 0xFF && byteData_next == 0xD9){

				//mainTexture.LoadImage ((byte[])imageBytes.ToArray ());
				imageBytes.Add(byteData_next);

				preview = new Texture2D(2, 2);
				preview.LoadImage ((byte[])imageBytes.ToArray ());
				//Thread.Sleep (1);

				imageBytes.Clear ();

				isLoadStart = false;
				break;
					
			}
		}

		byteData = byteData_next;

		}
	
		material_preview.mainTexture = preview;
		//rawImage.SetNativeSize();
	
	}


	string post_HTTP(String url, String data){
		// Create a request using a URL that can receive a post. 
		try 
		{
		Debug.Log("post_HTTP");
		WebRequest request = WebRequest.Create (url);
		// Set the Method property of the request to POST.
		request.Method = "POST";
		// Create POST data and convert it to a byte array.
		string postData = data;
		byte[] byteArray = Encoding.UTF8.GetBytes (postData);
		// Set the ContentType property of the WebRequest.
		request.ContentType = "application/json";
		// Set the ContentLength property of the WebRequest.
		request.ContentLength = byteArray.Length;
		// Get the request stream.
		Stream dataStream = request.GetRequestStream ();
		// Write the data to the request stream.
		dataStream.Write (byteArray, 0, byteArray.Length);
		// Close the Stream object.
		dataStream.Close ();
		// Get the response.

		WebResponse response = request.GetResponse ();
		// Display the status.

		Console.WriteLine (((HttpWebResponse)response).StatusDescription);

		// Get the stream containing content returned by the server.
		dataStream = response.GetResponseStream ();
		// Open the stream using a StreamReader for easy access.
		StreamReader reader = new StreamReader (dataStream);
		// Read the content.
		string responseFromServer = reader.ReadToEnd ();
		// Display the content.
		Debug.Log(responseFromServer);
		//Console.WriteLine (responseFromServer);
		// Clean up the streams.
		reader.Close ();
		dataStream.Close ();
		response.Close ();
		return responseFromServer;
		}catch (WebException webExcp) {
			// If you reach this point, an exception has been caught.
			Console.WriteLine("A WebException has been caught.");
			// Write out the WebException message.
			Console.WriteLine(webExcp.ToString());
			// Get the WebException status code.
			WebExceptionStatus status =  webExcp.Status;
			// If status is WebExceptionStatus.ProtocolError, 
			//   there has been a protocol error and a WebResponse 
			//   should exist. Display the protocol error.
			if (status == WebExceptionStatus.ProtocolError) {
				Console.Write("The server returned protocol error ");
				// Get HttpWebResponse so that you can check the HTTP status code.
				HttpWebResponse httpResponse = (HttpWebResponse)webExcp.Response;
				Console.WriteLine((int)httpResponse.StatusCode + " - "
					+ httpResponse.StatusCode);
			}
		}
		return null;
	}



	void post_HTTP_livestream(String url, String data){
		Debug.Log("post_HTTP_livestream");
		WebRequest request = WebRequest.Create (url);
		request.Method = "POST";
		request.Timeout = (int)(30 * 10000f); // タイムアウトしないようにする

		string postData = data;
		byte[] byteArray = Encoding.UTF8.GetBytes (postData);
		request.ContentType = "application/json";
		request.ContentLength = byteArray.Length;

		Stream dataStream = request.GetRequestStream ();
		dataStream.Write (byteArray, 0, byteArray.Length);
		dataStream.Close ();


		WebResponse response = request.GetResponse ();
		Console.WriteLine (((HttpWebResponse)response).StatusDescription);
		dataStream = response.GetResponseStream ();

		reader = new BinaryReader (new BufferedStream (dataStream), new System.Text.ASCIIEncoding ());
		Debug.Log("received_stream_data");

	}

}
