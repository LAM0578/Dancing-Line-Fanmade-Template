using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class ChangeSelectionObjectName : EditorWindow
{
	private bool _useRegex;
	private string _search;
	private string _name;

	private void ChangeSelectionObjectsName(string replacedName, string search,  bool useRegex)
	{
		var objs = Selection.gameObjects;
		if (objs.Length == 0) throw new UnassignedReferenceException();
		foreach (var obj in objs)
		{
			var objName = obj.name;
			objName = useRegex ? 
				Regex.Replace(objName, search, replacedName) : 
				objName.Replace(search, replacedName);
			obj.name = objName;
			
		}
	}
	
	[MenuItem("EditorTools/Game Object/Change Selection Object Name")]
	public static void ShowWindow()
	{
		var window = GetWindow<ChangeSelectionObjectName>();
		window.titleContent = new GUIContent("Change Selection Object Name");
		window.Show();
	}

	private void OnGUI()
	{
		_useRegex = EditorGUILayout.Toggle("Use Regex", _useRegex);
		_search = EditorGUILayout.TextField("Search", _search);
		_name = EditorGUILayout.TextField("Name", _name);
		if (GUILayout.Button("Apply"))
		{
			ChangeSelectionObjectsName(_name, _search, _useRegex);
		}
	}
	
}