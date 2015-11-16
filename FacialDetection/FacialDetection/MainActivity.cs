using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Android.Graphics;
using Android.Media;
using System.IO;

namespace FacialDetection
{
	[Activity (Label = "FacialDetection", MainLauncher = true, Icon = "@drawable/icon", ScreenOrientation=Android.Content.PM.ScreenOrientation.Portrait)]
	public class MainActivity : Activity
	{
		//Intent code for camera activity
		private static int TAKE_PICTURE_CODE = 100;

		//Max Faces to detect in a picture
		private static int MAX_FACES = 5;

		//Bitmap of a picture taken
		private Bitmap cameraBitmap = null;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			// Set our view from the "main" layout resource
			SetContentView (Resource.Layout.Main);

			Button take_picture = FindViewById<Button> (Resource.Id.take_picture);

			take_picture.Click	+= take_picture_Clicked;

		}

		void take_picture_Clicked (object sender, EventArgs args )
		{
			//call OpenCamera() Event
			openCamera();
		}



		//OnActivityResult
		protected override void OnActivityResult (int requestCode, Result resultCode, Intent data)
		{
			base.OnActivityResult (requestCode, resultCode, data);
			//check if method return takepicturecode
			if(TAKE_PICTURE_CODE == requestCode){
				//Call a method to process image data
				processCameraImage(data);
			}
		}


		/// <summary>
		/// Open an activity for take a picture
		/// </summary>
		private void openCamera()
		{
			Intent intent = new Intent (Android.Provider.MediaStore.ActionImageCapture);
			StartActivityForResult (intent, TAKE_PICTURE_CODE);
		}

		/// <summary>
		/// Process picture taken an change UI to detect face
		/// </summary>
		/// <param name="intent">Intent.</param>
		private void processCameraImage(Intent intent)
		{
			//Change layout to main Layout
			SetContentView(Resource.Layout.detectlayout);

			Button detect_face = FindViewById<Button> (Resource.Id.detect_face);

			detect_face.Click += detect_face_Clicked;

			ImageView image_view = FindViewById<ImageView> (Resource.Id.image_view);

			//Set image 
			image_view.SetImageBitmap(cameraBitmap);
		}

		void detect_face_Clicked (object sender, EventArgs args )
		{
			//Detect Face
			detectFaces();
		}

		/// <summary>
		/// detect faces on a picture and draw a square in each face
		/// </summary<
		private void detectFaces(){
			//first check if picture has been taken
			if(null != cameraBitmap){
				//get width of a picture
				int width = cameraBitmap.Width;
				//get height of a picture
				int height = cameraBitmap.Height;
				//Initialize a facedetector with the picture dimensions and the max number of faces to check
				FaceDetector detector = new FaceDetector(width, height, MainActivity.MAX_FACES);
				//Create an array of faces with the number of max faces to check
				Android.Media.FaceDetector.Face[] faces = new Android.Media.FaceDetector.Face[MainActivity.MAX_FACES];

				//create a main bitmap
				Bitmap bitmap565 = Bitmap.CreateBitmap(width, height, Bitmap.Config.Rgb565);
				//create a dither paint
				Paint ditherPaint = new Paint();
				//create a draw paint
				Paint drawPaint = new Paint();

				//set true dither to dither paint variable
				ditherPaint.Dither = true;
				//set color red for the square
				drawPaint.Color = Color.Green;
				//set stroke to style
				drawPaint.SetStyle(Paint.Style.Stroke);
				//set stroke width
				drawPaint.StrokeWidth = 2;

				//Create a canvas
				Canvas canvas = new Canvas();
				//set bitmap to canvas
				canvas.SetBitmap(bitmap565);
				//draw bitmap to canvas
				canvas.DrawBitmap(cameraBitmap, 0, 0, ditherPaint);

				//get a number of faces detected
				int facesFound = detector.FindFaces(bitmap565, faces);
				//mid face point
				PointF midPoint = new PointF();
				//eye distance variable
				float eyeDistance = 0.0f;
				//confidence variable
				float confidence = 0.0f;
				//print numbre of faces found
				System.Console.WriteLine ("Number of faces found: " + facesFound);

				//check if found at least one face
				if(facesFound > 0)
				{
					//for each face draw a red squeare
					for(int index=0; index<facesFound; ++index){
						// get midpoint of a face
						faces[index].GetMidPoint(midPoint);
						//get eye distance
						eyeDistance = faces[index].EyesDistance();
						//get confidence
						confidence = faces [index].Confidence ();
						//print all parameters
						System.Console.WriteLine ("Confidence: " + confidence + 
							", Eye distance: " + eyeDistance + 
							", Mid Point: (" + midPoint.X + ", " + midPoint.Y + ")");
						//draw a square in the picture
						canvas.DrawRect((int)midPoint.X - eyeDistance , 
							(int)midPoint.Y- eyeDistance , 
							(int)midPoint.X + eyeDistance, 
							(int)midPoint.Y + eyeDistance, drawPaint);
					}
				}

				//get imageview from layout
				ImageView imageView = (ImageView)FindViewById(Resource.Id.image_view);
				//set image with the red squares to imageview
				imageView.SetImageBitmap(bitmap565);
			}
		}
	}
}


