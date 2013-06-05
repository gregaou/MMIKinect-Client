namespace MMIKinect.Network {

	using System;
	using System.Net.Sockets;

	class Packet {

		/// <summary>
		/// Variabe contenant la taille du corps du message
		/// </summary>
		int _bodySize = -1;

		/// <summary>
		/// Données du paquet
		/// </summary>
		private byte[] _data;

		/// <summary>
		/// Id du paquet
		/// </summary>
		private UInt16 _id;

		/// <summary>
		/// Type du paquet
		/// </summary>
		private byte _type;

		/// <summary>
		/// Version du paquet
		/// </summary>
		private byte _version = (byte)PacketVersion.UNDEFINED;

		/// <summary>
		/// Constructeur de la classe Paquet
		/// </summary>
		/// <param name="stream">NetworkStream ouvert sur la socket de communication</param>
		public Packet() {
			_type = (byte)PacketType.UNDEFINED_TYPE;
		}

		/// <summary>
		/// Renvoi la version du paquet
		/// </summary>
		/// <returns>La version du paquet</returns>
		public byte getVersion() {
			if(_version == (byte)PacketVersion.UNDEFINED) { doReadVersion(); }
			return _version;
		}

		/// <summary>
		/// Renvoi le type du paquet
		/// </summary>
		/// <returns>Taille des données du paquet</returns>
		public byte getType() {
			if(_type == (byte)PacketType.UNDEFINED_TYPE) { doReadHeader(); }
			return _type;
		}

		/// <summary>
		/// Renvoi l'id du paquet
		/// </summary>
		/// <returns>Id du paquet</returns>
		public UInt16 getId() {
			if(_id == 0x00) { doReadHeader(); }
			return _id;
		}

		/// <summary>
		/// Renvoi la taille des données du paquet
		/// </summary>
		/// <returns>Taille des données du paquet</returns>
		public int getBodySize() {
			if(_bodySize == -1) { doReadHeader(); }
			return _bodySize;
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
		/// Définit la version du paquet
		/// </summary>
		/// <param name="version">la version du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setVersion( byte version ) {
			_version = version;
			return this;
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
		/// Permet de définir l'identifiant du paquet
		/// </summary>
		/// <param name="id">l'identifiant du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setId( UInt16 id ) {
			_id = id;
			return this;
		}

		/// <summary>
		/// Permet de définir la taille des données du paquet
		/// </summary>
		/// <param name="size">Taille des données du paquet</param>
		/// <returns>L'objet Paquet courant</returns>
		public Packet setBodySize( int size ) {
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
			setBodySize(_data.Length);
			return this;
		}

		/// <summary>
		///  Lis et rempli le champ 'version'
		/// </summary>
		/// <returns>L'objet Paquet courant</returns>
		private Packet doReadVersion() {
			setVersion((byte)ClientTCP.getInstance.getSocket().ReadByte());
			return this;
		}

		/// <summary>
		/// Lis le header et rempli les données membres associés
		/// </summary>
		/// <returns>L'objet Paquet courant</returns>
		/// <exception cref="NetworkException">Version du paquet incorrecte</exception>
		private Packet doReadHeader() {
			if(getVersion() != (byte)PacketVersion.ACTUAL) {
				throw new NetworkException("Wrong packet version :" + getVersion() +
					" given, " + PacketVersion.ACTUAL + " expected");
			}

			byte[] buffer = new byte[(int)PacketDefinition.HEADERSIZE - 1];
			doReadStream(buffer, 0, (int)PacketDefinition.HEADERSIZE - 1);

			setType(buffer[(int)PacketDefinition.TYPESTART-1]);
			setId((UInt16)BitConverter.ToInt16(buffer, (int)PacketDefinition.IDSTART-1));
			setBodySize(BitConverter.ToInt32(buffer, (int)PacketDefinition.BODYSTART-1));

			return this;
		}

		/// <summary>
		/// Lis et rempli les données
		/// </summary>
		/// <returns>L'objet Paquet courant</returns>
		private Packet doReadData() {
			_data = new byte[getBodySize()];
			doReadStream(_data, 0, (uint)getBodySize());
			return this;
		}

		/// <summary>
		/// Remplit buffer de start et de longueur length en lisant _stream
		/// </summary>
		/// <param name="buffer">Tableau de byte à remplir</param>
		/// <param name="start">Début du remplissage</param>
		/// <param name="length">Taille du remplissage</param>
		/// <exception cref="NetworkException">Impossible de lire depuis le stream</exception>
		/// <exception cref="NetworkException">Connection perdue</exception>
		/// <returns>L'objet Paquet courant</returns>
		private Packet doReadStream( byte[] buffer, uint start, uint length ) {
			int n = 0, r;

			if(!ClientTCP.getInstance.getSocket().CanRead)
				throw new Exception("The stream cannot read");

			while(n < length) {
				r = ClientTCP.getInstance.getSocket().Read(buffer, (int)(start + n), (int)(length - n));
				Console.WriteLine(r + " caractères lus");
				if(r == 0)
					throw new Exception("Connection lost");

				n += r;
			}
			return this;
		}

		/// <summary>
		/// Envoie le packet correctement formaté
		/// </summary>
		/// <returns>L'objet Paquet courant</returns>
		public Packet doSend() {
			if(ClientTCP.getInstance.getSocket().CanWrite && ClientTCP.getInstance.Connected) {
				byte[] sendMessage = new byte[(int)PacketDefinition.HEADERSIZE + getBodySize()];

				sendMessage[0] = (byte)PacketVersion.ACTUAL;
				sendMessage[1] = getType();

				byte[] id = BitConverter.GetBytes(getId());
				id.CopyTo(sendMessage, (int)PacketDefinition.IDSTART);

				byte[] bodySize = BitConverter.GetBytes(getBodySize());
				bodySize.CopyTo(sendMessage, (int)PacketDefinition.BODYSTART);

				getData().CopyTo(sendMessage, (int)PacketDefinition.HEADERSIZE);

				Console.WriteLine("Envoi de paquet :" + getBodySize() + "octets.");
				ClientTCP.getInstance.getSocket().Write(sendMessage, 0, sendMessage.Length);
				Console.WriteLine("Paquet envoyé");
			}
			return this;
		}
	}
}
