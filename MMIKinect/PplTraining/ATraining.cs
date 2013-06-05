namespace MMIKinect.PplTraining {
	using System;
	using System.IO;
	using System.Text;
	using System.Windows.Media.Imaging;
	using MMIKinect.Network;
	using MMIKinect.PplTracking;
	abstract class ATraining {

		/// <summary>
		/// La personne à entrainer
		/// </summary>
		protected PplTracker _pplTracker;

		/// <summary>
		/// Nom de la personne à entrainer
		/// </summary>
		protected string _pplName;

		/// <summary>
		/// Défini la personne à entrainer
		/// </summary>
		/// <param name="p">Personne</param>
		/// <returns>L'Objet courant</returns>
		public ATraining setPplTracker( PplTracker p ) { 
			_pplTracker = p;
			return this; 
		}

		/// <summary>
		/// Défini le nom de la personne à entrainer
		/// </summary>
		/// <param name="name">Nom de la personne</param>
		/// <returns>L'Objet Courant</returns>
		public ATraining setPplName( string name ) {	_pplName = name; return this; }

		/// <summary>
		/// Récupère le nom de la personne à entrainer
		/// </summary>
		/// <returns>Nom de la personne</returns>
		protected string getPplName() {
			if(_pplName.Length == 0) throw new TrainingException("Nom non défini pour l'enregistrement");
			return _pplName;
		}

		/// <summary>
		/// Récupère la personne à entrainer
		/// </summary>
		/// <returns>La personne</returns>
		protected string getPplTracker() {
			if(_pplTracker == null ) throw new TrainingException("Personne non définie pour l'enregistrement");
			return _pplName;
		}

		protected void saveImage( string filename, BitmapSource image5 ) {
			if(filename != string.Empty) {
				using(FileStream stream5 = new FileStream(filename, FileMode.Create)) {
					PngBitmapEncoder encoder5 = new PngBitmapEncoder();
					encoder5.Frames.Add(BitmapFrame.Create(image5));
					encoder5.Save(stream5);
					stream5.Close();
				}
			}
		}

		abstract public ATraining doTraining();
		abstract public ATraining saveTraining();
		abstract public ATraining sendTraining();

		public ATraining sendTraining(PacketType pt, byte[] data) {
			byte[] sizeP = BitConverter.GetBytes((UInt16)_pplName.Length);
			byte[] nameP = Encoding.ASCII.GetBytes(_pplName);

			byte[] dataPacket = new byte[sizeP.Length + nameP.Length + data.Length];

			sizeP.CopyTo(dataPacket, 0);
			nameP.CopyTo(dataPacket, sizeP.Length);
			data.CopyTo(dataPacket, sizeP.Length + nameP.Length);
			
			Packet p = new Packet();
			p.setId(_pplTracker.getId())
				.setType((byte)pt)
				.setVersion((int)PacketVersion.ACTUAL)
				.setData(dataPacket)
				.doSend();
			return this;
		}
	}
}
