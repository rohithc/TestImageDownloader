
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
using Android.Graphics;
using System.Threading.Tasks;
using System.Threading;

namespace TestImageDownloader
{

    /// <summary>
    /// Bitmap worker. Class that handles downloading bitmap
    /// in background thread and updates imageview
    /// </summary>
    public class BitmapWorker
    {
        List<ImageViewTask> m_bitMapTasks;
        Context m_context;

        public BitmapWorker( Context context )
        {
            m_bitMapTasks = new List<ImageViewTask>();
            m_context = context;
        }
        /// <summary>
        /// Downloads the and set image view. Downloads the Bitmap in a seperate task
        /// and updates the imageview in UI thread
        /// </summary>
        public void DownloadAndSetImageView( string url, ImageView imageView, int width, int height )
        {
            //If the imageview is being recycled and used for another photo, cancel the previous task
            //as that is not needed in the view anymore
            ImageViewTask imageTask = m_bitMapTasks.Find( task => task.View == imageView );
            if( imageTask != null )
            {
                imageTask.CancelToken.Cancel();
                imageView.SetImageResource( Resource.Drawable.Icon );
                m_bitMapTasks.Remove( imageTask );
            }
            CancellationTokenSource cancelToken = new CancellationTokenSource();
            string url1 = url;
            ImageView imageView1 = imageView;
            Task<Bitmap> bitmapTask = Task.Factory.StartNew( () =>
            {
                return ImageDownloader.DownloadImage( url, width, height );
            }, cancelToken.Token );
            bitmapTask.ContinueWith( task => SetBitmap( url1, imageView1, task.Result, cancelToken.Token ) );
            imageTask = new ImageViewTask( url, imageView1, bitmapTask, cancelToken );
            m_bitMapTasks.Add( imageTask );
        }

        private void SetBitmap( string url, ImageView imageView, Bitmap bitmap, CancellationToken token )
        {
            if( !token.IsCancellationRequested  )
            {
                (this.m_context as Activity).RunOnUiThread( () =>
                                                            imageView.SetImageBitmap( bitmap ) );
            }
        }

        class ImageViewTask
        {
            public string Url { get; set; }
            public ImageView View { get; set; }
            public Task<Bitmap> Task { get; set; }
            public CancellationTokenSource CancelToken { get; set; }

            public ImageViewTask( string url, ImageView view, Task<Bitmap> task, CancellationTokenSource cancelToken )
            {
                Url = url;
                View = view;
                Task = task;
                CancelToken = cancelToken;
            }
        }
    }
	class ImageDownloader
	{
		public const string CAM_DIRECTORY = "TestImageDownloader";

		/// <summary>
		/// Gets the pic directory. Creates the directory if it does not already exist
		/// </summary>
		public static Java.IO.File PicDirectory
		{
			get
			{
				Java.IO.File dir = new Java.IO.File(
					Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures), CAM_DIRECTORY);
				if (!dir.Exists())
					dir.Mkdirs();
				return dir;
			}
		}

		/// <summary>
		/// Gets the URI.
		/// </summary>
		public static Android.Net.Uri GetUri (string photoUrl)
		{
			string filename = photoUrl.Substring (photoUrl.LastIndexOf ('/'));
			var systemFile = new Java.IO.File (string.Format ("{0}{1}{2}", ImageDownloader.PicDirectory.AbsolutePath, Java.IO.File.Separator, filename));
			return Android.Net.Uri.FromFile (systemFile);
		}
		/// <summary>
		/// Downloads the image if it does not exist in local storage.
		/// It downloads from web if its not there locally
		/// </summary>
		public static Bitmap DownloadImage(string url, int width, int height)
		{
			try
			{
				string filename = url.Substring(url.LastIndexOf('/'));
				var systemFile = new Java.IO.File(string.Format("{0}{1}{2}", PicDirectory.AbsolutePath, Java.IO.File.Separator, filename));
				Android.Net.Uri systemPath = Android.Net.Uri.FromFile(systemFile);

				if( !PhotoExists(filename) )
				{
					Bitmap bitMap = RestWebClient.GetWebImageAsync(url, "");
					System.IO.FileStream fs = new System.IO.FileStream(string.Format("{0}{1}{2}", PicDirectory.AbsolutePath, "/", filename), 
					                                                   System.IO.FileMode.OpenOrCreate); 
					bitMap.Compress(Android.Graphics.Bitmap.CompressFormat.Jpeg, 100, fs); 
					bitMap.Recycle();
				}
				return DecodeSampledBitmap(systemPath.Path,width,height);
			}
			catch (Exception ex)
			{
				ex.GetType();
				return null;
			}
		}
		/// <summary>
		/// Decodes the bitmap with sample size based on specified width and height.
		/// </summary>
		public static Bitmap DecodeSampledBitmap(string path, int reqWidth, int reqHeight) 
		{	
			// First decode with inJustDecodeBounds=true to check dimensions
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.InJustDecodeBounds = true;
			BitmapFactory.DecodeFile(path, options);
			
			// Calculate inSampleSize
			options.InSampleSize = GetSampleSize(options, reqWidth, reqHeight);
			
			// Decode bitmap with inSampleSize set
			options.InJustDecodeBounds = false;
			return BitmapFactory.DecodeFile(path, options);
		}
		/// <summary>
		/// Gets the sample size for  specified width and height
		/// </summary>
		public static int GetSampleSize (BitmapFactory.Options options, int viewWidth, int viewHeight)
		{
			// Raw height and width of image
			int height = options.OutHeight;
			int width = options.OutWidth;
			int inSampleSize = 1;
			
			if (height > viewHeight || width > viewWidth) {
				
				// Calculate ratios of height and width to requested height and width
				int heightRatio = (int)Math.Round((float) height / (float) viewHeight);
				int widthRatio = (int)Math.Round((float) width / (float) viewWidth);
				
				// Choose the smallest ratio as inSampleSize value, this will guarantee
				// a final image with both dimensions larger than or equal to the
				// requested height and width.
				inSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
			}
			
			return inSampleSize;
		}

		/// <summary>
		/// Finds if the photo with specified filename already exists.
		/// </summary>
		/// <returns>
		public static bool PhotoExists(string filename)
		{
			bool photoExists = System.IO.File.Exists(string.Format("{0}{1}{2}", PicDirectory.AbsolutePath, "/", filename));
			return photoExists;
		}
	}
}

