using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;
using MMIKinect.Network;

namespace MMIKinect.PplTracking {
	/// <summary>
	/// Logique d'interaction pour SkeletonsTracker.xaml
	/// </summary>
	public partial class PplsTracker : UserControl {

		public static readonly DependencyProperty KinectProperty = DependencyProperty.Register(
					 "Kinect",
					 typeof(KinectSensor),
					 typeof(PplsTracker),
					 new PropertyMetadata(
							 null, ( o, args ) => ((PplsTracker)o).OnSensorChanged((KinectSensor)args.OldValue, (KinectSensor)args.NewValue)));

		public Dictionary<int, PplTracker> _trackedPpls = new Dictionary<int, PplTracker>();

		private bool _disposed;

		private const uint MaxMissedFrames = 100;

		private byte[] _colorImage;

		private ColorImageFormat _colorImageFormat = ColorImageFormat.Undefined;

		private short[] _depthImage;

		private DepthImageFormat _depthImageFormat = DepthImageFormat.Undefined;

		private Skeleton[] _skeletonData;

		private double _soundSource;

		WriteableBitmap _colorBitmap;

		public bool doIdentification = false;

		private KinectSensor _sensor {
			get {
				return (KinectSensor)this.GetValue(KinectProperty);
			}

			set {
				this.SetValue(KinectProperty, value);
			}
		}

		public PplsTracker() {
			InitializeComponent();
			ClientTCP.getInstance.setTrackedPpls(_trackedPpls);
		}

		/// <summary>
		/// Destructeur
		/// </summary>
		~PplsTracker() { Dispose(false); }


		protected override void OnRender( DrawingContext drawingContext ) {
			base.OnRender(drawingContext);
			foreach(PplTracker st in this._trackedPpls.Values) {
				st.DrawInfos(drawingContext);
			}
		}

		/// <summary>
		/// Retourne toutes les personnes trackés
		/// </summary>
		/// <returns>Dictionnaires des personnes trackées</returns>
		public Dictionary<int, PplTracker> getPpls() {
			return _trackedPpls;
		}

		/// <summary>
		/// Capte l'évènement de changement de capteur Kinect
		/// </summary>
		/// <param name="oldSensor">Ancien capteur Kinect</param>
		/// <param name="newSensor">Nouveau capteur Kinect</param>
		private void OnSensorChanged( KinectSensor oldSensor, KinectSensor newSensor ) {
			if(oldSensor != null) {
				oldSensor.AllFramesReady -= OnAllFramesReady;
				oldSensor.AudioSource.SoundSourceAngleChanged -= OnSourceSourceAngleChanged;
				ResetTracking();
			}

			if(newSensor != null) {
				newSensor.AllFramesReady += OnAllFramesReady;
				newSensor.AudioSource.SoundSourceAngleChanged += OnSourceSourceAngleChanged;
			}
		}

		private void OnSourceSourceAngleChanged( object sender, SoundSourceAngleChangedEventArgs e ) {
			double angle = (e.Angle - 8.5) / 28.5;
			_soundSource = 320 + (angle * 320);
		}

		/// <summary>
		/// Reset le tracking
		/// </summary>
		private void ResetTracking() {
			foreach(int trackingId in new List<int>(this._trackedPpls.Keys)) {
				RemoveTracker(trackingId);
			}
		}

		/// <summary>
		/// Supprime le tracking d'un squelette
		/// </summary>
		/// <param name="trackingId">Id du squelette</param>
		private void RemoveTracker( int trackingId ) {
			this._trackedPpls[trackingId].Dispose();
			this._trackedPpls.Remove(trackingId);
		}

		/// <summary>
		/// Supprime les anciens trackers
		/// </summary>
		/// <param name="currentFrameNumber">Numéro de frame courant</param>
		private void RemoveOldTrackers( int currentFrameNumber ) {
			var trackersToRemove = new List<int>();

			foreach(var tracker in this._trackedPpls) {
				uint missedFrames = (uint)currentFrameNumber - (uint)tracker.Value._lastTracketFrame;
				if(missedFrames > MaxMissedFrames) {
					// Ils y a eu trop de frames depuis la dernère fois que nous avons vu ce squelette
					trackersToRemove.Add(tracker.Key);
				}
			}

			foreach(int trackingId in trackersToRemove) {
				this.RemoveTracker(trackingId);
			}
		}

		/// <summary>
		/// Capte l'évènement quand tout les frames sont prêtes
		/// </summary>
		/// <param name="sender">Objet envoyant l'évènement</param>
		/// <param name="e">Arguments de l'évènement</param>
		private void OnAllFramesReady( object sender, AllFramesReadyEventArgs allFramesReadyEventArgs ) {
			ColorImageFrame colorImageFrame = null;
			DepthImageFrame depthImageFrame = null;
			SkeletonFrame skeletonFrame = null;

			try {
				colorImageFrame = allFramesReadyEventArgs.OpenColorImageFrame();
				depthImageFrame = allFramesReadyEventArgs.OpenDepthImageFrame();
				skeletonFrame = allFramesReadyEventArgs.OpenSkeletonFrame();

				if(colorImageFrame == null || depthImageFrame == null || skeletonFrame == null) {
					return;
				}

				// Check for image format changes.  The FaceTracker doesn't
				// deal with that so we need to reset.
				if(_depthImageFormat != depthImageFrame.Format) {
					ResetTracking();
					_depthImage = null;
					_depthImageFormat = depthImageFrame.Format;
				}

				if(_colorImageFormat != colorImageFrame.Format) {
					ResetTracking();
					_colorImage = null;
					_colorImageFormat = colorImageFrame.Format;
				}

				// Create any buffers to store copies of the data we work with
				if(_depthImage == null) {
					_depthImage = new short[depthImageFrame.PixelDataLength];
				}

				if(_colorImage == null) {
					_colorImage = new byte[colorImageFrame.PixelDataLength];
				}

				// Get the skeleton information
				if(_skeletonData == null || _skeletonData.Length != skeletonFrame.SkeletonArrayLength) {
					_skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
				}

				colorImageFrame.CopyPixelDataTo(this._colorImage);
				depthImageFrame.CopyPixelDataTo(this._depthImage);
				skeletonFrame.CopySkeletonDataTo(this._skeletonData);

				_colorBitmap = new WriteableBitmap(
													_sensor.ColorStream.FrameWidth,
													_sensor.ColorStream.FrameHeight,
													96.0, 96.0, PixelFormats.Bgr32, null);

				_colorBitmap.WritePixels(
								new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
								_colorImage,
								_colorBitmap.PixelWidth * sizeof(int),
				0);

				// Update the list of trackers and the trackers with the current frame information
				foreach(Skeleton skeleton in this._skeletonData) {
					if(skeleton.TrackingState == SkeletonTrackingState.Tracked
							|| skeleton.TrackingState == SkeletonTrackingState.PositionOnly) {
						// We want keep a record of any skeleton, tracked or untracked.
						if(!_trackedPpls.ContainsKey(skeleton.TrackingId)) {
							_trackedPpls.Add(skeleton.TrackingId, new PplTracker());
						}

						// Give each tracker the upated frame.
						PplTracker skeletonFaceTracker;
						if(_trackedPpls.TryGetValue(skeleton.TrackingId, out skeletonFaceTracker)) {
							skeletonFaceTracker.OnFrameReady(_sensor, _colorImageFormat, _colorImage, _depthImageFormat, _depthImage, skeleton, _soundSource, _colorBitmap, doIdentification);
							skeletonFaceTracker._lastTracketFrame = skeletonFrame.FrameNumber;
						}
					}
				}

				this.RemoveOldTrackers(skeletonFrame.FrameNumber);

				this.InvalidateVisual();

			} finally {
				if(colorImageFrame != null) {
					colorImageFrame.Dispose();
				}

				if(depthImageFrame != null) {
					depthImageFrame.Dispose();
				}

				if(skeletonFrame != null) {
					skeletonFrame.Dispose();
				}
			}
		}

		public void Dispose() {
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose( bool p ) {
			if(!this._disposed) {
				this.ResetTracking();
				this._disposed = true;
			}
		}

	}
}
