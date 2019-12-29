using UnityEditor;
using UnityEngine;

namespace JCMG.Curves.Editor
{
	/// <summary>
	/// Menu items for the curves library.
	/// </summary>
	internal static class MenuItems
	{
		[MenuItem("GameObject/JCMG/Curves/Bezier3DSpline", false, 10)]
		internal static void CreateBezierSpline()
		{
			var obj = new GameObject("Bezier3DSpline").AddComponent<Bezier3DSpline>();

			Selection.objects = new Object[]
			{
				obj.gameObject
			};

			EditorGUIUtility.PingObject(obj.gameObject);
		}
	}
}
