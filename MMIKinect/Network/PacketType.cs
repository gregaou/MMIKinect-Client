namespace MMIKinect.Network {
	/// <summary>
	/// Défini les différents types de paquets existants
	/// </summary>
	enum PacketType {
		/// <summary>
		/// Type de paquet indéfini
		/// </summary>
		UNDEFINED_TYPE = 0x00,
		/// <summary>
		/// Paquet de type audio
		/// </summary>
		AUDIO_TYPE = 0x10,
		/// <summary>
		/// Paquet de type video
		/// </summary>
		VIDEO_TYPE = 0x20,
		/// <summary>
		/// Paquet de type histogramm
		/// </summary>
		HISTOGRAMM_TYPE = 0x30,
		/// <summary>
		/// Paquet de type broadcast
		/// </summary>
		BROADCAST_TYPE = 0xF0,

		/// <summary>
		/// Action du paquet indéfini
		/// </summary>
		UNDEFINED_ACTION = 0x00,
		/// <summary>
		/// Requête pour entrainement
		/// </summary>
		TRAINING_REQUEST = 0x01,
		/// <summary>
		///  pour entrainement
		/// </summary>
		TRAINING_RESULT = 0x02,
		/// <summary>
		/// Requête pour scoring
		/// </summary>
		SCORING_REQUEST = 0x03,
		/// <summary>
		/// Résultat pour scoring
		/// </summary>
		SCORING_RESULT = 0x04,
		/// <summary>
		/// Requête pour listing
		/// </summary>
		LISTING_REQUEST = 0x05,
		/// <summary>
		/// Résultat pour listing
		/// </summary>
		LISTING_RESULT = 0x06,

		/// <summary>
		/// Requête pour entrainement audio
		/// </summary>
		AUDIO_TRAINING_REQUEST = AUDIO_TYPE | TRAINING_REQUEST,
		/// <summary>
		/// Résultat pour entrainement audio
		/// </summary>
		AUDIO_TRAINING_RESULT = AUDIO_TYPE | TRAINING_RESULT,
		/// <summary>
		/// Requête pour scoring audio
		/// </summary>
		AUDIO_SCORING_REQUEST = AUDIO_TYPE | SCORING_REQUEST,
		/// <summary>
		/// Résultat pour scoring audio
		/// </summary>
		AUDIO_SCORING_RESULT = AUDIO_TYPE | SCORING_RESULT,
		/// <summary>
		/// Requête pour listing audio
		/// </summary>
		AUDIO_LISTING_REQUEST = AUDIO_TYPE | LISTING_REQUEST,
		/// <summary>
		/// Résultat pour listing audio
		/// </summary>
		AUDIO_LISTING_RESULT = AUDIO_TYPE | LISTING_RESULT,

		/// <summary>
		/// Requête pour entrainement video
		/// </summary>
		VIDEO_TRAINING_REQUEST = VIDEO_TYPE | TRAINING_REQUEST,
		/// <summary>
		/// Résultat pour entrainement video
		/// </summary>
		VIDEO_TRAINING_RESULT = VIDEO_TYPE | TRAINING_RESULT,
		/// <summary>
		/// Requête pour scoring video
		/// </summary>
		VIDEO_SCORING_REQUEST = VIDEO_TYPE | SCORING_REQUEST,
		/// <summary>
		/// Résultat pour scoring video
		/// </summary>
		VIDEO_SCORING_RESULT = VIDEO_TYPE | SCORING_RESULT,
		/// <summary>
		/// Requête pour listing video
		/// </summary>
		VIDEO_LISTING_REQUEST = VIDEO_TYPE | LISTING_REQUEST,
		/// <summary>
		/// Résultat pour listing video
		/// </summary>
		VIDEO_LISTING_RESULT = VIDEO_TYPE | LISTING_RESULT,

		/// <summary>
		/// Requête pour entrainement hitstogramm
		/// </summary>
		HISTOGRAMM_TRAINING_REQUEST = HISTOGRAMM_TYPE | TRAINING_REQUEST,
		/// <summary>
		/// Résultat pour entrainement hitstogramm
		/// </summary>
		HISTOGRAMM_TRAINING_RESULT = HISTOGRAMM_TYPE | TRAINING_RESULT,
		/// <summary>
		/// Requête pour scoring hitstogramm
		/// </summary>
		HISTOGRAMM_SCORING_REQUEST = HISTOGRAMM_TYPE | SCORING_REQUEST,
		/// <summary>
		/// Résultat pour scoring hitstogramm
		/// </summary>
		HISTOGRAMM_SCORING_RESULT = HISTOGRAMM_TYPE | SCORING_RESULT,
		/// <summary>
		/// Requête pour listing hitstogramm
		/// </summary>
		HISTOGRAMM_LISTING_REQUEST = HISTOGRAMM_TYPE | LISTING_REQUEST,
		/// <summary>
		/// Résultat pour listing hitstogramm
		/// </summary>
		HISTOGRAMM_LISTING_RESULT = VIDEO_TYPE | LISTING_RESULT
	};
}
