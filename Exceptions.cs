using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

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

	protected ResourceNotFoundException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public string Path { get { return path; } }

}

public class UnassignedReferenceException : Exception {

	private MonoBehaviour script;
	private string referenceName;

	protected UnassignedReferenceException() : base() {}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName) :
	   base(string.Format("Script {0} has unassigned reference {1}. Please assign it in the inspector.", script, referenceName))
	{
	   this.script = script;
	   this.referenceName = referenceName;
	}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName, string message)
	   : base(message)
	{
		this.script = script;
		this.referenceName = referenceName;
	}

	public UnassignedReferenceException(MonoBehaviour script, string referenceName, string message, Exception innerException) :
	   base(message, innerException)
	{
		this.script = script;
		this.referenceName = referenceName;
	}

	protected UnassignedReferenceException(SerializationInfo info, StreamingContext context)
	   : base(info, context)
	{ }

	public MonoBehaviour Script { get { return script; } }
	public string ReferenceName { get { return referenceName; } }

}
