namespace MMIKinect.PplTracking {
	using System;
	using System.ComponentModel;
	using System.Diagnostics;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Globalization;
	using System.IO;
	using System.Windows;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit.FaceTracking;
	using MMIKinect.Network;
	public class PplTracker : IDisposable {

		/// <summary>
		/// Détecteur de visage
		/// </summary>
		private FaceTracker _faceTracker;

		/// <summary>
		/// Défini l'orientation du visage
		/// </summary>
		public enum Orientation { TOPLEFT, TOPMIDDLE, TOPRIGHT, MIDLEFT, MIDMIDDLE, MIDRIGHT, BOTLEFT, BOTMIDDLE, BOTRIGHT, UNDEFINED };

		/// <summary>
		/// 
		/// </summary>
		private FaceTrackFrame _frameFaceTrack;

		/// <summary>
		/// 
		/// </summary>
		private double _sourceAngle;

		private DateTime _lastIdentification;

		/// <summary>
		/// Détermine si le dernier suivi de la personne à réussie
		/// </summary>
		private bool _lastFaceTrackSucceeded;

		/// <summary>
		/// Etat du suivi du squelette
		/// </summary>
		public SkeletonTrackingState _skeletonTrackingState;

		/// <summary>
		/// Squelette de la personne
		/// </summary>
		private Skeleton _skeleton;

		/// <summary>
		/// Capteur Kinect utilisé
		/// </summary>
		private KinectSensor _sensor;

		private volatile WriteableBitmap _image;

		/// <summary>
		/// Numéro de la dernière frame analysée
		/// </summary>
		public int _lastTracketFrame;

		public string _nameHisto = null;
		public string _nameFace = null;

		/// <summary>
		/// Constructeur
		/// </summary>
		public PplTracker() {
			_frameFaceTrack = null;
			_skeleton = null;
			_lastIdentification = DateTime.Now;
		}

		/// <summary>
		/// Met à jour les informations de face tracking de ce squelette
		/// </summary>
		internal void OnFrameReady( KinectSensor kinectSensor, ColorImageFormat colorImageFormat, byte[] colorImage, DepthImageFormat depthImageFormat, short[] depthImage, Skeleton skeletonOfInterest, double sourceAngle, WriteableBitmap image, bool doIdentification ) {
			_skeletonTrackingState = skeletonOfInterest.TrackingState;

			if(_skeletonTrackingState != SkeletonTrackingState.Tracked) {
				// Rien à faire avec un squelette non suivi
				return;
			}

			_skeleton = skeletonOfInterest;
			_sourceAngle = sourceAngle;
			_sensor = kinectSensor;
			_image = image;

			if(_faceTracker == null) {
				try { _faceTracker = new FaceTracker(kinectSensor); } catch(InvalidOperationException) {
					Debug.WriteLine("AllFramesReady - creating a new FaceTracker threw an InvalidOperationException");
					_faceTracker = null;
				}
			}

			if(_faceTracker != null) {
				_frameFaceTrack = _faceTracker.Track(colorImageFormat, colorImage, depthImageFormat, depthImage, skeletonOfInterest);
				_lastFaceTrackSucceeded = _frameFaceTrack.TrackSuccessful;

				if(DateTime.Compare(_lastIdentification.AddSeconds(5), DateTime.Now) < 0 && doIdentification) {
					_lastIdentification = DateTime.Now;
					Packet p = new Packet();
					byte[] img = null;
					img = getBodyImage();
					if(img == null) return;
					p.setId(getId())
						.setType((byte)PacketType.HISTOGRAMM_SCORING_REQUEST)
						.setVersion((int)PacketVersion.ACTUAL)
						.setData(img)
						.doSend();
					img = getFaceImage();
					if(img == null) return;
					p.setId(getId())
						.setType((byte)PacketType.VIDEO_SCORING_REQUEST)
						.setVersion((int)PacketVersion.ACTUAL)
						.setData(img)
						.doSend();
				}
			}

		}

		/// <summary>
		/// Retourne le rectangle du buste
		/// </summary>
		/// <returns>Le Rectangle du buste</returns>
		public Rectangle getBoundingBody() {

			if(!_lastFaceTrackSucceeded) return new Rectangle(0, 0, 0, 0);

			Skeleton skeleton = _skeleton;

			if(skeleton == null) return new Rectangle(0, 0, 0, 0);

			if(skeleton.Joints[JointType.ShoulderRight].TrackingState != JointTrackingState.Tracked) return new Rectangle(0, 0, 0, 0);
			if(skeleton.Joints[JointType.ShoulderLeft].TrackingState != JointTrackingState.Tracked) return new Rectangle(0, 0, 0, 0);
			if(skeleton.Joints[JointType.HipRight].TrackingState != JointTrackingState.Tracked) return new Rectangle(0, 0, 0, 0);
			if(skeleton.Joints[JointType.HipLeft].TrackingState != JointTrackingState.Tracked) return new Rectangle(0, 0, 0, 0);

			System.Windows.Point tL, tR, bL, bR;

			Joint toL = skeleton.Joints[JointType.ShoulderLeft];

			tL = SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderLeft].Position);
			tR = SkeletonPointToScreen(skeleton.Joints[JointType.ShoulderRight].Position);
			bL = SkeletonPointToScreen(skeleton.Joints[JointType.HipLeft].Position);
			bR = SkeletonPointToScreen(skeleton.Joints[JointType.HipRight].Position);

			double x, y, width, height, xMax, yMax;
			x = (tL.X < bL.X) ? tL.X : bL.X;
			y = (tL.Y < tR.Y) ? tL.Y : tR.Y;
			xMax = (tR.X > bR.X) ? tR.X : bR.X;
			yMax = (bR.Y > bL.Y) ? bR.Y : bL.Y;
			width = xMax - x;
			height = yMax - y;

			return new Rectangle((int)x, (int)y, (int)width, (int)height);
		}

		/// <summary>
		/// Obtenir la boîte englobante du visage 
		/// </summary>
		/// <returns>Une rectangle entourant le visage</returns>
		public System.Drawing.Rectangle getBoundingFaceRect() {

			if(!_lastFaceTrackSucceeded) return new Rectangle(0, 0, 0, 0);

			Microsoft.Kinect.Toolkit.FaceTracking.Rect frameRect = _frameFaceTrack.FaceRect;

			double width = (frameRect.Width * 1.40);
			double height = (frameRect.Height * 1.40);
			double x = frameRect.Left - (frameRect.Width * 0.20);
			double y = frameRect.Top - (frameRect.Height * 0.40);

			double ratio = 92 / 112;

			if(width / height > ratio) {
				double newHeight = (width * 112) / 92;
				y -= (newHeight - height) / 2;
				height = newHeight;
			} else if(width / height < ratio) {
				double newWidth = (height * 92) / 112;
				x -= (newWidth - width) / 2;
				width = newWidth;
			}

			return new Rectangle((int)x, (int)y, (int)width, (int)height);
		}

		public byte[] getFaceImage() {
			if(!_lastFaceTrackSucceeded) return null;
			Rectangle r = getBoundingFaceRect();
			WriteableBitmap wb = BitmapFactory.ConvertToPbgra32Format(_image).Crop(r.X, r.Y, r.Width, r.Height);
			var stream = new MemoryStream();
			var encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(wb));
			encoder.Save(stream);
			byte[] buffer = stream.GetBuffer();
			return buffer;
		}

		public UInt16 getId() {
			if(!_lastFaceTrackSucceeded) return 0;
			return (UInt16)_skeleton.TrackingId;
		}

		public byte[] getBodyImage() {
			if(!_lastFaceTrackSucceeded) return null;
			try {
				Rectangle r = getBoundingBody();
				WriteableBitmap wb = BitmapFactory.ConvertToPbgra32Format(_image).Crop(r.X, r.Y, r.Width, r.Height);
				var stream = new MemoryStream();
				var encoder = new JpegBitmapEncoder();
				encoder.Frames.Add(BitmapFrame.Create(wb));
				encoder.Save(stream);
				byte[] buffer = stream.GetBuffer();
				return buffer;
			} catch(Exception e) { Console.WriteLine(e.Message); }
			return null;
		}

		/// <summary>
		/// Obtient l'orientation du visage tracké
		/// </summary>
		/// <returns>l'orientation du visage</returns>
		public Orientation getOrientation() {

			if(!_lastFaceTrackSucceeded) return Orientation.UNDEFINED;

			Vector3DF v = _frameFaceTrack.Rotation;

			float x, y;

			x = v.X; y = v.Y;
			const int sensibilite = 15;

			if(x > sensibilite) {
				if(y > sensibilite) return Orientation.TOPLEFT;
				if(y < -sensibilite) return Orientation.TOPRIGHT;
				return Orientation.TOPMIDDLE;
			} else if(x < -sensibilite) {
				if(y > sensibilite) return Orientation.BOTLEFT;
				if(y < -sensibilite) return Orientation.BOTRIGHT;
				return Orientation.BOTMIDDLE;
			} else {
				if(y > sensibilite) return Orientation.MIDLEFT;
				if(y < -sensibilite) return Orientation.MIDRIGHT;
				return Orientation.MIDMIDDLE;
			}
		}

		private bool isTalking() {
			Rectangle rectHead = getBoundingFaceRect();
			return (_sourceAngle > rectHead.X - 0.25 * rectHead.Width && _sourceAngle < rectHead.X + rectHead.Width * 1.25);
		}

		public void DrawInfos( DrawingContext drawingContext ) {

			if(!_lastFaceTrackSucceeded || _skeletonTrackingState != SkeletonTrackingState.Tracked) {
				return;
			}

			DisplayRect(drawingContext, getBoundingBody());
			DisplayRect(drawingContext, getBoundingFaceRect());
			string name = (_nameFace == null && _nameHisto == null) ? getId().ToString() :
				(_nameFace == null) ? _nameHisto :
				(_nameHisto == null) ? _nameFace : "H: " +_nameHisto + " \n" + "F: " + _nameFace;
			drawingContext.DrawText(new FormattedText(name, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 16, System.Windows.Media.Brushes.Red), new System.Windows.Point(getBoundingFaceRect().Left, name.Contains("\n") ? getBoundingFaceRect().Top - 38 : getBoundingFaceRect().Top - 20));
		}

		private PplTracker DisplayRect( DrawingContext drawingContext, Rectangle r ) {
			if(r.IsEmpty) return this;

			System.Windows.Media.Pen p = (isTalking()) ?
				new System.Windows.Media.Pen(System.Windows.Media.Brushes.Green, 2) :
				new System.Windows.Media.Pen(System.Windows.Media.Brushes.Chocolate, 2);
			try {
				drawingContext.DrawRectangle(null, p, new System.Windows.Rect(r.X, r.Y, r.Width, r.Height));
			} catch(Exception e) {
				Console.WriteLine(e.Message);
			}
			return this;
		}

		/// <summary>
		/// Converti un point d'un squelette en point2D pour affichage sur le rendu
		/// </summary>
		/// <param name="skelpoint">Point à convertir</param>
		/// <returns>Point pour affichage sur le rendu</returns>
		private System.Windows.Point SkeletonPointToScreen( SkeletonPoint skelpoint ) {
			DepthImagePoint depthPoint = _sensor.CoordinateMapper.MapSkeletonPointToDepthPoint(skelpoint, DepthImageFormat.Resolution640x480Fps30);
			return new System.Windows.Point(depthPoint.X, depthPoint.Y);
		}

		public void Dispose() {
			if(_faceTracker != null) {
				_faceTracker.Dispose();
				_faceTracker = null;
			}
		}

	}
}
