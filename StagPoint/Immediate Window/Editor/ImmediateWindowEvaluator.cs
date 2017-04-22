// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

using StagPoint.Eval;

using UnityEngine;
using UnityEditor;

namespace StagPoint.DeveloperTools
{

	using ScriptEnvironment = StagPoint.Eval.Environment;

	/// <summary>
	/// Contains values and functionality used for runtime script evaluation
	/// </summary>
	internal class ImmediateWindowEvaluator
	{

		#region Static variables

		/// <summary>
		/// Contains all default constants used by the script evaluation system. Can be used to
		/// provide an alias for any desired System.Type, or any constant values that are not
		/// defined by the script grammar.
		/// </summary>
		public static Dictionary<string, object> Constants = new Dictionary<string, object>()
		{
			{ "Random", typeof( UnityEngine.Random ) },
			{ "Type", typeof( System.Type ) },
			{ "Time", typeof( UnityEngine.Time ) },
			{ "Object", typeof( UnityEngine.Object ) },
			{ "GameObject", typeof( UnityEngine.GameObject ) },
			{ "Component", typeof( UnityEngine.Component ) },
			{ "ScriptableObject", typeof( UnityEngine.ScriptableObject ) },
			{ "MonoBehaviour", typeof( UnityEngine.MonoBehaviour ) },
			{ "Rect", typeof( UnityEngine.Rect ) },
			{ "Vector2", typeof( UnityEngine.Vector2 ) },
			{ "Vector3", typeof( UnityEngine.Vector3 ) },
			{ "Vector4", typeof( UnityEngine.Vector4 ) },
			{ "Quaternion", typeof( UnityEngine.Quaternion ) },
			{ "Matrix", typeof( UnityEngine.Matrix4x4 ) },
			{ "Mathf", typeof( UnityEngine.Mathf ) },
			{ "Math", typeof( UnityEngine.Mathf ) },
			{ "Debug", typeof( UnityEngine.Debug ) },
			{ "Application", typeof( UnityEngine.Application ) },
			{ "EditorApplication", typeof( UnityEditor.EditorApplication ) },
			{ "ClientScene", typeof( UnityEngine.Networking.ClientScene ) },  // ADDED
			{ "NetworkManager", typeof( UnityEngine.Networking.NetworkManager ) },  // ADDED
			{ "NetworkServer", typeof( UnityEngine.Networking.NetworkServer ) },  // ADDED
		};

		#endregion

		#region Public properties

		public ScriptEnvironment Environment { get { return this.environment; } }

		#endregion

		#region Private variables

		private ScriptEnvironment environment = null;

		#endregion

		#region Constructor

		static ImmediateWindowEvaluator()
		{

			var assemblies = new List<Assembly>
			{
				typeof( MonoBehaviour ).Assembly,
				typeof( EditorWindow ).Assembly,
				Assembly.GetExecutingAssembly(),
			};

			assemblies.AddRange( AppDomain.CurrentDomain.GetAssemblies().Where( x => x.FullName.StartsWith( "Assembly-CSharp" ) ) );

			foreach( var assembly in assemblies )
			{

				foreach( var type in assembly.GetTypes() )
				{

					if( type.IsGenericType || type.IsSpecialName || !type.IsPublic )
						continue;

					Constants[ type.Name ] = type;

				}

			}

		}

		public ImmediateWindowEvaluator( GameObject target )
		{

			environment = new ScriptEnvironment()
			{
				Constants = ImmediateWindowEvaluator.Constants,
				ResolveEnvironmentVariable = resolveScriptVariables,
				AllowImplicitGlobals = true
			};

			if( target == null )
				return;

			var components = target.GetComponents<MonoBehaviour>();
			for( int i = 0; i < components.Length; i++ )
			{
				if( components[ i ] != null )
				{

					var component = components[ i ];
					var variableName = "$" + component.GetType().Name;

					if( !environment.Variables.ContainsKey( variableName ) )
					{
						var componentVariable = new Variable( variableName, component );
						environment.AddVariable( componentVariable );
					}

				}
			}

			environment.AddVariable( "this", target, typeof( GameObject ) );

			var members = typeof( GameObject ).GetMembers( BindingFlags.Public | BindingFlags.Instance );
			foreach( var member in members )
			{

				if( member.DeclaringType == typeof( object ) )
					continue;

				if( member is FieldInfo || member is PropertyInfo || member is MethodInfo )
				{
					if( !environment.Variables.ContainsKey( member.Name ) )
					{

						if( member is MethodInfo && ( (MethodInfo)member ).IsSpecialName )
							continue;

						environment.AddVariable( new BoundVariable( member.Name, target, member ) );

					}
				}

			}

		}

		#endregion

		#region Public methods

		public VariableBase GetVariable( string name )
		{

			VariableBase result = null;

			environment.Variables.TryGetValue( name, out result );

			return result;

		}

		public object GetConstant( string name )
		{

			object result = null;

			environment.Constants.TryGetValue( name, out result );

			return result;

		}

		public void AddVariable( string name, object value, System.Type type )
		{
			environment.AddVariable( name, value, type );
		}

		public void AddMethod( string name, object target, MethodInfo method )
		{
			var boundVariable = new BoundVariable( name, target, method );
			environment.AddVariable( boundVariable );
		}

		public void AddMethod( string name, System.Type type, string methodName )
		{

			var method = type.GetMethod( methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );
			if( method == null || !method.IsStatic )
				throw new MethodAccessException();

			var boundVariable = new BoundVariable( name, null, method );
			environment.AddVariable( boundVariable );

		}

		public void AddMethod( string name, Func<object> callback )
		{
			AddMethod( name, callback.Target, callback.Method );
		}

		public void AddMethod( string name, Func<object,object> callback )
		{
			AddMethod( name, callback.Target, callback.Method );
		}

		public void AddMethod( string name, Action<object> callback )
		{
			AddMethod( name, callback.Target, callback.Method );
		}

		public void AddMethod( string name, Action callback )
		{
			AddMethod( name, callback.Target, callback.Method );
		}

		public object Evaluate( string script )
		{

			var expression = EvalEngine.Compile( script, environment );
			if( expression == null || expression.Execute == null )
			{
				throw new InvalidOperationException( "Unknown error compiling script" );
			}

			var result = expression.Execute();
			return result;

		}

		#endregion

		#region Private utility methods

		private bool resolveScriptVariables( string name, out VariableBase variable )
		{

			variable = null;

			var agent = Selection.activeGameObject;
			if( agent == null )
			{
				return false;
			}

			if( name.StartsWith( "$" ) )
			{

				var typeName = name.Substring( 1 );

				var component = agent.GetComponent( typeName );
				if( component != null )
				{
					variable = new Variable( name, component );
					return true;
				}

				return false;

			}

			var member = agent.GetType().GetMember( name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).FirstOrDefault();
			if( member is FieldInfo || member is PropertyInfo || member is MethodInfo )
			{
				variable = new BoundVariable( name, agent, member );
				return true;
			}

			return false;

		}

		#endregion

	}

}
