using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MMIKinect {

	class Packet {

		/// <summary>
		/// Constante définissant la taille du header du paquet
		/// </summary>
		private const uint _headerLength = 5;

		/// <summary>
		/// Network Stream ouvert sur la socket
		/// </summary>
		private NetworkStream _stream;

		/// <summary>
		/// Données du paquet
		/// </summary>
		private byte[] _data;

		/// <summary>
		/// Type du paquet
		/// </summary>
		private char _type;

		/// <summary>
		/// Constructeur de la classe Paquet
		/// </summary>
		/// <param name="stream">NetworkStream ouvert sur la socket de communication</param>
		public Packet( NetworkStream stream ) {
			_stream = stream;
		}

		/// <summary>
		/// Renvoi la taille du header du paquet
		/// </summary>
		/// <returns>Taille du header du paquet</returns>
		public uint getHeaderSize() {
			return _headerLength;
		}

		/// <summary>
		/// Renvoi la taille des données du paquet
		/// </summary>
		/// <returns>Taille des données du paquet</returns>
		public uint getBodySize() {
			return (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(getData(), 1));
		}

		/// <summary>
		/// Renvoi un byte[] contenant les données du paquet
		/// </summary>
		/// <returns>Données du paquet : byte[]</returns>
		public byte[] getData() {
			if(_data == null) {
				try {
					_data = new byte[getHeaderSize()];
					readBuffer(_data, 0, (int)getHeaderSize());
					setBodySize();
					if(getBodySize() != 0)
						readBuffer(_data, (int)getHeaderSize(), (int)getBodySize());
				} catch(Exception e) {
					Console.WriteLine(e.Message);
				}
			}
			return _data;
		}

		/// <summary>
		/// Redéfini la taille de byte[] _data en fonction de la taille reçue du paquet
		/// </summary>
		private void setBodySize() {
			byte[] tmp = new byte[_data.Length];
			_data.CopyTo(tmp, 0);
			_data = new byte[getHeaderSize() + getBodySize()];
			tmp.CopyTo(_data, 0);
		}

		/// <summary>
		/// Remplit buffer de start et de longueur length en lisant _stream
		/// </summary>
		/// <param name="buffer">Tableau de byte à remplir</param>
		/// <param name="start">Début du remplissage</param>
		/// <param name="length">Taille du remplissage</param>
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

		/// <summary>
		/// Retourne _data : byte[] converti en string UTF8
		/// </summary>
		/// <returns>Les données sous forme de string (UTF8)</returns>
		public string getMessage() {
			return ByteArrayToStr(getData(), getHeaderSize(), getBodySize());
		}

		/// <summary>
		/// Converti un tableau de byte en une string en commençant à l'index index et en prenant count éléments
		/// </summary>
		/// <param name="bArr">byte []</param>
		/// <param name="index">début</param>
		/// <param name="count">nombre d'éléments</param>
		/// <returns>string (UTF8)</returns>
		private static string ByteArrayToStr( byte[] bArr, uint index, uint count ) {
			System.Text.UTF8Encoding enc = new System.Text.UTF8Encoding();
			return enc.GetString(bArr, (int)index, (int)count);
		}

		/// <summary>
		/// Converti l'intégralité d'un tableau de byte sous forme de string (UTF8)
		/// </summary>
		/// <param name="bArr">byte []</param>
		/// <returns>string (UTF8)</returns>
		private static string ByteArrayToStr( byte[] bArr ) {
			return ByteArrayToStr(bArr, 0, (uint)bArr.Length);
		}


	}
}
