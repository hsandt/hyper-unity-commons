using UnityEngine;
using System;
using System.Collections.Generic;

public static class ResourcesUtil {

	public static T Load<T>(string path) where T : class {
		return Resources.Load(path) as T;
	}

	public static T Load<T>(string path, Type systemTypeInstance) where T : class {
		return Resources.Load(path, systemTypeInstance) as T;
	}

	public static T LoadOrFail<T>(string path) where T : class {
		T resource = Load<T>(path);
		if (resource == null) {
			throw new ResourceNotFoundException(path);
			throw ExceptionsUtil.CreateExceptionFormat("No resource \"{0}\" found", path);
		}
		return resource;
	}

	public static T LoadOrFail<T>(string path, Type systemTypeInstance) where T : class {
		T resource = Load<T>(path, systemTypeInstance);
		if (resource == null) {
			Debug.LogErrorFormat("No resource \"{0}\" of type {1} found", path, systemTypeInstance);
		}
		return resource;
	}

}
