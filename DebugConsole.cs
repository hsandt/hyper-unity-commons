/*==== DebugConsole.cs ====================================================
 * Class for handling multi-line, multi-color debugging messages.
 * Original Author: Jeremy Hollingsworth
 * Based On: Version 1.2.1 Mar 02, 2006
 *
 * Modified: Simon Waite
 * Date: 22 Feb 2007
 *
 * Modified: Shinsuke Sugita
 * Date: 1 Dec 2015
 *
 * Modified: Long Nguyen Huu
 * Date: 21 Apr 2017
 *
 * Modification to original script to allow pixel-correct line spacing
 *
 * Setting the boolean pixelCorrect changes the units in lineSpacing property
 * to pixels, so you have a pixel correct gui font in your console.
 *
 * It also checks every frame if the screen is resized to make sure the
 * line spacing is correct (To see this; drag and let go in the editor
 * and the text spacing will snap back)
 *
 * Modification by Long Nguyen Huu to register the DebugConsole logging to the UnityEngine.Debug.LogXXX methods.
 *
 * This is a ompatibility-breaking change that replaces log type name strings with LogType enum values,
 * besides calling the custom log methods each time the Unity log methods are called,
 * and therefore not shared on Unify community.
 *
 * USAGE:
 * ::Drop in your standard assets folder (if you want to change any of the
 * default settings in the inspector, create an empty GameObject and attach
 * this script to it from you standard assets folder.  That will provide
 * access to the default settings in the inspector)
 *
 * ::To use, call DebugConsole.functionOrProperty() where
 * functionOrProperty = one of the following:
 *
 * -Log(string message)  Adds "message" to the list
 * -LogWarning(string message)  Adds "message" to the list with warning color
 * -LogError(string message)  Adds "message" to the list with error color
 *
 * However, for the three methods above, we recommend to check attachToStandardLog and just use
 * Unity debug log methods (which also allow formatting)
 *
 * Clear() Clears all messages
 *
 * isVisible (true,false)  Toggles the visibility of the output.  Does _not_
 * clear the messages.
 *
 * isDraggable (true, false)  Toggles mouse drag functionality
 * =========================================================================*/

// REFACTOR / OPTIMIZE: use IMGUI (entirely in code) instead of Legacy GUI

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Commons.Debug
{

	public class DebugConsole : MonoBehaviour
	{
		public GameObject DebugGui = null;             // The GUI that will be duplicated
		public Vector3 defaultGuiPosition = new Vector3(0.01F, 0.98F, 0F);
		public Vector3 defaultGuiScale = new Vector3(0.5F, 0.5F, 1F);
		public Color normal = Color.green;
		public Color warning = Color.yellow;
		public Color error = Color.red;
		public Color assertColor = Color.magenta;
		public Color exceptionColor = Color.blue;
		public int maxMessages = 30;                   // The max number of messages displayed
		public float lineSpacing = 0.02F;              // The amount of space between lines
		public ArrayList messages = new ArrayList();
		public ArrayList guis = new ArrayList();
		public List<LogType> types = new List<LogType>(); // MODIFIED: replaced color names with list of log types, that are then converted to colors
		public bool draggable = true;                  // Can the output be dragged around at runtime by default?
		public bool visible = true;                    // Does output show on screen by default or do we have to enable it with code?
		public bool pixelCorrect = false; // set to be pixel Correct linespacing
		public bool attachToStandardLog = false; // use the custom log methods as callbacks when Unity Debug log methods are called (reenable component to update behaviour)
		public bool dontDestroyOnLoad = false;
		public static bool isVisible
		{
			get
			{
				return DebugConsole.instance.visible;
			}

			set
			{
				DebugConsole.instance.visible = value;
				if (value == true)
				{
					DebugConsole.instance.Display();
				}
				else if (value == false)
				{
					DebugConsole.instance.ClearScreen();
				}
			}
		}

		public static bool isDraggable
		{
			get
			{
				return DebugConsole.instance.draggable;
			}

			set
			{
				DebugConsole.instance.draggable = value;

			}
		}


		private static DebugConsole s_Instance = null;   // Our instance to allow this script to be called without a direct connection.
		public static DebugConsole instance
		{
			get
			{
				if (s_Instance == null)
				{
					s_Instance = FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
					if (s_Instance == null)
					{
						GameObject console = new GameObject();
						console.AddComponent<DebugConsole>();
						console.name = "DebugConsoleController";
						s_Instance = FindObjectOfType(typeof(DebugConsole)) as DebugConsole;
						DebugConsole.instance.InitGuis();
					}

				}

				return s_Instance;
			}
		}

		void Awake()
		{
			s_Instance = this;

			if (dontDestroyOnLoad)
				DontDestroyOnLoad(this);

			InitGuis();
		}

		#if DEVELOPMENT_BUILD
		// ADDED
		void OnEnable()
		{
			if (attachToStandardLog)
				Application.logMessageReceived += HandleLog;
		}

		// ADDED
		void OnDisable()
		{
			Application.logMessageReceived -= HandleLog;
		}

		// ADDED
		void HandleLog (string message, string stackTrace, LogType type) {
			// Print on screen any message except errors, that are already shown in the Development console.
			// TODO: also check if exceptions and assertions are not shown too
			if (!(type == LogType.Error))
				Log(message, type);
		}
		#endif

		protected bool guisCreated = false;
		protected float screenHeight =-1;
		public void InitGuis()
		{
			float usedLineSpacing = lineSpacing;
			screenHeight = Screen.height;
			if(pixelCorrect)
				usedLineSpacing = 1.0F / screenHeight * usedLineSpacing;

			if (guisCreated == false)
			{
				if (DebugGui == null)  // If an external GUIText is not set, provide the default GUIText
				{
					DebugGui = new GameObject();
					DebugGui.AddComponent<GUIText>();
					DebugGui.name = "DebugGUI(0)";
					DebugGui.transform.position = defaultGuiPosition;
					DebugGui.transform.localScale = defaultGuiScale;
				}

				// Create our GUI objects to our maxMessages count
				Vector3 position = DebugGui.transform.position;
				guis.Add(DebugGui);
				int x = 1;

				while (x < maxMessages)
				{
					position.y -= usedLineSpacing;
					GameObject clone = null;
					clone = (GameObject)Instantiate(DebugGui, position, transform.rotation);
					clone.name = string.Format("DebugGUI({0})", x);
					guis.Add(clone);
					position = clone.transform.position;
					x += 1;
				}

				x = 0;
				while (x < guis.Count)
				{
					GameObject temp = (GameObject)guis[x];
					temp.transform.parent = DebugGui.transform;
					x++;
				}
				guisCreated = true;
			} else {
				// we're called on a screensize change, so fiddle with sizes
				Vector3 position = DebugGui.transform.position;
				for(int x=0;x < guis.Count; x++)
				{
					position.y -= usedLineSpacing;
					GameObject temp = (GameObject)guis[x];
					temp.transform.position= position;
				}
			}
		}



		bool connectedToMouse = false;
		void Update()
		{
			// If we are visible and the screenHeight has changed, reset linespacing
			if (visible == true && screenHeight != Screen.height)
			{
				InitGuis();
			}
			if (draggable == true)
			{
				if (Input.GetMouseButtonDown(0))
				{
					if (connectedToMouse == false && DebugGui.GetComponent<GUIText>().HitTest((Vector3)Input.mousePosition) == true)
					{
						connectedToMouse = true;
					}
					else if (connectedToMouse == true)
					{
						connectedToMouse = false;
					}

				}

				if (connectedToMouse == true)
				{
					float posX = DebugGui.transform.position.x;
					float posY = DebugGui.transform.position.y;
					posX = Input.mousePosition.x / Screen.width;
					posY = Input.mousePosition.y / Screen.height;
					DebugGui.transform.position = new Vector3(posX, posY, 0F);
				}
			}

			if (Input.GetKeyDown(KeyCode.F3))
				Clear();
		}
		//+++++++++ INTERFACE FUNCTIONS ++++++++++++++++++++++++++++++++
		public static void Log(string message, LogType type)
		{
			DebugConsole.instance.AddMessage(message, type);
		}
		//++++ OVERLOAD ++++
		public static void Log(string message)
		{
			DebugConsole.instance.AddMessage(message);
		}

		public static void LogWarning(string message)
		{
			DebugConsole.instance.AddMessage(message, LogType.Warning);
		}

		public static void LogError(string message)
		{
			DebugConsole.instance.AddMessage(message, LogType.Error);
		}

		public static void Clear()
		{
			DebugConsole.instance.ClearMessages();
		}
		//++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


		//---------- void AddMesage(string message, LogType type) ------
		//Adds a message to the list
		//--------------------------------------------------------------

		public void AddMessage(string message, LogType type)
		{
			messages.Add(message);
			types.Add(type);
			Display();
		}
		//++++++++++ OVERLOAD for AddMessage ++++++++++++++++++++++++++++
		// Overloads AddMessage to only require one argument(message)
		//+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
		public void AddMessage(string message)
		{
			messages.Add(message);
			types.Add(LogType.Log);
			Display();
		}


		//----------- void ClearMessages() ------------------------------
		// Clears the messages from the screen and the lists
		//---------------------------------------------------------------
		public void ClearMessages()
		{
			messages.Clear();
			types.Clear();
			ClearScreen();
		}


		//-------- void ClearScreen() ----------------------------------
		// Clears all output from all GUI objects
		//--------------------------------------------------------------
		void ClearScreen()
		{
			if (guis.Count < maxMessages)
			{
				//do nothing as we haven't created our guis yet
			}
			else
			{
				int x = 0;
				while (x < guis.Count)
				{
					GameObject gui = (GameObject)guis[x];
					gui.GetComponent<GUIText>().text = "";
					//increment and loop
					x += 1;
				}
			}
		}


		//---------- void Prune() ---------------------------------------
		// Prunes the array to fit within the maxMessages limit
		//---------------------------------------------------------------
		void Prune()
		{
			int diff;
			if (messages.Count > maxMessages)
			{
				if (messages.Count <= 0)
				{
					diff = 0;
				}
				else
				{
					diff = messages.Count - maxMessages;
				}
				messages.RemoveRange(0, (int)diff);
				types.RemoveRange(0, (int)diff);
			}

		}

		//---------- void Display() -------------------------------------
		// Displays the list and handles coloring
		//---------------------------------------------------------------
		void Display()
		{
			//check if we are set to display
			if (visible == false)
			{
				ClearScreen();
			}
			else if (visible == true)
			{


				if (messages.Count > maxMessages)
				{
					Prune();
				}

				// Carry on with display
				int x = 0;
				if (guis.Count < maxMessages)
				{
					//do nothing as we havent created our guis yet
				}
				else
				{
					while (x < messages.Count)
					{
						GameObject gui = (GameObject)guis[x];

						//set our color
						switch (types[x])
						{
						case LogType.Log: gui.GetComponent<GUIText>().material.color = normal;
							break;
						case LogType.Warning: gui.GetComponent<GUIText>().material.color = warning;
							break;
						case LogType.Error: gui.GetComponent<GUIText>().material.color = error;
							break;
						case LogType.Assert: gui.GetComponent<GUIText>().material.color = assertColor;
							break;
						case LogType.Exception: gui.GetComponent<GUIText>().material.color = exceptionColor;
							break;
						}

						//now set the text for this element
						gui.GetComponent<GUIText>().text = (string)messages[x];

						//increment and loop
						x += 1;
					}
				}

			}
		}


	}// End DebugConsole Class

}
