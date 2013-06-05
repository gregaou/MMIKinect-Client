namespace MMIKinect.PplTraining {
	using System;
	using MMIKinect.Network;
	class AudioTraining : ATraining {

		private byte[] _audio = null;

		public override ATraining doTraining() {
			if(_audio == null) throw new TrainingException("Aucun contenu audio");
			return this;
		}

		public AudioTraining setAudio( byte[] audio ) {
			_audio = audio;
			return this;
		}

		public override ATraining saveTraining() {
			throw new NotImplementedException();
		}

		public override ATraining sendTraining() {
			return sendTraining(PacketType.AUDIO_TRAINING_REQUEST, _audio);
		}
	}
}
