
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;

namespace TestImageDownloader
{
	class RestWebClient
	{
        static int Count = 0;
		public static Bitmap GetWebImageAsync(string url, string filename)
		{
            Count++;
            Android.Util.Log.Info( "GetImageAsyncCalled", Count.ToString() );
			Bitmap newBMimage = null;
			WebClient webClient = new WebClient();
			Uri uri = new Uri(url);
			HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(uri);
			request.Method = "GET";
			
			try
			{
                newBMimage = BitmapFactory.DecodeStream( request.GetResponse().GetResponseStream() );
			}
			catch (Exception e)
			{
				Android.Util.Log.Error("GetImageAsync",e.Message);
			}
			
			return newBMimage;
		}
	}
}

