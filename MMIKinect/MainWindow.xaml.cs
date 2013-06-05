namespace MMIKinect {

	using System;
	using System.IO;
	using System.Windows;
	using System.Windows.Data;
	using System.Windows.Media;
	using System.Windows.Media.Imaging;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit;
	using MMIKinect.Network;
	using MMIKinect.PplTracking;
	using MMIKinect.PplTraining;

	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {

		private readonly KinectSensorChooser _sensorChooser;

		private ClientTCP _clientTCP { get; set; }

		enum State { Connected, Connecting, NotConnected };

		private bool _audioTrain = false;

		private bool _audioIden = false;

		/// <summary>
		/// Bitmap that will hold color information
		/// </summary>
		private WriteableBitmap _colorBitmap;

		/// <summary>
		/// Intermediate storage for the color data received from the camera
		/// </summary>
		private byte[] _colorPixels;

		/// <summary>
		/// Constructeur de la classe MainWindow
		/// </summary>
		public MainWindow() {
			InitializeComponent();

			// Initialisation du capteur Kinect
			this._sensorChooser = new KinectSensorChooser();

			var kinectBinding = new Binding("Kinect") { Source = _sensorChooser };
			_skeletonsTracker.SetBinding(PplsTracker.KinectProperty, kinectBinding);

			this._sensorChooser.KinectChanged += SensorChooserOnKinectChanged;
			this._sensorChooser.Start();

			Audio.AudioCapture.getInstance.setSensor(_sensorChooser);
		}

		~MainWindow() { ClientTCP.getInstance.finish(); }

		/// <summary>
		/// Actions à effectuées lors d'un changement du capteur Kinect
		/// </summary>
		/// <param name="sender">Objet appelant la fonction</param>
		/// <param name="e">Arguments</param>
		private void SensorChooserOnKinectChanged( object sender, KinectChangedEventArgs e ) {
			KinectSensor oldSensor = e.OldSensor;
			KinectSensor newSensor = e.NewSensor;

			if(oldSensor != null) {
				try {
					oldSensor.DepthStream.Range = DepthRange.Default;
					oldSensor.SkeletonStream.EnableTrackingInNearRange = false;
					oldSensor.DepthStream.Disable();
					oldSensor.DepthStream.Range = DepthRange.Default;
					oldSensor.SkeletonStream.Disable();
					oldSensor.ColorStream.Disable();
					Audio.AudioCapture.getInstance._audioStream = null;
				} catch(InvalidOperationException) {
					// Le capteur Kinect est entré dans un état invalide durant l'activation/désactivation des flux ou des paramètres de flux
					// i.e.: Le capteur Kinect à été débranché brutalement
				}
			}

			if(newSensor != null) {
				try {
					newSensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);
					newSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
					newSensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
					newSensor.SkeletonStream.Enable();
					newSensor.ColorFrameReady += SensorColorFrameReady;
					Audio.AudioCapture.getInstance._audioStream = newSensor.AudioSource.Start();
					try {
						newSensor.DepthStream.Range = DepthRange.Near;
						newSensor.SkeletonStream.EnableTrackingInNearRange = true;
					} catch(InvalidOperationException) {
						// Il ne s'agit pas d'une Kinect pour Windows donc elle ne supporte pas le Near mode, donc on fixe le paramètre au mode par défault
						newSensor.DepthStream.Range = DepthRange.Default;
						newSensor.SkeletonStream.EnableTrackingInNearRange = false;
					}
				} catch(InvalidOperationException) {
					// Le capteur Kinect est entré dans un état invalide durant l'activation/désactivation des flux ou des paramètres de flux
					// i.e.: Le capteur Kinect à été débranché brutalement
				}
			}
		}

		/// <summary>
		/// Permet de récupérer le capteur Kinect courant
		/// </summary>
		/// <exception cref="NullReferenceException">Pas de capteur kinect connecté</exception>
		/// <returns>KinectSensor : Le capteur Kinect courant</returns>
		private KinectSensor getSafeSensor() {
			if(_sensorChooser.Kinect == null)
				throw new NullReferenceException("No Kinect Sensor available !");
			return _sensorChooser.Kinect;
		}

		/// <summary>
		/// Renvoi l'image (format Bitmap) envoyé par le capteur Kinect
		/// </summary>
		/// <returns>ImageSource image Bitmap</returns>
		private ImageSource getColorBitmap() {
			try {
				if(_colorBitmap == null)
					_colorBitmap = new WriteableBitmap(
													getSafeSensor().ColorStream.FrameWidth,
													getSafeSensor().ColorStream.FrameHeight,
													96.0, 96.0, PixelFormats.Bgr32, null);
			} catch(NullReferenceException e) {
				setStatusBarMessage(e.Message);
			}
			return _colorBitmap;
		}

		/// <summary>
		/// Retourne l'image (format RAW) envoyé par le capteur Kinect
		/// </summary>
		/// <returns>byte[] image RAW</returns>
		private byte[] getColorPixels() {
			if(_colorPixels == null)
				_colorPixels = new byte[getSafeSensor().ColorStream.FramePixelDataLength];
			return _colorPixels;
		}

		/// <summary>
		/// Event handler for Kinect sensor's ColorFrameReady event
		/// </summary>
		/// <param name="sender">object sending the event</param>
		/// <param name="e">event arguments</param>
		private void SensorColorFrameReady( object sender, ColorImageFrameReadyEventArgs e ) {
			using(ColorImageFrame colorFrame = e.OpenColorImageFrame()) {
				if(colorFrame != null) {
					// Copy the pixel data from the image to a temporary array
					try {
						colorFrame.CopyPixelDataTo(getColorPixels());

						// Write the pixel data into our bitmap
						_colorBitmap.WritePixels(
								new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
								getColorPixels(),
								_colorBitmap.PixelWidth * sizeof(int),
								0);
					} catch(NullReferenceException ex) {
						setStatusBarMessage(ex.Message);
					}
				}
			}
		}


		/// <summary>
		/// Fonction appelée au chargement de la fenêtre
		/// </summary>
		/// <param name="sender">Objet envoyant l'évènement</param>
		/// <param name="e">Arguments de l'évènement</param>
		private void display( object sender, RoutedEventArgs e ) {
			_kinectImage.Source = getColorBitmap();
		}

		/// <summary>
		/// Change le message de la status bar
		/// </summary>
		/// <param name="msg">Message à afficher</param>
		/// <returns>L'objet MainWindow courant</returns>
		private MainWindow setStatusBarMessage( string msg ) {
			_statusBarText.Text = msg;
			return this;
		}

		private void OnButtonTrainHistoClick( object sender, RoutedEventArgs e ) {
			if(verifPPlPresentForTraining()) {
				foreach(PplTracker pplTracker in _skeletonsTracker.getPpls().Values) {
					if(pplTracker._skeletonTrackingState == SkeletonTrackingState.Tracked) {
						try {
							HistogramTraining ht = new HistogramTraining();
							ht.setPplTracker(pplTracker).setPplName(_nameTraining.Text).doTraining().sendTraining();
						} catch(Exception ex) { setStatusBarMessage(ex.Message); }
					} else setStatusBarMessage("La personne doit être trackée pour effectuer un entrainement");
				}
			} else setStatusBarMessage("Une et une seule personne doit être présente pour effectuer un entrainement");
		}

		private bool verifPPlPresentForTraining() {
			return (_skeletonsTracker.getPpls().Count == 1);
		}

		private void OnButtonTrainFaceClick( object sender, RoutedEventArgs e ) {
			if(verifPPlPresentForTraining()) {
				foreach(PplTracker pplTracker in _skeletonsTracker.getPpls().Values) {
					if(pplTracker._skeletonTrackingState == SkeletonTrackingState.Tracked) {
						try {
							FaceTraining ft = new FaceTraining();
							ft.setPplTracker(pplTracker).setPplName(_nameTraining.Text).doTraining().sendTraining();
						} catch(Exception ex) { setStatusBarMessage(ex.Message); }
					} else setStatusBarMessage("La personne doit être trackée pour effectuer un entrainement");
				}
			} else setStatusBarMessage("Une et une seule personne doit être présente pour effectuer un entrainement");
		}

		//private void onClick( object sender, RoutedEventArgs e ) {
		//	try {
		//		foreach(PplTracker pplTracker in _skeletonsTracker.getPpls().Values) {
		//			if(pplTracker._skeletonTrackingState == SkeletonTrackingState.Tracked) {
		//				Packet p = new Packet();
		//				byte[] img = null;
		//				while(img == null) img = pplTracker.getBodyImage();
		//				File.WriteAllBytes("testcapture.jpg", img);
		//				p.setId(pplTracker.getId())
		//					.setType((byte)PacketType.HISTOGRAMM_SCORING_REQUEST)
		//					.setVersion((int)PacketVersion.ACTUAL)
		//					.setData(img)
		//					.doSend();
		//			}
		//		}
		//	} catch(Exception ex) { Console.WriteLine(ex.Message); }
		//}

		private void OnButtonTrainSoundClick( object sender, RoutedEventArgs e ) {
			lock(_audioTrainButton) {
				if(_audioTrain) {
					byte[] audio = Audio.AudioCapture.getInstance.stopCapture();
					if(verifPPlPresentForTraining()) {
						foreach(PplTracker pplTracker in _skeletonsTracker.getPpls().Values) {
							if(pplTracker._skeletonTrackingState == SkeletonTrackingState.Tracked) {
								try {
									AudioTraining at = new AudioTraining();
									at.setAudio(audio).setPplTracker(pplTracker).setPplName(_nameTraining.Text).doTraining().sendTraining();
								} catch(Exception ex) { setStatusBarMessage(ex.Message); }
							} else setStatusBarMessage("La personne doit être trackée pour effectuer un entrainement");
						}
					} else setStatusBarMessage("Une et une seule personne doit être présente pour effectuer un entrainement");
					_audioTrainButton.Content = "Démarrer entrainement audio";
					_audioTrain = false;
				} else {
					Audio.AudioCapture.getInstance.startCapture();
					_audioTrainButton.Content = "Arrêter entrainement audio";
					_audioTrain = true;
				}
			}
		}

		private void OnTrainChecked( object sender, RoutedEventArgs e ) {
			_skeletonsTracker.doIdentification = false;
		}

		private void OnIdentificationChecked( object sender, RoutedEventArgs e ) {
			_skeletonsTracker.doIdentification = true;
			if(Audio.AudioCapture.getInstance.isReading()) Audio.AudioCapture.getInstance.stopCapture();
			_audioTrainButton.Content = "Démarrer entrainement audio";
		}

		private void OnButtonIdenSoundClick( object sender, RoutedEventArgs e ) {
			lock(_audioIdenButton) {
				if(_audioIden) {
					byte[] audio = Audio.AudioCapture.getInstance.stopCapture();
					Packet p = new Packet();
					p.setId(42)
						.setType((byte)PacketType.AUDIO_SCORING_REQUEST)
						.setVersion((int)PacketVersion.ACTUAL)
						.setData(audio)
						.doSend();
					_audioIdenButton.Content = "Démarrer une identification vocale";
					_audioIden = false;
				} else {
					Audio.AudioCapture.getInstance.startCapture();
					_audioIdenButton.Content = "Arrêter une identification vocale";
					_audioIden = true;
				}
			}
		}


	}
}
