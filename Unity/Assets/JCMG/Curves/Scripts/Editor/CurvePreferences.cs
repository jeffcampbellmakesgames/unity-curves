using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	/// <summary>
	/// An editor class for managing project and user preferences for the Curves library.
	/// </summary>
	public static class CurvePreferences
	{
		/// <summary>
		/// Returns true if debug features should be enabled, otherwise false.
		/// </summary>
		public static bool IsDebugEnabled
		{
			get { return GetBoolPref(ENABLE_DEBUG_PREF, ENABLE_DEBUG_DEFAULT); }
			set { EditorPrefs.SetBool(ENABLE_DEBUG_PREF, value); }
		}

		/// <summary>
		/// Returns true if rotation visualization info should be enabled, otherwise false.
		/// </summary>
		public static bool ShouldVisualizeRotation
		{
			get { return GetBoolPref(SHOW_ROTATION_PREF, SHOW_ROTATION_DEFAULT); }
			set { EditorPrefs.SetBool(SHOW_ROTATION_PREF, value); }
		}

		public static bool ShouldMirrorHandleMovement
		{
			get { return GetBoolPref(MIRROR_HANDLE_MOVEMENT_PREF, MIRROR_HANDLE_MOVEMENT_DEFAULT); }
			set { EditorPrefs.SetBool(MIRROR_HANDLE_MOVEMENT_PREF, value); }
		}

		// UI
		private const string PREFERENCES_TITLE_PATH = "Preferences/JCMG Curves";
		private const string USER_PREFERENCES_HEADER = "User Preferences";

		private static readonly GUILayoutOption MAX_WIDTH;

		// Searchable Fields
		private static readonly string[] KEYWORDS =
		{
			"Curve",
			"Curves"
		};

		// User Editor Preferences
		private const string SHOW_ROTATION_PREF = "JCMG.Curves.ShowRotationVisualization";
		private const string ENABLE_DEBUG_PREF = "JCMG.Curves.EnableDebug";
		private const string MIRROR_HANDLE_MOVEMENT_PREF = "JCMG.Curves.MirrorHandleMovement";

		private const bool SHOW_ROTATION_DEFAULT = true;
		private const bool ENABLE_DEBUG_DEFAULT = true;
		private const bool MIRROR_HANDLE_MOVEMENT_DEFAULT = true;

		static CurvePreferences()
		{
			MAX_WIDTH = GUILayout.MaxWidth(175f);
		}

		[SettingsProvider]
		private static SettingsProvider CreatePersonalPreferenceSettingsProvider()
		{
			return new SettingsProvider(PREFERENCES_TITLE_PATH, SettingsScope.User)
			{
				guiHandler = DrawPersonalPrefsGUI, keywords = KEYWORDS
			};
		}

		private static void DrawAllGUI()
		{
			DrawPersonalPrefsGUI();
		}

		private static void DrawPersonalPrefsGUI(string value = "")
		{
			EditorGUILayout.LabelField(USER_PREFERENCES_HEADER, EditorStyles.boldLabel);

			// Enable Orientation Visualization
			EditorGUILayout.HelpBox(
				"This will enable visualization of a point's rotation along the curve, " +
				"with lines drawn to show its local forward, up, and right vectors.",
				MessageType.Info);

			GUI.changed = false;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Should Visualize Rotation", MAX_WIDTH);
				var drawEventPref = EditorGUILayout.Toggle(ShouldVisualizeRotation);
				if (GUI.changed)
				{
					ShouldVisualizeRotation = drawEventPref;
					SceneView.RepaintAll();
				}
			}

			// Enable Debugging
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(
				"This will enable debug features for troubleshooting purposes",
				MessageType.Info);

			GUI.changed = false;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Enable Debug", MAX_WIDTH);
				var enableDebugPref = EditorGUILayout.Toggle( IsDebugEnabled, MAX_WIDTH);
				if (GUI.changed)
				{
					IsDebugEnabled = enableDebugPref;
					SceneView.RepaintAll();
				}
			}

			// Enable Mirror Handle Movement
			EditorGUILayout.Space();
			EditorGUILayout.HelpBox(
				"When enabled, moving a handle will cause the other handle to copy its " +
				"movement in the opposite direction.",
				MessageType.Info);

			GUI.changed = false;
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField("Mirror Handle Movement", MAX_WIDTH);
				var mirrorHandleMovementPref = EditorGUILayout.Toggle(ShouldMirrorHandleMovement, MAX_WIDTH);
				if (GUI.changed)
				{
					ShouldMirrorHandleMovement = mirrorHandleMovementPref;
					SceneView.RepaintAll();
				}
			}
		}

		/// <summary>
		/// Returns the current bool preference; if none exists, the default is set and returned.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultValue"></param>
		/// <returns></returns>
		private static bool GetBoolPref(string key, bool defaultValue)
		{
			if (!EditorPrefs.HasKey(key))
			{
				EditorPrefs.SetBool(key, defaultValue);
			}

			return EditorPrefs.GetBool(key);
		}
	}
}
