using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeObjectsMesh : EditorWindow
{
    private Mesh _mesh;
    private Material _material;
    private bool _findObjectUseSpecifiedMaterial;

    private void FindAndApply(Mesh mesh, Material mat, bool findObjectUseSpecifiedMaterial)
    {
        var objs = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var obj in objs)
        {
            if (!obj.scene.isLoaded) continue;
            var meshFilter = obj.GetComponent<MeshFilter>();
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            if (!meshFilter || !meshRenderer) continue;
            if (findObjectUseSpecifiedMaterial)
            {
                if (meshRenderer.sharedMaterial.name.Replace("(Instance)", "").Trim() != mat.name) continue;
            }
            // Debug.Log($"Name: {obj.name}, Mesh: {meshFilter.sharedMesh.name}");
            meshFilter.sharedMesh = mesh;
        }
    }
    
    [MenuItem("EditorTools/Game Object/Change Objects Mesh")]
    public static void ShowWindow()
    {
        var window = GetWindow<ChangeObjectsMesh>();
        window.titleContent = new GUIContent("Change Objects Mesh");
        window.Show();
    }

    private void OnGUI()
    {
        _mesh = EditorGUILayout.ObjectField("Mesh", _mesh, typeof(Mesh), false) as Mesh;
        _findObjectUseSpecifiedMaterial = EditorGUILayout.Toggle("Find Object Use Specified Material", _findObjectUseSpecifiedMaterial);
        if (_findObjectUseSpecifiedMaterial)
        {
            _material = EditorGUILayout.ObjectField("Material", _material, typeof(Material), false) as Material;
        }

        if (GUILayout.Button("Apply"))
        {
            FindAndApply(_mesh, _material, _findObjectUseSpecifiedMaterial);
        }
    }
}
