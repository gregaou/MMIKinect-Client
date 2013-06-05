namespace MMIKinect.PplTraining {
	using System;
	using System.Windows;
	using System.Windows.Media.Imaging;
	using MMIKinect.Network;
	class HistogramTraining : ATraining {

		public byte[] _body = null;

		public HistogramTraining() {}

		public override ATraining doTraining() {
			_body = _pplTracker.getBodyImage();
			if(_body == null) throw new TrainingException("Aucun corps détecté");
			return this;
		}

		public override ATraining saveTraining() {
			throw new System.NotImplementedException();
		}

		public override ATraining sendTraining() {
			return sendTraining(PacketType.HISTOGRAMM_TRAINING_REQUEST, _body);
		}
	}
}
