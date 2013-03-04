
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;

namespace TestImageDownloader
{
	[Activity (Label = "WebViewActivity")]			
	public class WebViewActivity : Activity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.large_image_view);
			// Create your application here
			WebView fullImageView = (WebView)FindViewById( Resource.Id.webView);
			fullImageView.Settings.BuiltInZoomControls = true;
			fullImageView.Settings.LoadWithOverviewMode = true;
			fullImageView.Settings.UseWideViewPort = true;
			string url = Intent.GetStringExtra("URL");
			//fullImageView.LoadUrl(url); //this is for web url
			String html = "<html><head></head><body><img src=\""+ url + "\"></body></html>";
			fullImageView.LoadDataWithBaseURL("", html, "text/html","utf-8", "");

		}
	}
}

