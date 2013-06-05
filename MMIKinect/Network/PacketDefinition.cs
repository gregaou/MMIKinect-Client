namespace MMIKinect.Network {
	enum PacketDefinition {
		/// <summary>
		/// Taille du header d'un paquet
		/// </summary>
		HEADERSIZE = 8,
		/// <summary>
		/// Taille utilisée pour stocker la version d'un paquet
		/// </summary>
		VERSIONSIZE = 1,
		/// <summary>
		/// Taille utilisée pour stocker le type d'un paquet
		/// </summary>
		TYPESIZE = 1,
		/// <summary>
		/// Taille utilisée pour stocker l'id d'un paquet
		/// </summary>
		IDSIZE = 2,
		/// <summary>
		/// Taille utilisée pour stocker la taille du corps paquet
		/// </summary>
		BODYSIZE = 4,
		/// <summary>
		/// Début du numéro de version dans le header
		/// </summary>
		VERSIONSTART = 0,
		/// <summary>
		/// Début du numéro de type dans le header
		/// </summary>
		TYPESTART = 1,
		/// <summary>
		/// Début du l'id dans le header
		/// </summary>
		IDSTART = 2,
		/// <summary>
		/// Début de la taille du corps dans le header
		/// </summary>
		BODYSTART=4
	};
}