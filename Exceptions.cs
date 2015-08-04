using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

public class ResourceNotFoundException : Exception {

	private string path;

	protected ResourceNotFoundException() : base() {}

	// IMPROVE: support Type systemTypeInstance argument
	public ResourceNotFoundException(string path) :
	   base(string.Format("Resource \"{0}\" not found", path))
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

	protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public string Path { get { return path; } }

}
