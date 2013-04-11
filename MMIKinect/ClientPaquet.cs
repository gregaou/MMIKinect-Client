using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MMIKinect {
	class ClientPaquet {
		private const uint _headerLength = 5;
		NetworkStream _stream;
		protected byte[] _data { get; private set; }

		public ClientPaquet( NetworkStream stream ) {
			_stream = stream;
		}

		protected void setBodySize( uint size ) {
			byte[] tmp = new byte[_data.Length];
			_data.CopyTo(tmp, 0);
			_data = new byte[getHeaderSize() + size];
			tmp.CopyTo(_data, 0);
		}

		public byte[] getData() {
			if(_data == null) {
				try {
					_data = new byte[getHeaderSize()];
					readBuffer(_data, 0, (int)getHeaderSize());
					Console.WriteLine("Datasize :" + _data.Length);
					setBodySize(getBodySize());
					Console.WriteLine("Datasize :" + _data.Length);
					Console.WriteLine("[0] :" + _data[0]);
					Console.WriteLine("[1] :" + _data[1]);
					Console.WriteLine("[2] :" + _data[2]);
					Console.WriteLine("[3] :" + _data[3]);
					Console.WriteLine("[4] :" + _data[4]);
					Console.WriteLine("BodySize :" + getBodySize());
					Console.WriteLine("data.Length :" + _data.Length);
					if(getBodySize() != 0)
						readBuffer(_data, (int)getHeaderSize(), (int)getBodySize());
				} catch(Exception e) {
					Console.WriteLine(e.Message);
					System.Environment.Exit(0);
				}
			}
			return _data;
		}

		public uint getHeaderSize() {
			return _headerLength;
		}

		private void readBuffer( byte[] buffer, int start, int length ) {
			int n = 0, r;

			if(!_stream.CanRead)
				throw new Exception("The stream cannot read");

			while(n < length) {
				r = _stream.Read(buffer, start + n, length - n);
				if(r == 0)
					throw new Exception("Connection lost");

				n += r;
			}
		}

		public UInt32 getBodySize() {
			return (UInt32)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(getData(), 1));
		}

		public string getMessage() {
			return ByteArrayToStr(getData(), getHeaderSize(), getBodySize());
		}

		private static string ByteArrayToStr( byte[] bArr, uint index, uint count ) {
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			return enc.GetString(bArr, (int)index, (int)count);
		}

		private static string ByteArrayToStr( byte[] bArr ) {
			return ByteArrayToStr(bArr, 0, (uint)bArr.Length);
		}


	}
}
