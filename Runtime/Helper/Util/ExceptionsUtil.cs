using UnityEngine;
using System;
using System.Collections.Generic;

namespace HyperUnityCommons
{

	public class ExceptionsUtil {

		public static Exception CreateExceptionFormat(string message, params object[] args) {
			string formatMessage = string.Format(message, args);
			return new Exception(formatMessage);
		}

	}

}

