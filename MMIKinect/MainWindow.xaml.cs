using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Kinect;


namespace MMIKinect {
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		/// <summary>
		/// Active Kinect sensor
		/// </summary>
		private KinectSensor _sensor;

		private MemoryStream _jpgImage;

		private ClientTCP _clientTCP;

		/// <summary>
		/// Bitmap that will hold color information
		/// </summary>
		private WriteableBitmap _colorBitmap;

		/// <summary>
		/// Intermediate storage for the color data received from the camera
		/// </summary>
		private byte[] _colorPixels;

		public MainWindow() {
			InitializeComponent();
			_clientTCP = new ClientTCP();
		}

		~MainWindow() {
			if(_sensor != null) {
				_sensor.Stop();
				_sensor.ColorFrameReady -= SensorColorFrameReady;
			}
		}

		/// <summary>
		/// Accesseur kinect sensor
		/// </summary>
		private KinectSensor getSafeSensor() {
			if(_sensor == null) {
				foreach(var potentialSensor in KinectSensor.KinectSensors) {
					if(potentialSensor.Status == KinectStatus.Connected) {
						_sensor = potentialSensor;
						break;
					}
				}
				if(_sensor == null) throw new NullReferenceException(Properties.Resources.noKinectFound);
				doSensorConfigAndStart();
			}
			return _sensor;
		}

		private void doSensorConfigAndStart() {
			_sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
			_sensor.ColorFrameReady += SensorColorFrameReady;
			_sensor.Start();
		}

		private ImageSource getColorBitmap() {
			if(_colorBitmap == null)
				_colorBitmap = new WriteableBitmap(
												getSafeSensor().ColorStream.FrameWidth,
												getSafeSensor().ColorStream.FrameHeight,
												96.0, 96.0, PixelFormats.Bgr32, null);
			return _colorBitmap;
		}

		public MemoryStream getJpegImage() {
			return _jpgImage;
		}

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
					colorFrame.CopyPixelDataTo(getColorPixels());

					// Write the pixel data into our bitmap
					_colorBitmap.WritePixels(
							new Int32Rect(0, 0, _colorBitmap.PixelWidth, _colorBitmap.PixelHeight),
							getColorPixels(),
							_colorBitmap.PixelWidth * sizeof(int),
							0);

					_jpgImage = new MemoryStream();
					unsafe {
						fixed(byte* ptr = getColorPixels()) {
							using(Bitmap image = new Bitmap(colorFrame.Width, colorFrame.Height, colorFrame.Width * 4, System.Drawing.Imaging.PixelFormat.Format32bppArgb, new IntPtr(ptr))) {
								image.Save(_jpgImage, ImageFormat.Jpeg);
								image.Save("coucou.jpg", ImageFormat.Jpeg);
							}
						}
					}

					_clientTCP.doSendImage(_jpgImage);

						
				}
			}
		}

		private void display( object sender, RoutedEventArgs e ) {
			_kinectImage.Source = getColorBitmap();
		}
	}
}
