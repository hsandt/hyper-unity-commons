using UnityEngine;
using System;
using System.Collections.Generic;
#if !(UNITY_WP8 || UNITY_WSA)
using System.Runtime.Serialization;
#endif

namespace HyperUnityCommons
{

	public class ResourceNotFoundException : Exception {

		private string path;

		protected ResourceNotFoundException() : base() {}

		// IMPROVE: support Type systemTypeInstance argument
		public ResourceNotFoundException(string path) :
		   base(string.Format("Resource \"{0}\" not found.", path))
		{
		   this.path = path;
		}

		public ResourceNotFoundException(string path, string message)
		   : base(message)
		{
		   this.path = path;
		}

		public ResourceNotFoundException(string path, string message, Exception innerException) :
		   base(message, innerException)
		{
		   this.path = path;
		}

	#if !(UNITY_WP8 || UNITY_WSA)
		protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
		   : base(info, context)
		{ }
	#endif

		public string Path { get { return path; } }

	}

	public class UninitializedSingletonException : Exception {

		private string singletonName;

		protected UninitializedSingletonException() : base() {}

		public UninitializedSingletonException(string singletonName) :
		   base(string.Format("Singleton {0} has no instance initialized. Ensure that the game object with the {0} component has an Awake() method with \"Instance = this\".", singletonName))
		{
		   this.singletonName = singletonName;
		}

		public UninitializedSingletonException(string singletonName, string message)
		   : base(message)
		{
			this.singletonName = singletonName;
		}

		public UninitializedSingletonException(string singletonName, string message, Exception innerException) :
		   base(message, innerException)
		{
			this.singletonName = singletonName;
		}

	#if !(UNITY_WP8 || UNITY_WSA)
		protected UninitializedSingletonException(SerializationInfo info, StreamingContext context)
		   : base(info, context)
		{ }
	#endif

		public string SingletonName { get { return singletonName; } }

	}

	public class ReinitializeSingletonException : Exception {

		private string singletonName;

		protected ReinitializeSingletonException() : base() {}

		public ReinitializeSingletonException(string singletonName) :
		   base(string.Format("Singleton {0} has already an instance initialized. Please make sure there is only one game object with a {0} component.", singletonName))
		{
		   this.singletonName = singletonName;
		}

		public ReinitializeSingletonException(string singletonName, string message)
		   : base(message)
		{
			this.singletonName = singletonName;
		}

		public ReinitializeSingletonException(string singletonName, string message, Exception innerException) :
		   base(message, innerException)
		{
			this.singletonName = singletonName;
		}

	#if !(UNITY_WP8 || UNITY_WSA)
		protected ReinitializeSingletonException(SerializationInfo info, StreamingContext context)
		   : base(info, context)
		{ }
	#endif

		public string SingletonName { get { return singletonName; } }

	}

}
