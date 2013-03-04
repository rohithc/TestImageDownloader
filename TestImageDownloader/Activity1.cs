using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Graphics;
using Android.Webkit;

namespace TestImageDownloader
{
	[Activity (Label = "TestImageDownloader", MainLauncher = true)]
	public class Activity1 : ListActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			List<Update> updates = new List<Update> ();
			//Create the picdirectory if it does not exist
			Java.IO.File directory = ImageDownloader.PicDirectory;
            //for (int i =0; i< 1; i++) 
            for( int i = 0; i < 10; i++ ) 
            {
                updates.Add( new Update( "http://www.android.com/images/about/about-nexus-family.png", "Image1" ) );
                updates.Add( new Update( string.Empty, "ImageEmpty" ) );
                updates.Add( new Update( "http://www.thedroidclub.com/wp-content/uploads/2012/09/ws_Flash_for_Android_1920x1200.jpg", "Image2" ) );
			}
			ListAdapter = new ImageAdapter(this,Resource.Layout.list_view,updates.ToArray());
		}

		protected override void OnListItemClick (Android.Widget.ListView l, View v, int position, long id)
		{
			Update update = (ListAdapter as ImageAdapter).Updates [position];
			if (!string.IsNullOrEmpty (update.Url)) 
			{
				Intent intent = new Android.Content.Intent (this, typeof(WebViewActivity));
				intent.PutExtra ("URL", ImageDownloader.GetUri(update.Url).ToString());
				StartActivity (intent);
			}
		}
	}

	public class Update
	{
		public string Url { get;set;}
		public string Text { get;set;}

		public Update (string url, string text)
		{
			Url = url;
			Text = text;
		}
	}

	public class ImageAdapter : ArrayAdapter
	{
		public List<Update> Updates = new List<Update>();
		LayoutInflater Inflater;
        BitmapWorker m_bitMapWorker;

		public ImageAdapter (Context context, int resource, Update[] updates )
			:base(context, resource, updates)
		{
			Updates = new List<Update>(updates);
			Inflater = LayoutInflater.FromContext(context);
            m_bitMapWorker = new BitmapWorker( context );
		}
		public override int Count {
			get {
				return Updates.Count;
			}
		}
		
		
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			if (convertView == null) 
				convertView = Inflater.Inflate(Resource.Layout.list_view,null);
			//set text
			TextView imageText = (TextView)convertView.FindViewById(Resource.Id.updateText);
			imageText.Text = Updates[position].Text;
            ImageView updateImage = (ImageView)convertView.FindViewById( Resource.Id.updateImage );
			//set image
            if( !string.IsNullOrEmpty( Updates[position].Url ) )
            {
                updateImage.Visibility = ViewStates.Visible;
                m_bitMapWorker.DownloadAndSetImageView( Updates[position].Url, updateImage, 150, 150 );
               //updateImage.SetImageBitmap( ImageDownloader.DownloadImage( Updates[position].Url, 150, 150 ) );
            }
            else
                updateImage.Visibility = ViewStates.Gone;
			return convertView;
		}
		
	}
}


