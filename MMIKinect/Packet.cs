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
		private const uint _headerSize = 5;

		/// <summary>
		/// Variabe contenant la taille du corps du message
		/// </summary>
		uint _bodySize;

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
		private byte _type;

		/// <summary>
		/// Constructeur de la classe Paquet
		/// </summary>
		/// <param name="stream">NetworkStream ouvert sur la socket de communication</param>
		public Packet( NetworkStream stream ) {
			_bodySize = 0;
			_stream = stream;
			_type = 0xFF;
		}

		/// <summary>
		/// Renvoi la taille des données du paquet
		/// </summary>
		/// <returns>Taille des données du paquet</returns>
		public uint getBodySize() {
			if(_bodySize == 0) doReadHeader();
			return _bodySize;
		}

		/// <summary>
		/// Renvoi le type du paquet
		/// </summary>
		/// <returns>Taille des données du paquet</returns>
		public byte getType() {
			if(_type == 0xFF) doReadHeader();
			return _type;
		}

		/// <summary>
		/// Renvoi un byte[] contenant les données du paquet
		/// </summary>
		/// <returns>Données du paquet : byte[]</returns>
		public byte[] getData() {
			if(_data == null) doReadData();
			return _data;
		}

		/// <summary>
		/// Permet de définir le type du paquet
		/// </summary>
		/// <param name="type">byte type défini le type du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setType( byte type ) {
			_type = type;
			return this;
		}

		/// <summary>
		/// Permet de définir la taille des données du paquet
		/// </summary>
		/// <param name="size">Taille des données du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setBodySize( uint size ) {
			_bodySize = size;
			return this;
		}

		/// <summary>
		/// Permet de définir les données du paquet
		/// </summary>
		/// <param name="data">Les données du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setData( byte[] data ) {
			_data = data;
			setBodySize((uint)_data.Length);
			return this;
		}

		/// <summary>
		/// Lis le header et rempli les données membres associés
		/// </summary>
		/// <returns>Le Packet courant</returns>
		private Packet doReadHeader() {
			byte[] buffer = new byte[_headerSize];
			doReadStream(buffer, 0, _headerSize);
			setType(buffer[0]);
			setBodySize((uint)BitConverter.ToInt32(buffer, 1));
			return this;
		}

		/// <summary>
		/// Lis et rempli les données
		/// </summary>
		/// <returns>Le Packet courant</returns>
		private Packet doReadData() {
			_data = new byte[getBodySize()];
			doReadStream(_data, 0, getBodySize());
			return this;
		}

		/// <summary>
		/// Remplit buffer de start et de longueur length en lisant _stream
		/// </summary>
		/// <param name="buffer">Tableau de byte à remplir</param>
		/// <param name="start">Début du remplissage</param>
		/// <param name="length">Taille du remplissage</param>
		private Packet doReadStream( byte[] buffer, uint start, uint length ) {
			int n = 0, r;

			if(!_stream.CanRead)
				throw new Exception("The stream cannot read");

			while(n < length) {
				r = _stream.Read(buffer,(int)(start + n),(int)(length - n));
				if(r == 0)
					throw new Exception("Connection lost");

				n += r;
			}
			return this;	
		}

		public Packet doSend() {
			if(_stream.CanWrite) {
				byte[] sendMessage = new byte[_headerSize + getBodySize()];
				sendMessage[0] = getType();
				byte[] bodySize = BitConverter.GetBytes(getBodySize());
				bodySize.CopyTo(sendMessage, 1);
				getData().CopyTo(sendMessage, _headerSize);
				Console.WriteLine("Envoi de paquet :" + getBodySize() + "octets.");
				_stream.Write(sendMessage, 0, sendMessage.Length);
			}
			return this;
		}

		/// <summary>
		/// Retourne _data : byte[] converti en string UTF8
		/// </summary>
		/// <returns>Les données sous forme de string (UTF8)</returns>
		public string getMessage() {
			return ByteArrayToStr(getData());
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

		public static byte[] StrToByteArray( string str ) {
			System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
			return encoding.GetBytes(str);
		}


	}
}
