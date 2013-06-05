namespace MMIKinect.PplTraining {
	class TrainingException : System.ApplicationException {
		public TrainingException() { }
		public TrainingException( string message ) { }
		public TrainingException( string message, System.Exception inner ) { }

		// Constructor needed for serialization 
		// when exception propagates from a remoting server to the client.
		protected TrainingException( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ) { }
	}
}
