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

		[MenuItem("Tools/JCMG/Curves/Submit bug or feature request")]
		internal static void OpenURLToGitHubIssuesSection()
		{
			const string GITHUB_ISSUES_URL = "https://github.com/jeffcampbellmakesgames/unity-curves/issues";

			Application.OpenURL(GITHUB_ISSUES_URL);
		}

		[MenuItem("Tools/JCMG/Curves/Donate to support development")]
		internal static void OpenURLToKoFi()
		{
			const string KOFI_URL = "https://ko-fi.com/stampyturtle";

			Application.OpenURL(KOFI_URL);
		}

		[MenuItem("Tools/JCMG/Curves/About")]
		internal static void OpenAboutModalDialog()
		{
			AboutWindow.View();
		}
	}
}
