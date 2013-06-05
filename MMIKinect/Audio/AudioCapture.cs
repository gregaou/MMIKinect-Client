namespace MMIKinect.Audio {
	using System.IO;
	using System.Threading;
	using Microsoft.Kinect;
	using Microsoft.Kinect.Toolkit;
	using System.Collections;
	using System;
	class AudioCapture {

		private static AudioCapture instance;

		private KinectSensorChooser _sensorChooser;

		public Stream _audioStream;

		private bool _isReading;

		private Thread _readingThread;

		/// <summary>
		/// Number of milliseconds between each read of audio data from the stream.
		/// Faster polling (few tens of ms) ensures a smoother audio stream visualization.
		/// </summary>
		private const int _audioPollingInterval = 50;

		/// <summary>
		/// Number of samples captured from Kinect audio stream each millisecond.
		/// </summary>
		private const int _samplesPerMillisecond = 16;

		/// <summary>
		/// Number of bytes in each Kinect audio stream sample.
		/// </summary>
		private const int _bytesPerSample = 2;

		/// <summary>
		/// Buffer used to hold audio data read from audio stream.
		/// </summary>
		private readonly byte[] audioBuffer = new byte[_audioPollingInterval * _samplesPerMillisecond * _bytesPerSample];

		MemoryStream _audioContent;

		private AudioCapture() {
			_sensorChooser = new KinectSensorChooser();
			_sensorChooser.KinectChanged += OnKinectSensorChanged;
		}

		public bool isReading() {
			return _isReading;
		}

		private void OnKinectSensorChanged( object sender, KinectChangedEventArgs e ) {
			KinectSensor oldSensor = e.OldSensor;
			KinectSensor newSensor = e.NewSensor;

			if(oldSensor != null) {
				oldSensor.AudioSource.Stop();
			}

			if(newSensor != null) {
				_audioStream = newSensor.AudioSource.Start();
			}

		}

		private Stream getAudioStream() {
			if(_audioStream != null) _audioStream = _sensorChooser.Kinect.AudioSource.Start();
			return _audioStream;
		}

		public byte[] stopCapture() {
				lock(_audioContent) {
					_isReading = false;
					byte[] audio = finalizeWave(_audioContent.GetBuffer());
					//_audioContent.WriteTo(new FileStream("test.wav",FileMode.OpenOrCreate));
					return audio;
				}
			}


		public AudioCapture startCapture() {
			if(_isReading != true) {
				_isReading = true;
				_audioContent = new MemoryStream();
				initMemoryStream();
				_readingThread = new Thread(AudioReadingThread);
				_readingThread.Start();
			}
			return this;
		}

		private void initMemoryStream() {
			byte[] firstHeaderBlock = new byte[] { 
					(byte)'R', (byte)'I', (byte)'F', (byte)'F', 
					0, 0, 0, 0, 
					(byte)'W', (byte)'A', (byte)'V', (byte)'E' 
				};

			byte[] secondHeaderBlock = new byte[] {
					(byte)'f', (byte)'m', (byte)'t', (byte)' ',
					0, 0, 0, 0,																		// Block size
					0, 0,																					// PCM Format
					0, 0,																					// Chanels number
					0, 0, 0, 0,																		// Frequency
					0, 0, 0, 0,																		// Bytes per sec
					0, 0,																					// Bytes per bloc
					0, 0,																					// Bits per sample
					0, 0																					// ???
			};

			byte[] thirdHeaderBlock = new byte[] {
				(byte)'d', (byte)'a', (byte)'t', (byte)'a',
				0, 0, 0, 0																			// Data size (file size - WAVE_HEADER_SIZE)
			};

			/* Second bloc */
			writeWaveValue(secondHeaderBlock, (int)Wave.WAVE_SECOND_BLOC_SIZE - 8, 4, 4);
			writeWaveValue(secondHeaderBlock, (int)Wave.WAVE_PCM_FORMAT, 2, 8);
			writeWaveValue(secondHeaderBlock, (int)Wave.KINECT_CHANNELS, 2, 10);
			writeWaveValue(secondHeaderBlock, (int)Wave.KINECT_FREQUENCY, 4, 12);
			writeWaveValue(secondHeaderBlock, (int)Wave.KINECT_BYTES_PER_SEC, 4, 16);
			writeWaveValue(secondHeaderBlock, (int)Wave.KINECT_BYTES_PER_BLOC, 2,20);
			writeWaveValue(secondHeaderBlock, (int)Wave.KINECT_BITS_PER_SAMPLE, 2,22);

			_audioContent.Write(firstHeaderBlock, 0, firstHeaderBlock.Length);
			_audioContent.Write(secondHeaderBlock, 0, secondHeaderBlock.Length);
			_audioContent.Write(thirdHeaderBlock, 0, thirdHeaderBlock.Length);
		}

		private byte[] finalizeWave(byte[] audio) {
			writeWaveValue(audio, (int)(audio.Length - 8), 4,4);
			writeWaveValue(audio, (int)(_audioContent.Length - (int)Wave.WAVE_HEADER_SIZE), 4,42);
			return audio;
		}

		private void writeWaveValue(byte[] src, int value, int size)
		{
			UInt32[] masks = new UInt32[4] {0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000};

			for (UInt16 i=0; i<size; i++)	src[i] = (byte)((value & masks[i]) >> (i*8));
		}

		private void writeWaveValue( byte[] src, int value, int size, int start ) {
			UInt32[] masks = new UInt32[4] { 0x000000ff, 0x0000ff00, 0x00ff0000, 0xff000000 };

			for(UInt16 i = 0; i < size; i++) src[i+start] = (byte)((value & masks[i]) >> (i * 8));
		}

		private void AudioReadingThread() {
			while(_isReading) {
				lock(_audioContent) {
					int readCount = _audioStream.Read(audioBuffer, 0, audioBuffer.Length);
					_audioContent.Write(audioBuffer, 0, readCount);
				}
			}
		}

		public static AudioCapture getInstance {
			get {
				if(instance == null) {
					instance = new AudioCapture();
				}
				return instance;
			}
		}

		public AudioCapture setSensor( KinectSensorChooser ksc ) { _sensorChooser = ksc; return this; }
	}
}
