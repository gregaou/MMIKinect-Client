namespace MMIKinect.PplTraining {
	using System;
	using MMIKinect.Network;
	class FaceTraining : ATraining {

		public byte[] _face = null;

		public FaceTraining() { }

		public override ATraining doTraining() {
			_face = _pplTracker.getFaceImage();
			if(_face == null) throw new TrainingException("Aucune tête détectée");
			return this;
		}

		public override ATraining saveTraining() {
			throw new System.NotImplementedException();
		}

		public override ATraining sendTraining() {
			return sendTraining(PacketType.VIDEO_TRAINING_REQUEST, _face);
		}
	}
}
