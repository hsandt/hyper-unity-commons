using UnityEngine;
using System;
using System.Collections.Generic;

namespace HyperUnityCommons
{

	public static class ResourcesUtil {

		public static T LoadOrFail<T>(string path) where T : UnityEngine.Object {
			T resource = Resources.Load<T>(path);
			if (resource == null) {
				throw new ResourceNotFoundException(path);
			}
			return resource;
		}

	}

}

