using UnityEngine;

namespace JCMG.Curves.Editor
{
	internal static class SceneGUIConstants
	{
		// TODO some of these seem like they could be preferences.
		public static float HandleSize { get; }

		public static Vector2 GUIOffset { get; }

		static SceneGUIConstants()
		{
			HandleSize = 0.1f;
			GUIOffset = new Vector2(10, 10);
		}
	}
}
