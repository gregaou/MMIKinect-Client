namespace MMIKinect.Network {
	class NetworkException : System.ApplicationException {
		public NetworkException() { }
		public NetworkException( string message ) { }
		public NetworkException( string message, System.Exception inner ) { }

		// Constructor needed for serialization 
		// when exception propagates from a remoting server to the client.
		protected NetworkException( System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context ) { }
	}
}
