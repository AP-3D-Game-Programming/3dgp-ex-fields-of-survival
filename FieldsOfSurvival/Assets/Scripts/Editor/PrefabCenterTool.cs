#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Editor tool om alle children van een prefab te centreren rond de origin.
/// Berekent het center van alle objecten en verschuift alles zodat het center op (0,0,0) komt.
/// </summary>
public class PrefabCenterTool : EditorWindow
{
    private GameObject targetPrefab;

    [MenuItem("Tools/Prefab Center Tool")]
    public static void ShowWindow()
    {
        GetWindow<PrefabCenterTool>("Prefab Center Tool");
    }

    private void OnGUI()
    {
        GUILayout.Label("Center Prefab Children to Origin", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetPrefab = (GameObject)EditorGUILayout.ObjectField("Prefab / GameObject", targetPrefab, typeof(GameObject), true);

        GUILayout.Space(10);

        if (targetPrefab == null)
        {
            EditorGUILayout.HelpBox("Sleep je MapExtension prefab hierin, of selecteer het root object in de scene.", MessageType.Info);
            return;
        }

        // Calculate current center
        Vector3 currentCenter = CalculateCenter(targetPrefab);
        EditorGUILayout.HelpBox($"Huidig center van alle children:\nX: {currentCenter.x:F2}, Y: {currentCenter.y:F2}, Z: {currentCenter.z:F2}", MessageType.None);

        GUILayout.Space(10);

        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("Center All Children to Origin", GUILayout.Height(40)))
        {
            CenterChildren(targetPrefab);
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("Dit verschuift alle children zodat hun gezamenlijk center op (0,0,0) komt te staan.", MessageType.Info);
    }

    private Vector3 CalculateCenter(GameObject root)
    {
        Renderer[] renderers = root.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            // Fallback: gebruik transforms
            Transform[] children = root.GetComponentsInChildren<Transform>();
            if (children.Length <= 1) return Vector3.zero;

            Vector3 sum = Vector3.zero;
            int count = 0;

            foreach (Transform t in children)
            {
                if (t == root.transform) continue;
                sum += t.position;
                count++;
            }

            return count > 0 ? sum / count : Vector3.zero;
        }

        // Gebruik bounds van alle renderers
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds.center;
    }

    private void CenterChildren(GameObject root)
    {
        Vector3 offset = CalculateCenter(root);

        // Alleen X en Z centereren, Y laten we vaak intact (grond niveau)
        // Verwijder deze lijn als je ook Y wilt centereren:
        // offset.y = 0;

        Undo.RegisterCompleteObjectUndo(root, "Center Prefab Children");

        // Verzamel alleen direct children van root
        foreach (Transform child in root.transform)
        {
            Undo.RecordObject(child, "Move Child");
            child.position -= offset;
        }

        Debug.Log($"Centered {root.name} - Offset applied: {offset}");

        // Mark prefab dirty als we in prefab mode zijn
        if (PrefabUtility.IsPartOfPrefabAsset(root) || PrefabUtility.IsPartOfPrefabInstance(root))
        {
            EditorUtility.SetDirty(root);
        }
    }
}
#endif