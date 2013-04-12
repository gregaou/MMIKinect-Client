using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MMIKinect {
	class ClientTCP : TcpClient {

		private NetworkStream _socket;

		private Thread _threadRead;

		public ClientTCP() 
		{
			_threadRead = new Thread(new ThreadStart(readSocket));
			_threadRead.Start();
		}

		~ClientTCP() { }

		ClientTCP doConnect() {
			try {
				Connect("127.0.0.1", 1337);
			} catch(Exception e) {
				Console.WriteLine("Erreur de connexion" + e.Message);
			}
			return this;
		}

		NetworkStream getSocket() {
			if(_socket == null) {
				doConnect();
				_socket = GetStream();
			}
			return _socket;
		}

		private void readSocket() {
			while(Thread.CurrentThread.IsAlive) {
				try {
					byte[] data = new byte[2]; data[0] = 65; data[1] = 66;

					Packet sendPacket = new Packet(getSocket());
					sendPacket.setType(1).setData(data).doSend();

					Packet cp = new Packet(getSocket());

					string s = cp.getMessage();
					System.Console.WriteLine("Message : " + s);
				} catch(Exception e) {
					Console.WriteLine(e.Message);
				}
			}
		}

		ClientTCP doSendImage( MemoryStream image ) {
			return this;
		}

		ClientTCP doSendSound( MemoryStream sound ) {
			return this;
		}

		public static byte[] StrToByteArray( string str ) {
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			return encoding.GetBytes(str);
		}

		public static string ByteArrayToStr( byte[] bArr ) {
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			return enc.GetString(bArr);
		}

	}
}
