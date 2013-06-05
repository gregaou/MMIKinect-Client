namespace MMIKinect.Network {

	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Net.Sockets;
	using System.Threading;
using MMIKinect.PplTracking;

	class ClientTCP : TcpClient {

		/// <summary>
		/// Instance du client TCP
		/// </summary>
		private static ClientTCP instance;

		List<Packet> _lPacket;

		public Dictionary<int, PplTracker> _trackedPpls;

		/// <summary>
		/// Renvoi l'instance de la classe ClientTCP
		/// </summary>
		public static ClientTCP getInstance {
			get {
				if(instance == null) instance = new ClientTCP();
				return instance;
			}
		}

		public ClientTCP setTrackedPpls( Dictionary<int, PplTracker> dp ) {
			_trackedPpls = dp;
			return this;
		}

		/// <summary>
		/// Stream d'écriture
		/// </summary>
		private NetworkStream _socket;

		private bool _connectFailed = true;

		/// <summary>
		/// Thread de lecture
		/// </summary>
		private Thread _threadRead;

		/// <summary>
		/// Adresse du serveur
		/// </summary>
		private string _serverAdress = "192.168.56.101";

		/// <summary>
		/// Port du serveur
		/// </summary>
		private int _serverPort = 1337;

		private ConnectionsInfo _dialog = new ConnectionsInfo();

		/// <summary>
		/// Constructeur
		/// </summary>
		private ClientTCP() {
			_lPacket = new List<Packet>();
			_threadRead = new Thread(new ThreadStart(readSocket));
			_threadRead.SetApartmentState(ApartmentState.STA);
			_threadRead.Start();
		}

		public void finish() { _threadRead.Abort(); }

		public ClientTCP doConnect() {
			try {
				Connect(getServerAdress, getServerPort);
				_connectFailed = false;
			} catch(Exception e) {
				_connectFailed = true;
				Console.WriteLine(e.Message);
			}
			return this;
		}

		private void setServerInfo() {
			if(_serverAdress == null || _serverPort == 0 || (_connectFailed == false && !Connected)) {
				_dialog.ServerAdress = _serverAdress;
				_dialog.ServerPort = _serverPort;
				if(_dialog.ShowDialog() == true) {
					_serverAdress = _dialog.ServerAdress;
					_serverPort = _dialog.ServerPort;
				}
			}
		}

		private string getServerAdress {
			get { setServerInfo(); return _serverAdress; }
		}

		private int getServerPort {
			get { setServerInfo(); return _serverPort; }
		}

		/// <summary>
		/// Renvoi le Stream d'écriture
		/// </summary>
		/// <returns>Le stream d'écriture</returns>
		public NetworkStream getSocket() {
			if(_socket == null) {
				if(!Connected) doConnect();
				_socket = GetStream();
			}
			return _socket;
		}

		/// <summary>
		/// Thread de la socket de lecture
		/// </summary>
		private void readSocket() {
			while(Thread.CurrentThread.IsAlive) {
				try {
					if(Connected) {
						Packet cp = new Packet();
						cp.getData();
						analysePacket(cp);
					}
				} catch(Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		private void analysePacket( Packet p ) {
			if((0x0F & p.getType()) == (byte)PacketType.SCORING_RESULT) {
				Console.WriteLine("--------------------");
				Console.WriteLine("Analyse du packet :");
				Console.WriteLine("  Id : " + p.getId());
				Console.WriteLine("  Type : " + p.getType());
				byte[] data = p.getData();
				byte[] nbScoreA = new byte[2];
				Buffer.BlockCopy(data, 0, nbScoreA, 0, 1);
				UInt16 nbScore = BitConverter.ToUInt16(nbScoreA, 0);
				Console.WriteLine(" Nombre de score(s) : " + nbScore );
				int nbBytes = 1;
				double min = double.MaxValue;
				double max = double.MinValue;
				string finalNameMin = "";
				string finalNameMax = "";
				for(int i = 0; i < nbScore; ++i) {
					byte[] scoreA = new byte[8];
					Buffer.BlockCopy(data, nbBytes, scoreA, 0, 8);
					double score = BitConverter.ToDouble(scoreA, 0);
					nbBytes += 8;
					byte[] nameSizeA = new byte[2];
					Buffer.BlockCopy(data, nbBytes, nameSizeA, 0, 2);
					nbBytes += 2;
					UInt16 nameSize = BitConverter.ToUInt16(nameSizeA, 0);
					byte[] nameA = new byte[nameSize];
					Buffer.BlockCopy(data, nbBytes, nameA, 0, nameSize);
					nbBytes += nameSize;
					string name = System.Text.Encoding.ASCII.GetString(nameA);
					if(score < min) {
						min = score;
						finalNameMin = name;
					}
					if(score > max) {
						max = score;
						finalNameMax = name;
					}
				}
				PplTracker ppl;
				if(_trackedPpls.TryGetValue((int)p.getId(),out ppl)) {
					switch ((0xF0 & p.getType())) {
						case (byte)PacketType.VIDEO_TYPE:
							ppl._nameFace = finalNameMin;
							Console.WriteLine("  Reconnaissance Faciale :" + finalNameMin);
							break;
						case (byte)PacketType.HISTOGRAMM_TYPE:
							ppl._nameHisto = finalNameMin;
							Console.WriteLine("  Reconnaissance Histogramme :" + finalNameMin);
							break;
					}
				}

				if((0xF0 & p.getType()) == (byte)PacketType.AUDIO_TYPE)
					Console.WriteLine("  Reconnaissance Audio :" + finalNameMax);

				Console.WriteLine("--------------------");
			}
		}

	}
}
