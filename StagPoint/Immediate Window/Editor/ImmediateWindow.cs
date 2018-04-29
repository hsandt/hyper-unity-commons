// Copyright (c) 2014 StagPoint Consulting

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEditor;

namespace StagPoint.DeveloperTools
{

	using StagPoint.Eval;
	using StagPoint.Eval.Parser;

	[InitializeOnLoad]
	public class ImmediateWindow : EditorWindow
	{

		// const string WELCOME_KEY = "immediate-window-welcome";

		#region Static constructor

		static ImmediateWindow()
		{


			// ERROR:
			// GetBool is not allowed to be called from a ScriptableObject constructor (or instance field initializer), call it in OnEnable instead. Called from ScriptableObject 'ImmediateWindow'.
			// See "Script Serialization" page in the Unity Manual for further details.
			// SOLUTION: do this in OnEnable (welcome message will only appear when accessing ImmediateWindow once) or find how to trigger a Welcome window properly (other plugins such as DOTween do it)

			// MOVED TO OnEnable

			// var hasShownWelcome = EditorPrefs.GetBool( WELCOME_KEY, false );
			// if( hasShownWelcome )
			// 	return;

			// EditorPrefs.SetBool( WELCOME_KEY, true );

			// if( !EditorUtility.DisplayDialog( "Immediate Window", "You have successfully installed the Immediate Window Extension.\n\nDo you wish to view the online QuickStart guide?", "Yes", "No" ) )
			// 	return;

			// ShowQuickstart();

		}

		#endregion

		#region Tool menu support

		public static ImmediateWindow Instance = null;

		// [MenuItem( "Tools/StagPoint/Immediate Window/Open Immediate Window", false, 0 )]
		// [MenuItem( "Window/Immediate Window _F5" )]
		[MenuItem( "Debug/Immediate Window _F5" )]
		public static void OpenScriptWindow()
		{

			var window = OpenWindow();
			window.minSize = new Vector2( 800, 200 );

			window.Focus();

		}

		// StagPoint website is dead (2017-04-22)
		/*
		[MenuItem( "Tools/StagPoint/Immediate Window/Online Help", false, 1 )]
		public static void ShowQuickstart()
		{
			Help.BrowseURL( "http://www.stagpoint.com/immediate-window/quickstart/" );
		}
		*/

		private static ImmediateWindow OpenWindow()
		{
			var window = Instance = GetWindow( typeof( ImmediateWindow ) ) as ImmediateWindow;
			window.titleContent.text = "Immediate";

			return window;

		}

		#endregion

		#region Private variables

		private const int MAX_OUTPUT_LINES = 1024;

		private static GUIStyle outputAreaStyle = null;
		private static GUIStyle outputLineStyle = null;

		private static Color darkGray = new Color32(85, 85, 85, 255); // ADDED: Animation Curve Editor background (#555555)
		private static Texture2D darkGrayTexture; // ADDED: texture for outputAreaStyle

		private static List<string> output = new List<string>()
		{
			"> Enter your script expression in the text field below and press the <b>ENTER</b> key to run.",
			"> Enter <b>help()</b> for a quick introduction."
		};

		private static int errorLevel = 0;

		private static ImmediateWindowEvaluator globalEvaluator;

		[NonSerialized]
		private string command = "";

		private Dictionary<GameObject, ImmediateWindowEvaluator> cachedEvaluators = new Dictionary<GameObject, ImmediateWindowEvaluator>();
		private ImmediateWindowEvaluator evaluator = null;
		private CommandHistory history = new CommandHistory();
		private AutoCompleteField prompt;
		private Vector2 scrollPosition = Vector2.zero;
		private int fontSize = 12;

		#endregion

		#region EditorWindow events

		public void OnGUI()
		{

			showOutput();

			GUI.enabled = EditorWindow.focusedWindow == this;
			editCommand();
			GUI.enabled = true;

			if( Event.current.isMouse || Event.current.isKey )
			{
				Repaint();
			}

		}

		public void OnEnable()
		{

			this.titleContent.text = "Immediate";
			this.minSize = new Vector2( 600, 200 );
			this.wantsMouseMove = true;

			initializeEvaluator();
			initializeAutoComplete();
			Repaint();

			// WELCOME MESSAGE moved here
			// but commented out since StagPoint website is dead
			/*
			var hasShownWelcome = EditorPrefs.GetBool( WELCOME_KEY, false );
			if( hasShownWelcome )
				return;

			EditorPrefs.SetBool( WELCOME_KEY, true );

			if( !EditorUtility.DisplayDialog( "Immediate Window", "You have successfully installed the Immediate Window Extension.\n\nDo you wish to view the online QuickStart guide?", "Yes", "No" ) )
				return;

			ShowQuickstart();
			*/
		}

		public void OnFocus()
		{
			initializeEvaluator();
			initializeAutoComplete();
		}

		public void OnSelectionChange()
		{
			initializeEvaluator();
			initializeAutoComplete();
			Repaint();
		}

		#endregion

		private void initializeAutoComplete()
		{

			if( this.prompt != null )
				return;

			this.prompt = new AutoCompleteField()
			{
				onKeyDownPreview = ( Event evt, ref string text ) =>
				{

					if( evt.keyCode == KeyCode.Return || evt.keyCode == KeyCode.KeypadEnter )
					{
						logMessage( string.Format( "> <b>{0}</b>", text ) );
						this.history.Add( text );
						evaluateExpression( text );
						text = string.Empty;
					}

					else if( evt.keyCode == KeyCode.UpArrow )
					{
						evt.Use();
						text = history.Previous();
					}
					else if( evt.keyCode == KeyCode.DownArrow )
					{
						evt.Use();
						text = history.Next();
					}
					else if( evt.keyCode == KeyCode.Escape )
					{
						evt.Use();
						text = string.Empty;
					}

				}
			};

		}

		private void editCommand()
		{

			initializeEvaluator();
			initializeAutoComplete();

			GUILayout.BeginHorizontal();
			{

				command = prompt.TextField( command, evaluator );

				if( GUILayout.Button( "Execute", GUILayout.Width( 100 ) ) )
				{
					evaluateExpression( this.command );
					this.command = "";
				}

			}
			GUILayout.EndHorizontal();

		}

		private void logMessage( string message )
		{

			if( output.Count > MAX_OUTPUT_LINES )
			{
				output.RemoveAt( 0 );
			}

			// NOTE: Had to replace tab character due to frustrating Unity Editor crash. Not sure why it hates
			// tabs in rich text outputs.
			output.Add( message.Replace( "\t", "   " ) );

			scrollPosition = new Vector2( 0, float.MaxValue );

			Repaint();

		}

		private void logMessage( string message, int warningLevel )
		{
			var color = "white";
			if( warningLevel == 1 )
				color = "yellow";
			else if( warningLevel == 2 )
				color = "orange";

			logMessage( string.Format( "<color={1}>{0}</color>", message, color ) );

		}

		private void executeCommand()
		{

			command = command.Trim();
			if( string.IsNullOrEmpty( command ) )
				return;

			this.scrollPosition.y = float.MaxValue;

			history.Add( command );
			logMessage( "> " + command );
			evaluateExpression( command );

			logMessage( "<size=5> </size>" );
			initializeAutoComplete();

			command = string.Empty;

		}

		private void evaluateExpression( string expression )
		{

			try
			{

				expression = expression.Trim();
				if( string.IsNullOrEmpty( expression ) )
					return;

				// Save Undo information about the object before proceeding
				if( !Application.isPlaying && Selection.activeGameObject != null )
				{
					Undo.RegisterFullObjectHierarchyUndo( Selection.activeGameObject, "Evaluate Script" );
				}

				// Evaluate the script expression
				var result = evaluator.Evaluate( expression );
				if( result == null )
				{
					logMessage( "\tExpression did not return a value" );
				}
				else
				{
					// Output the results. Note that float values are handled differently in order to retain
					// higher floating point precision than the standard float.ToString() call
					if( result is float || result is double )
					{
						logMessage( string.Format( "\t<b>{0:G}</b>", result ) );
					}
					else
					{
						logMessage( string.Format( "\t<b>{0}</b>", result ) );
					}
				}

			}
			catch( Exception err )
			{
				if( errorLevel == 0 )
					logMessage( "\t" + err.Message, 2 );
				else
					logMessage( "\t" + err.ToString(), 2 );
			}

		}

		private void initializeEvaluator()
		{

			var target = Selection.activeGameObject;

			if( target != null )
			{

				if( cachedEvaluators.TryGetValue( target, out this.evaluator ) )
					return;

				this.evaluator = new ImmediateWindowEvaluator( target );
				cachedEvaluators[ target ] = this.evaluator;

			}
			else
			{

				if( globalEvaluator != null )
				{
					this.evaluator = globalEvaluator;
					return;
				}

				globalEvaluator = new ImmediateWindowEvaluator( null );
				this.evaluator = globalEvaluator;

			}

			evaluator.AddMethod( "fields", ( object value ) =>
			{
				showProperties( 1, "Fields", getProperties( value ), new List<object>() );
				return string.Empty;
			} );

			evaluator.AddMethod( "methods", ( object value ) =>
			{
				showProperties( 1, "Methods", getMethods( value ), new List<object>() );
				return string.Empty;
			} );

			evaluator.AddMethod( "dump", ( object value ) =>
			{
				showProperties( 1, "Dump", value, new List<object>() );
				return string.Empty;
			} );

			evaluator.AddMethod( "find", this.GetType(), "findObject" );

			evaluator.AddMethod( "env", () =>
			{

				var variables = evaluator.Environment.Variables;
				var keys = variables.Keys.ToList();
				keys.Sort();

				foreach( var key in keys )
				{

					var variable = variables[ key ];
					if( variable is BoundVariable )
					{
						var bound = variable as BoundVariable;
						if( bound.Member is MethodInfo )
							logMessage( string.Format( "\t{0} (Method) : {1}", key, ( (MethodInfo)bound.Member ).ReturnType ) );
						else
							logMessage( string.Format( "\t{0} : {1}", key, variable.Type ) );
					}
					else
					{
						logMessage( string.Format( "\t{0} : {1}", key, variable.Type ) );
					}

				}

				return string.Empty;

			} );

			evaluator.AddMethod( "help", () =>
			{
				showHelp();
				return string.Empty;
			} );

			evaluator.AddMethod( "clear", () =>
			{
				clearOutput();
				return string.Empty;

			} );

			evaluator.AddMethod( "errorlevel", ( object value ) =>
			{

				if( !( value is int ) )
				{
					logMessage( "\tYou must supply an integer value of either 0 or 1", 2 );
					return string.Empty;
				}

				errorLevel = Mathf.Max( 0, Mathf.Min( 1, (int)value ) );

				return string.Format( "errorlevel = {0}", errorLevel );

			} );

		}

		private void clearOutput()
		{

			output = new List<string>()
				{
					"> Enter your script expression in the text field below and press the <b>ENTER</b> key to run.",
					"> Enter <b>help()</b> for a quick introduction."
				};

			Repaint();

		}

		internal static GameObject findObject( string name )
		{
			return GameObject.Find( name );
		}

		private static MemberInfo[] getProperties( object variable )
		{

			if( variable == null )
				return null;

			if( variable is System.Type )
			{
				return ( (System.Type)variable )
					.GetMembers( BindingFlags.Public | BindingFlags.Static )
					.Where( x => ( x is FieldInfo || x is PropertyInfo ) )
					.OrderBy( x => x.Name )
					.ToArray();
			}

			return variable.GetType()
					.GetMembers( BindingFlags.Public | BindingFlags.Instance )
					.Where( x => ( x is FieldInfo || x is PropertyInfo ) )
					.OrderBy( x => x.Name )
					.ToArray();

		}

		private static MethodInfo[] getMethods( object variable )
		{

			if( variable == null )
				return null;

			if( variable is System.Type )
			{
				return ( (System.Type)variable )
					.GetMethods( BindingFlags.Public | BindingFlags.Static )
					.Where( x => !x.IsSpecialName && x.GetGenericArguments().Length == 0 )
					.OrderBy( x => x.Name )
					.ToArray();
			}

			var variableType = variable.GetType();

			return variable.GetType()
				.GetMethods( BindingFlags.Public | BindingFlags.Instance )
				.Where( x => x.DeclaringType == variableType && !x.IsSpecialName && x.GetGenericArguments().Length == 0 )
				.OrderBy( x => x.Name )
				.ToArray();

		}

		private void showProperties( int indent, string name, object result, List<object> cycleCheck )
		{

			if( indent > 10 )
				return;

			var prefix = new string( '\t', indent ) + string.Format( "<b>{0}</b>", name );

			if( result == null )
			{
				logMessage( prefix + ": (null)" );
				return;
			}

			if( result is MethodInfo )
			{
				logMessage( prefix + ": " + result.ToString() );
				return;
			}

			var resultType = result.GetType();

			var toStringMethod = resultType.GetMethod( "ToString", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null );
			var hasCustomToString = indent > 1 && toStringMethod != null && toStringMethod.DeclaringType == resultType;

			if( resultType.IsPrimitive || resultType.IsEnum || hasCustomToString )
			{
				var text = result.ToString().Replace( "\n", ", " );
				logMessage( prefix + ": " + text.Trim() );
				return;
			}

			logMessage( prefix + ": (" + resultType.FullName + ")" );

			if( cycleCheck.Contains( result ) )
				return;

			cycleCheck.Add( result );

			try
			{

				if( result is IList )
				{

					var list = (IList)result;

					for( int i = 0; i < list.Count; i++ )
					{
						showProperties( indent + 1, i.ToString(), list[ i ], cycleCheck );
					}

				}
				else if( result is IEnumerable && !( result is UnityEngine.Component ) )
				{

					var idx = 0;
					foreach( var item in ( (IEnumerable)result ) )
					{
						showProperties( indent + 1, idx.ToString(), item, cycleCheck );
						idx += 1;
					}

				}
				else if( !resultType.IsPrimitive )
				{

					var members = result.GetType().GetMembers( BindingFlags.Instance | BindingFlags.Public );
					foreach( var member in members )
					{

						if( member.DeclaringType != resultType )
							continue;

						object memberValue = null;
						if( member is FieldInfo )
						{
							memberValue = ( (FieldInfo)member ).GetValue( result );
							showProperties( indent + 1, member.Name, memberValue, cycleCheck );
						}
						else if( member is PropertyInfo )
						{

							var property = (PropertyInfo)member;
							if( !property.CanRead )
								continue;

							if( property.GetIndexParameters().Length != 0 )
								continue;

							try
							{
								memberValue = property.GetValue( result, null );
								showProperties( indent + 1, member.Name, memberValue, cycleCheck );
							}
							catch { }

						}

					}

				}

			}
			catch( Exception err )
			{
				logMessage( new string( '\t', indent ) + err.Message, 2 );
			}

		}

		private void showHelp()
		{

			string help = @"This window allows you to execute script commands on whichever GameObject is selected in the Hierarchy pane.
To do so, you must first select a GameObject. Then enter your command in the textbox below and press the <b>ENTER</b> key or the
<b>Execute</b> button.

You may also execute static methods on any Unity type or custom type defined in your project, provided that they are in the global namespace.
This allows you to use functions such as <b>Vector3.Distance()</b>, <b>Mathf.Max</b>, and so on. Also note that all public static fields
and properties are available, such as <b>Mathf.PI</b>.

When a GameObject is selected, you can access it via the <b>this</b> keyword (such as <b>this.transform</b>) and all GameObject properties
and fields will be available to your script commands as variables. You can quickly access any GameObject's components by prefixing the
component's type with a <b>$</b> symbol. For example, you can access the GameObject's Transform using <b>$Transform</b>.

You can use any valid script expression, such as <b>this.transform.position = new Vector3( 100, 0, 25 )</b>, etc. You can declare temporary
variables using the syntax <b>var variable = value</b>.

Auto-complete can be shown with the <b>CTRL-J</b> key combo, and can be hidden with the <b>ESC</b> key.

You can also use the following built-in helper functions:

<b>help()</b> - Shows this help text
<b>clear()</b> - Clears the immediate window
<b>dump( target )</b> - Prints the target's data as an indented hierarchical listing
<b>env()</b> - Displays information about the current script environment (all defined variables)
<b>fields( target )</b> - Shows the list of fields and properties that can be accessed on the target
<b>find( name )</b> - Finds any GameObject in the scene with the indicated name
<b>methods( target )</b> - Shows the list of methods that can be called on the target
";

			foreach( var line in help.Split( '\n' ) )
			{
				logMessage( "\t" + line );
			}

		}

		// ADDED from https://forum.unity3d.com/threads/giving-unitygui-elements-a-background-color.20510/#post-430604
		private static Texture2D MakeTex(int width, int height, Color col)
	    {
	        Color[] pix = new Color[width*height];

	        for(int i = 0; i < pix.Length; i++)
	            pix[i] = col;

	        Texture2D result = new Texture2D(width, height);
	        result.SetPixels(pix);
	        result.Apply();

	        return result;
	    }

		private void showOutput()
		{

			var textColor = EditorGUIUtility.isProSkin ? new Color( 0.75f, 0.75f, 0.75f, 1f ) : Color.white;

			if( outputAreaStyle == null )
			{
				// Style "AnimationCurveEditorBackground" cannot be found, and the line below with GetStyle doesn't work either.
				// (it is present in ParticleSystemCurveEditor, but apparently now inaccessible)
				// Instead, pick some style as a base and set the background texture directly with the color I picked from the Animation Curve Editor
				// See all styless available in Unity decompiled: EditorStyles.cs
				// outputAreaStyle = new GUIStyle( GUI.skin.GetStyle("AnimationCurveEditorBackground") );
				// outputAreaStyle = new GUIStyle( (GUIStyle)"AnimationCurveEditorBackground" );
				outputAreaStyle = new GUIStyle( GUI.skin.GetStyle("ControlLabel") );
				outputAreaStyle.padding = new RectOffset( 5, 5, 5, 5 );
				// ADDED: compute dark gray texture from Animation Curve Editor background (#555555)
				if (darkGrayTexture == null)
					darkGrayTexture = MakeTex(1, 1, darkGray);
				outputAreaStyle.normal.background = darkGrayTexture;

				outputLineStyle = new GUIStyle( (GUIStyle)"IN Label" );
				outputLineStyle.normal.textColor = textColor;
				outputLineStyle.richText = true;
				outputLineStyle.wordWrap = false;

			}

			var evt = Event.current;
			if( evt.type == EventType.ScrollWheel && ( evt.control || evt.command ) )
			{
				this.fontSize -= (int)Mathf.Sign( evt.delta.y );
				this.fontSize = Mathf.Clamp( fontSize, 10, 22 );
			}
			else if( evt.type == EventType.KeyDown && evt.keyCode == KeyCode.Alpha0 && evt.control )
			{
				this.fontSize = 0;
			}

			outputLineStyle.fontSize = this.fontSize;

			try
			{

				this.scrollPosition = GUILayout.BeginScrollView( this.scrollPosition, outputAreaStyle, GUILayout.Height( Screen.height - 50 ) );

				for( int i = 0; i < output.Count; i++ )
				{
					if( string.IsNullOrEmpty( output[ i ] ) )
						GUILayout.Space( 5 );
					else
						GUILayout.Label( output[ i ], outputLineStyle );
				}

			}
			finally
			{
				GUILayout.EndScrollView();
			}

			GUILayout.Space( 2 );

		}

		#region Nested classes

		private class AutoCompleteField
		{

			#region Public delegates

			public delegate void EventPreviewHandler( Event evt, ref string text );

			#endregion

			#region Public fields

			public EventPreviewHandler onKeyDownPreview;

			#endregion

			#region Private variables

			private static string IDENTIFIER_CHACTERS = "@$_abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
			private string AUTO_COMPLETE_CHARACTERS = " {}[]().,:;+-%&|^!~=<>?@#'\"\\";
			private const string FIELD_NAME = "__script_expression";
			private const int SCROLL_HEIGHT = 100;

			private GUIStyle backgroundStyle;
			private GUIStyle itemStyle;
			private GUIStyle hiliteStyle;
			private GUIStyle inputStyle;

			private List<GUIContent> options = new List<GUIContent>();
			private List<GUIContent> filteredOptions = new List<GUIContent>();

			private Vector2 scrollPos;

			private string guid = System.Guid.NewGuid().ToString();
			private string lastText = string.Empty;

			private int _selectedIndex = -1;
			private int selectedIndex
			{
				get { return _selectedIndex; }
				set
				{
					//if( value != _selectedIndex )
					//	Debug.LogWarning( "selectedIndex set to " + value );
					_selectedIndex = value;
				}
			}

			private bool _showCompletion = false;
			private bool showCompletion
			{
				get { return _showCompletion; }
				set
				{
					//if( value != _showCompletion )
					//	Debug.LogWarning( "showCompletion set to " + value );
					_showCompletion = value;
				}
			}

			private bool hasShownCompletion = false;

			private TextEditor editor = null;

			private Texture classIcon;
			private Texture fieldIcon;
			private Texture propertyIcon;
			private Texture methodIcon;

			#endregion

			#region Public methods

			public string TextField( string text, ImmediateWindowEvaluator evaluator )
			{

				// Unity forces us to initialize all GUIStyle instances during the OnGUI call,
				// rather than when this class is instantiated O.o
				if( backgroundStyle == null )
				{
					initializeStyles();
				}

				// Obtain the control ID and event information before editing the user's command text
				var controlID = GUIUtility.GetControlID( this.GetHashCode() & this.guid.GetHashCode(), FocusType.Passive );
				var evt = Event.current;
				var eventType = evt.GetTypeForControl( controlID );

				// We will use the TextEditor instance associated with our text field to determine the
				// text field's position, cursor position, etc. Note that Unity creates an associated
				// TextEditor behind the scenes for GUILayout.TextField calls, but if one doesn't exist
				// the following will impliclitly create a new one. We use GUIUtility.keyboardControl to
				// obtain the correct instance. We don't have to check for focus in this particular case
				// because our EditorWindow only contains one text entry control.
				this.editor = (TextEditor)GUIUtility.GetStateObject( typeof( TextEditor ), GUIUtility.keyboardControl );

				// Some user input must be processed *before* displaying the text field, if we want to
				// be able to consume the event rather than allow default processing. For instance, handling
				// the up and down arrow keys. Additionally, GUILayout.TextField tends to eat KeyDown
				// events, so if we want to process any of those we need to do so first.
				handlePreRenderEvents( evt, ref text, evaluator );

				// If the text has been set outside of this class (such as from a scrollable command history),
				// then clear all autocomplete info and position the cursor at the end of the text field.
				if( lastText != text )
				{
					clearAutoComplete();
					editor.cursorIndex = editor.selectIndex = text.Length;
					hasShownCompletion = false;
				}

				// Edit the command text and keep track of the last command text the user entered
				GUI.SetNextControlName( FIELD_NAME );
				text = this.lastText = GUILayout.TextField( text, inputStyle );

				// This doesn't really matter in this instance, since there is only one focus-able control
				// in this EditorWindow instance. Even so, I feel more confident leaving this in place, since
				// I may add new controls in the future, and have definite plans to re-use this class for
				// other tools.
				if( GUI.GetNameOfFocusedControl() != FIELD_NAME )
				{
					return text;
				}

				// Clicking in the text field with the mouse automatically cancels the autocomplete list
				if( eventType == EventType.MouseDown && editor.position.Contains( evt.mousePosition ) )
				{
					clearAutoComplete();
					return text;
				}

				// Handle any keystrokes that can be (or are better served by being) processed after the
				// textfield has been displayed.
				handlePostRenderEvents( ref text, evaluator, evt );

				if( showCompletion )
				{

					var fieldRect = editor.position;
					var graphicalCursorPos = editor.graphicalCursorPos;
					var cursorPos = new Vector2( fieldRect.x + graphicalCursorPos.x, fieldRect.y + graphicalCursorPos.y );

					text = showList( text, evt, cursorPos );

				}

				return text;

			}

			#endregion

			#region Private utility methods

			private void handlePreRenderEvents( Event evt, ref string text, ImmediateWindowEvaluator evaluator )
			{

				// Unity GUI is so frakking broken that I have to implement cut, copy, and paste commands
				// manually. The EditorGUILayout.TextField() method does have this functionality, but is
				// broken in so many other ways that it will not work for the purpose of this class.
				if( evt.isKey && ( evt.control || evt.command ) )
				{

					if( evt.keyCode == KeyCode.A )
					{
						editor.SelectAll();
						return;
					}
					else if( evt.keyCode == KeyCode.V && !string.IsNullOrEmpty( EditorGUIUtility.systemCopyBuffer ) )
					{
						// note from hs
						// copy paste kinda works with the native UI, so you can rely on it for simple cases
						// however, auto-completion stuff will not be handled
						// uncomment the next line to activate native handling (do not consume event)
						// return;

						var pasteText = EditorGUIUtility.systemCopyBuffer.Replace( "\n", "" ).Trim();
						if( string.IsNullOrEmpty( pasteText ) )
							return;

						evt.Use();

						// author: hs
						// minor change: just swapped start and end so that selectIndex is the start
						// because selectIndex is the initial cursor position and most people highlight
						// text from left to right
						var start = editor.selectIndex;
						var end = editor.cursorIndex;

						if( editor.hasSelection )
						{

							if( end < start )
							{
								swap( ref start, ref end );
							}

							text = text.Remove( start, end - start );

						}

						text = lastText = text.Insert( start, pasteText );
						// FIXME: editor.content.text is not updated with text yet, so the cursorIndex and selectIndex set properties
						// are clipped before the end of the final text if we pasted a text so that the new text is longer than the previous one
						// The real fix if we still want to delay the update of editor.content.text is to also delay setting the values of cursorIndex / selectIndex
						// as well (just after updating the text content).
						// Otherwise, the next line provides a small fix, though redundant with the later text update
						// editor.content.text = text;
						editor.cursorIndex = editor.selectIndex = start + pasteText.Length;

						clearAutoComplete();

						return;

					}
					else if( ( evt.keyCode == KeyCode.C || evt.keyCode == KeyCode.X ) && editor.hasSelection )
					{

						evt.Use();

						var start = editor.selectIndex;
						var end = editor.cursorIndex;

						if( end < start )
						{
							swap( ref start, ref end );
						}

						EditorGUIUtility.systemCopyBuffer = text.Substring( start, end - start );

						if( evt.keyCode == KeyCode.X )
						{
							text = lastText = text.Remove( start, end - start );
							editor.selectIndex = editor.cursorIndex = start;
						}

						return;

					}

				}

				if( evt.type != EventType.KeyDown )
					return;

				if( showCompletion )
				{

					switch( evt.keyCode )
					{
						case KeyCode.Home:
						case KeyCode.End:
							clearAutoComplete();
							break;

						case KeyCode.LeftArrow:
						case KeyCode.Backspace:
							if( editor.cursorIndex > 0 && !char.IsLetterOrDigit( text[ editor.cursorIndex - 1 ] ) )
							{
								clearAutoComplete();
							}
							break;

						case KeyCode.Delete:
							if( !char.IsLetterOrDigit( text[ editor.cursorIndex ] ) )
							{
								clearAutoComplete();
							}
							break;

						case KeyCode.UpArrow:
							evt.Use();
							selectedIndex = Mathf.Max( 0, selectedIndex - 1 );
							ensureVisible( this.selectedIndex );
							return;

						case KeyCode.DownArrow:
							evt.Use();
							selectedIndex = Mathf.Min( filteredOptions.Count - 1, selectedIndex + 1 );
							ensureVisible( this.selectedIndex );
							return;

						case KeyCode.Escape:
							evt.Use();
							clearAutoComplete();
							return;

						case KeyCode.Tab:
						case KeyCode.Return:
							evt.Use();
							this.lastText = text = doAutoComplete( text );
							return;

						default:
							if( showCompletion && AUTO_COMPLETE_CHARACTERS.Contains( evt.character ) )
							{
								handleCompletionCharacter( ref text, evt, evaluator );
							}
							break;

					}

				}

				if( this.onKeyDownPreview != null )
				{
					this.onKeyDownPreview( evt, ref text );
				}

			}

			private void handlePostRenderEvents( ref string text, ImmediateWindowEvaluator evaluator, Event evt )
			{

				var forceAutoComplete = evt.keyCode == KeyCode.J && ( evt.control || evt.command );
				if( forceAutoComplete )
				{
					hasShownCompletion = false;
				}

				// Tokenize the source so that we can determine whether to show the autocomplete prompt,
				// as well as determine what should be displayed in it.
				var tokens = Tokenizer.Tokenize( text, false, false );

				// If no tokens were returned, then don't show the prompt. Additionally, if there are
				// any tokens whose Token.Type == TokenType.Symbol, that indicates that the source was
				// not properly formed. For instance, the user could be entering a string value, but
				// not yet have finished the string and added the final quote character. This causes
				// the tokenizer to return a Symbol for the start quote and Identifier (etc.) for the
				// rest of the string, because the full pattern for string matching cannot be completed.
				// This is just one example of probably several possibilities. Basically, it's safe
				// to assume that TokenType.Symbol indicates that the source isn't valid script yet.
				if( tokens.Count == 0 || tokens.Any( x => x.Type == TokenType.Symbol ) )
				{

					if( forceAutoComplete )
					{
						fillOptionsFromEnvironment( evaluator );
						filterOptions( string.Empty );
						showCompletion = options.Count > 0;
					}

					if( tokens.Count == 0 )
					{
						filterOptions( string.Empty );
					}

				}
				else
				{

					// Find the token where the caret is positioned
					var tokenIndex = getTokenIndex( tokens, editor.cursorIndex );
					var currentToken = tokens[ tokenIndex ];

					// Determine what to show (if anything) based on the current token (and possibly keystroke)
					if( currentToken.Type == TokenType.Identifier )
					{

						if( ( !hasShownCompletion && IDENTIFIER_CHACTERS.Contains( evt.character ) ) || forceAutoComplete )
						{
							fillOptions( tokens, tokenIndex, evaluator );
							showCompletion = options.Count > 0;
							hasShownCompletion = showCompletion;
						}

						if( showCompletion && ( char.IsLetterOrDigit( evt.character ) || forceAutoComplete ) )
						{
							filterOptions( currentToken.Value );
						}

					}
					else if( currentToken.Type == TokenType.Dot && evt.character == '.' )
					{

						if( tokenIndex > 0 && tokens[ tokenIndex - 1 ].Type == TokenType.Identifier )
						{

							fillOptions( tokens, tokenIndex, evaluator );
							filterOptions( string.Empty );

							showCompletion = options.Count > 0;

						}
						else
						{
							clearAutoComplete();
						}

					}
					else if( evt.keyCode == KeyCode.Space )
					{

						if( evt.type == EventType.KeyDown && showCompletion && filteredOptions.Count > 0 )
						{
							handleCompletionCharacter( ref text, evt, evaluator );
						}

						if( evt.type == EventType.KeyUp && tokenIndex > 0 )
						{

							currentToken = tokens[ --tokenIndex ];

							var triggers = new List<TokenType> { TokenType.LeftParens, TokenType.Comma, TokenType.New };
							if( triggers.Contains( currentToken.Type ) )
							{

								fillOptions( tokens, tokenIndex, evaluator );
								filterOptions( string.Empty );

								showCompletion = options.Count > 0;

							}

						}

					}
					else if( forceAutoComplete )
					{
						fillOptions( tokens, tokenIndex, evaluator );
						filterOptions( string.Empty );
						showCompletion = options.Count > 0;
					}
					else
					{

						var clearTypes = new List<TokenType>() { TokenType.Number, TokenType.Float, TokenType.Integer, TokenType.Symbol };

						if( clearTypes.Contains( currentToken.Type ) )
						{
							clearAutoComplete();
						}

					}

				}

			}

			private void handleCompletionCharacter( ref string text, Event evt, ImmediateWindowEvaluator evaluator )
			{

				var tokens = Tokenizer.Tokenize( text, false, false );
				if( tokens.Count == 0 )
				{
					clearAutoComplete();
				}
				else
				{

					var triggers = new List<TokenType>() { TokenType.LeftParens, TokenType.Comma, TokenType.New };

					var tokenIndex = getTokenIndex( tokens, editor.cursorIndex );
					var currentToken = tokens[ tokenIndex ];

					if( currentToken.Type == TokenType.Identifier )
					{
						text = doAutoComplete( text, tokens, tokenIndex );
						editor.cursorIndex = editor.selectIndex += 1;
						return;
					}
					//else if( triggers.Contains( currentToken.Type ) )
					//{
					//	fillOptions( tokens, tokenIndex, evaluator );
					//	filterOptions( string.Empty );
					//	showCompletion = filteredOptions.Count > 0;
					//}
					else if( !triggers.Contains( currentToken.Type ) )
					{
						clearAutoComplete();
					}

				}

			}

			private string doAutoComplete( string text )
			{

				var tokens = Tokenizer.Tokenize( text, false, false );
				var tokenIndex = getTokenIndex( tokens, editor.cursorIndex );

				return doAutoComplete( text, tokens, tokenIndex );

			}

			private string doAutoComplete( string text, TokenList tokens, int tokenIndex )
			{

				if( selectedIndex < 0 || selectedIndex > filteredOptions.Count - 1 )
				{
					clearAutoComplete();
					return text;
				}

				var completion = filteredOptions[ selectedIndex ];

				// If there is currently no text in the text field, just return the selected autocomplete option
				if( string.IsNullOrEmpty( text ) )
				{

					editor.text = completion.text;  // MODIFIED: use text instead of content.text following new API
					editor.cursorIndex = editor.selectIndex = completion.text.Length;

					clearAutoComplete();

					return completion.text;

				}

				var currentToken = tokens[ tokenIndex ];

				if( currentToken.Type == TokenType.Identifier )
				{

					text = text
						.Remove( currentToken.LineOffset, currentToken.Length )
						.Insert( currentToken.LineOffset, completion.text );

					editor.cursorIndex = editor.selectIndex = currentToken.LineOffset + completion.text.Length;

				}
				else
				{
					text = text.Insert( currentToken.LineOffset + currentToken.Length, completion.text );
					editor.cursorIndex = editor.selectIndex = currentToken.LineOffset + currentToken.Length + completion.text.Length;
				}

				clearAutoComplete();

				return text;

			}

			private void filterOptions( string filter )
			{

				selectedIndex = -1;
				filteredOptions.Clear();

				if( string.IsNullOrEmpty( filter ) )
				{
					filteredOptions.AddRange( options );
				}
				else
				{

					var lower = filter.ToLower();

					foreach( var item in options )
					{
						if( item.text.ToLower().Contains( lower ) )
						{
							filteredOptions.Add( item );
						}
					}

				}

				if( filteredOptions.Count == 0 )
				{
					return;
				}

				if( filteredOptions.Count == 1 && filteredOptions[ 0 ].text == filter )
				{
					filteredOptions.Clear();
					return;
				}

				this.selectedIndex = selectClosestOption( filter );

			}

			private static void swap<T>( ref T arg1, ref T arg2 )
			{
				T temp = arg1;
				arg1 = arg2;
				arg2 = temp;
			}

			private static int calcEditDistance( string source, string target, int threshold )
			{

				// http://stackoverflow.com/a/9454016/154165

				int length1 = source.Length;
				int length2 = target.Length;

				if( Math.Abs( length1 - length2 ) > threshold )
				{
					return int.MaxValue;
				}

				if( length1 > length2 )
				{
					swap( ref target, ref source );
					swap( ref length1, ref length2 );
				}

				int maxi = length1;
				int maxj = length2;

				int[] dCurrent = new int[ maxi + 1 ];
				int[] dMinus1 = new int[ maxi + 1 ];
				int[] dMinus2 = new int[ maxi + 1 ];
				int[] dSwap;

				for( int i = 0; i <= maxi; i++ )
				{
					dCurrent[ i ] = i;
				}

				int jm1 = 0, im1 = 0, im2 = -1;

				for( int j = 1; j <= maxj; j++ )
				{

					dSwap = dMinus2;
					dMinus2 = dMinus1;
					dMinus1 = dCurrent;
					dCurrent = dSwap;

					int minDistance = int.MaxValue;
					dCurrent[ 0 ] = j;
					im1 = 0;
					im2 = -1;

					for( int i = 1; i <= maxi; i++ )
					{

						int cost = source[ im1 ] == target[ jm1 ] ? 0 : 1;

						int del = dCurrent[ im1 ] + 1;
						int ins = dMinus1[ i ] + 1;
						int sub = dMinus1[ im1 ] + cost;

						int min = ( del > ins ) ? ( ins > sub ? sub : ins ) : ( del > sub ? sub : del );

						if( i > 1 && j > 1 && source[ im2 ] == target[ jm1 ] && source[ im1 ] == target[ j - 2 ] )
							min = Math.Min( min, dMinus2[ im2 ] + cost );

						dCurrent[ i ] = min;

						if( min < minDistance )
							minDistance = min;

						im1++;
						im2++;

					}

					jm1++;

					if( minDistance > threshold )
						return int.MaxValue;

				}

				int result = dCurrent[ maxi ];

				return ( result > threshold ) ? int.MaxValue : result;

			}

			private int scoreOption( string filter, string option )
			{

				if( option == filter )
					return -int.MaxValue;

				if( option.ToLower() == filter.ToLower() )
					return -int.MaxValue / 2;

				if( option.StartsWith( filter ) )
					return -filter.Length * 2;

				if( option.ToLower().StartsWith( filter.ToLower() ) )
					return -filter.Length;

				return calcEditDistance( filter, option, 5 );

			}

			private int selectClosestOption( string filter )
			{

				if( string.IsNullOrEmpty( filter ) )
					return -1;

				var closestOption = filteredOptions
					.Select( x => new { score = scoreOption( filter, x.text ), value = x.text } )
					.OrderBy( x => x.score )
					.First();

				if( closestOption.score >= 2 )
					return -1;

				return filteredOptions.FindIndex( x => x.text == closestOption.value );

			}

			private void fillOptions( TokenList tokens, int currentIndex, ImmediateWindowEvaluator evaluator )
			{

				if( currentIndex > 0 && tokens[ currentIndex - 1 ].Type == TokenType.New )
				{
					fillOptionsFromTypes( evaluator );
					return;
				}

				if( currentIndex == 0 || tokens[ currentIndex - 1 ].Type != TokenType.Identifier )
				{
					fillOptionsFromEnvironment( evaluator );
					return;
				}

				var currentToken = tokens[ currentIndex ];

				if( currentIndex > 0 && currentToken.Type == TokenType.Dot )
				{

					var memberInfo = new List<string>();

					var seek = currentIndex - 1;
					while( seek >= 0 )
					{

						var token = tokens[ seek ];
						if( token.Type != TokenType.Dot )
						{

							if( token.Type == TokenType.Whitespace )
								break;
							else if( token.Type != TokenType.Identifier )
								return;

							memberInfo.Insert( 0, token.Value );

						}

						seek -= 1;

					}

					fillOptionsFromMembers( memberInfo, evaluator );

				}


			}

			private void fillOptionsFromMembers( List<string> memberInfo, ImmediateWindowEvaluator evaluator )
			{

				clearAutoComplete();

				var index = 0;

				var type = typeof( object );
				var staticMembers = false;

				var variableName = memberInfo[ index++ ];
				var variable = evaluator.GetVariable( variableName );
				if( variable != null )
				{
					type = variable.Type;
				}
				else
				{

					type = evaluator.GetConstant( variableName ) as System.Type;
					if( type == null )
						return;

					staticMembers = true;

				}

				while( index < memberInfo.Count )
				{

					var bindingFlags = BindingFlags.Public | ( staticMembers ? BindingFlags.Static : BindingFlags.Instance );
					staticMembers = false;

					var memberName = memberInfo[ index++ ];
					var member = type.GetMember( memberName, bindingFlags ).FirstOrDefault();

					if( member is FieldInfo )
						type = ( (FieldInfo)member ).FieldType;
					else if( member is PropertyInfo )
						type = ( (PropertyInfo)member ).PropertyType;
					else
						return;

				}

				var members = type
					.GetMembers( BindingFlags.Public | ( staticMembers ? BindingFlags.Static : BindingFlags.Instance ) )
					.Where( x => isValidMember( x ) );

				foreach( var member in members )
				{

					if( options.Any( x => x.text == member.Name ) )
						continue;

					options.Add( buildGUIContent( member ) );

				}

				options.Sort( ( lhs, rhs ) => lhs.text.CompareTo( rhs.text ) );

			}

			private GUIContent buildGUIContent( MemberInfo member )
			{

				var icon = fieldIcon;
				var text = member.Name;

				if( member is PropertyInfo )
					icon = propertyIcon;
				else if( member is MethodInfo )
					icon = methodIcon;

				return new GUIContent( text, icon );

			}

			private bool isValidMember( MemberInfo member )
			{

				if( member is FieldInfo || member is PropertyInfo )
					return true;

				var method = member as MethodInfo;

				return method != null && !method.IsSpecialName;

			}

			private void fillOptionsFromTypes( ImmediateWindowEvaluator evaluator )
			{

				clearAutoComplete();

				var environment = evaluator.Environment;

				var keys = environment.Constants.Keys.ToList();
				keys.Sort();

				foreach( var key in keys )
				{
					var type = environment.Constants[ key ];
					if( type is System.Type )
					{
						options.Add( buildGUIContent( key, type ) );
					}
				}

				filteredOptions.AddRange( options );

			}

			private void fillOptionsFromEnvironment( ImmediateWindowEvaluator evaluator )
			{

				clearAutoComplete();

				var environment = evaluator.Environment;

				var contantKeys = environment.Constants.Keys;
				foreach( var key in contantKeys )
				{
					var value = environment.Constants[ key ];
					options.Add( buildGUIContent( key, value ) );
				}

				var variableKeys = environment.Variables.Keys;
				foreach( var key in variableKeys )
				{
					var variable = environment.Variables[ key ];
					options.Add( buildGUIContent( key, variable ) );
				}

				fillKeywords( options );

				options.Sort( ( lhs, rhs ) => { return lhs.text.CompareTo( rhs.text ); } );

				filteredOptions.AddRange( options );

			}

			private void fillKeywords( List<GUIContent> options )
			{

				var keywords = new List<string>()
				{
					"var", "byte", "char", "short", "int", "uint", "ulong", "long", "float", "double",
					"object", "null", "undefined", "string", "bool", "true", "false"
				};

				var keywordIcon = (Texture)Resources.Load( "keywordIcon" );

				foreach( var keyword in keywords )
				{
					if( !options.Any( x => x.text == keyword ) )
					{
						options.Add( new GUIContent( keyword, keywordIcon ) );
					}
				}

			}

			private GUIContent buildGUIContent( string text, object value )
			{

				var icon = fieldIcon;
				if( value is System.Type )
					icon = classIcon;

				return new GUIContent( text, icon );

			}

			private string showList( string text, Event evt, Vector2 cursorPos )
			{

				if( filteredOptions.Count == 0 )
				{
					showCompletion = false;
					return text;
				}

				var size = measureOptions( filteredOptions );
				var lineHeight = size.y / filteredOptions.Count;
				var windowHeight = Mathf.Min( SCROLL_HEIGHT, size.y );
				var windowWidth = ( size.y > SCROLL_HEIGHT ) ? size.x + 16 : size.x;
				var viewRect = new Rect( 0, 0, size.x, size.y );
				var windowRect = new Rect( cursorPos.x, cursorPos.y - windowHeight - 3, windowWidth, windowHeight );

				GUI.Box( windowRect, GUIContent.none, backgroundStyle );

				if( evt.isKey )
				{
					ensureVisible( this.selectedIndex );
				}

				var itemRect = new Rect( 0, 0, size.x, lineHeight );
				var mousePosition = evt.mousePosition - windowRect.position + scrollPos;

				this.scrollPos = GUI.BeginScrollView( windowRect, scrollPos, viewRect );
				{

					for( int i = 0; i < filteredOptions.Count; i++ )
					{

						var style = itemStyle;

						if( evt.type == EventType.MouseMove && itemRect.Contains( mousePosition ) )
							selectedIndex = i;

						if( i == selectedIndex )
						{
							style = hiliteStyle;
						}
						else
						{
							style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
						}

						if( GUI.Button( itemRect, filteredOptions[ i ], style ) )
						{
							selectedIndex = i;
							return doAutoComplete( text );
						}

						itemRect.y += itemRect.height;

					}

				}
				GUI.EndScrollView();

				hasShownCompletion = true;

				return text;

			}

			private void clearAutoComplete()
			{

				this.options.Clear();
				this.filteredOptions.Clear();

				this.scrollPos = Vector2.zero;
				this.selectedIndex = -1;
				this.showCompletion = false;

			}

			private int getTokenIndex( TokenList tokens, int offset )
			{

				var tokenIndex = 0;
				while( tokenIndex < tokens.Count - 1 && tokens[ tokenIndex + 1 ].LineOffset < offset )
				{
					tokenIndex += 1;
				}

				return tokenIndex;

			}

			private void initializeStyles()
			{

				inputStyle = new GUIStyle( (GUIStyle)"LargeTextField" );
				inputStyle.fontSize = 12;

				backgroundStyle = new GUIStyle( (GUIStyle)"WindowBackground" );

				itemStyle = new GUIStyle( (GUIStyle)"ControlLabel" );
				itemStyle.padding.left += 5;
				itemStyle.padding.right += 5;
				itemStyle.fontSize = 12;

				hiliteStyle = new GUIStyle( (GUIStyle)"LODSliderRangeSelected" );
				hiliteStyle.font = itemStyle.font;
				hiliteStyle.fontStyle = itemStyle.fontStyle;
				hiliteStyle.fontSize = itemStyle.fontSize;
				hiliteStyle.alignment = itemStyle.alignment;
				hiliteStyle.padding = itemStyle.padding;
				hiliteStyle.contentOffset = itemStyle.contentOffset;
				hiliteStyle.normal.textColor = Color.white;

				classIcon = Resources.Load( "classIcon" ) as Texture;
				fieldIcon = Resources.Load( "fieldIcon" ) as Texture;
				propertyIcon = Resources.Load( "propertyIcon" ) as Texture;
				methodIcon = Resources.Load( "methodIcon" ) as Texture;

			}

			private void ensureVisible( int selectedIndex )
			{

				if( selectedIndex < 0 || selectedIndex > filteredOptions.Count - 1 )
					return;

				var itemSize = itemStyle.CalcSize( filteredOptions[ 0 ] );
				var itemTop = selectedIndex * itemSize.y;
				var itemBottom = itemTop + itemSize.y;

				if( itemTop < scrollPos.y )
				{
					scrollPos = new Vector2( 0, itemTop );
				}
				else if( itemBottom > scrollPos.y + SCROLL_HEIGHT )
				{
					scrollPos = new Vector2( 0, itemBottom - SCROLL_HEIGHT );
				}

			}

			private Vector2 measureOptions( List<GUIContent> options )
			{

				var size = new Vector2( 150, 0 );

				foreach( var item in options )
				{

					var itemSize = itemStyle.CalcSize( item );

					size.y += itemSize.y;
					size.x = Mathf.Max( size.x, itemSize.x + 25 );

				}

				return size;

			}

			#endregion

		}

		private class CommandHistory
		{

			#region Private variables

			private List<string> history = new List<string>();
			private int index = 0;

			#endregion

			#region Public Methods

			public void Add( string command )
			{
				history.Add( command );
				index = history.Count;
			}

			public string Previous()
			{

				if( history.Count == 0 )
					return string.Empty;

				if( index > 0 )
					index -= 1;

				return history[ index ];

			}

			public string Next()
			{

				if( history.Count == 0 )
					return string.Empty;

				index = Mathf.Min( history.Count, index + 1 );
				if( index > history.Count - 1 )
					return string.Empty;

				return history[ index ];

			}

			public void Clear()
			{
				history.Clear();
				index = 0;
			}

			#endregion

		}

		#endregion

	}

}
