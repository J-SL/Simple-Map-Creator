using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapCreator : EditorWindow
{
    private GameObject prefabToAdd;
    private List<Vector3> spawnPositions = new List<Vector3>();
    private List<Quaternion> spawnRotations = new List<Quaternion>(); // 使用Quaternion表示旋转
    private Dictionary<GameObject, List<Vector3>> prefabSpawnPositions = new Dictionary<GameObject, List<Vector3>>();
    private Dictionary<GameObject, List<Quaternion>> prefabSpawnRotations = new Dictionary<GameObject, List<Quaternion>>();

    private UnityEditorInternal.ReorderableList spawnPositionList;
    private UnityEditorInternal.ReorderableList spawnRotationList;

    private string prefabName;
    private string spawnPositionsText;

    [MenuItem("Custom Tools/Create Map")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(MapCreator), false, "Create Map");
    }

    void OnEnable()
    {
        spawnPositionList = new UnityEditorInternal.ReorderableList(spawnPositions, typeof(Vector3), true, true, true, true);
        spawnPositionList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Spawn Positions");
        };
        spawnPositionList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            spawnPositions[index] = EditorGUI.Vector3Field(rect, GUIContent.none, spawnPositions[index]);
        };

        spawnRotationList = new UnityEditorInternal.ReorderableList(spawnRotations, typeof(Quaternion), true, true, true, true);
        spawnRotationList.drawHeaderCallback = (Rect rect) =>
        {
            EditorGUI.LabelField(rect, "Spawn Rotations");
        };
        spawnRotationList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
        {
            spawnRotations[index] = Quaternion.Euler(EditorGUI.Vector3Field(rect, GUIContent.none, spawnRotations[index].eulerAngles));
        };
    }

    void OnGUI()
    {
        prefabToAdd = EditorGUILayout.ObjectField("Prefab", prefabToAdd, typeof(GameObject), false) as GameObject;

        spawnPositionList.DoLayoutList();
        spawnRotationList.DoLayoutList();

        if (GUILayout.Button("Add"))
        {
            AddPrefabWithSpawnPositions();
        }

        GUILayout.Label("Prefab Name: " + prefabName);

        GUILayout.Label("Spawn Positions:\n" + spawnPositionsText);

        if (GUILayout.Button("Generate"))
        {
            GeneratePrefabs();
        }
    }

    void AddPrefabWithSpawnPositions()
    {
        if (prefabToAdd == null)
        {
            Debug.LogError("Please assign a prefab to add.");
            return;
        }
        if (spawnPositions.Count == 0)
        {
            Debug.LogError("Please assign at least one spawn position for the prefab.");
            return;
        }

        if (prefabSpawnPositions.ContainsKey(prefabToAdd))
        {
            prefabSpawnPositions[prefabToAdd].AddRange(spawnPositions);
            prefabSpawnRotations[prefabToAdd].AddRange(spawnRotations);
        }
        else
        {
            prefabSpawnPositions.Add(prefabToAdd, new List<Vector3>(spawnPositions));
            prefabSpawnRotations.Add(prefabToAdd, new List<Quaternion>(spawnRotations));
        }

        spawnPositions.Clear();
        spawnRotations.Clear();

        UpdateTextInfo();
    }

    void GeneratePrefabs()
    {
        if (prefabSpawnPositions.Count == 0)
        {
            Debug.LogError("Please add at least one prefab with spawn positions.");
            return;
        }

        foreach (KeyValuePair<GameObject, List<Vector3>> kvp in prefabSpawnPositions)
        {
            GameObject prefab = kvp.Key;
            List<Vector3> positions = kvp.Value;
            List<Quaternion> rotations = prefabSpawnRotations[prefab];

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Quaternion rotation = Quaternion.identity;

                if (i < rotations.Count)
                {
                    rotation = rotations[i];
                }

                GameObject generatedPrefab = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                generatedPrefab.transform.position = position;
                generatedPrefab.transform.rotation = rotation;
            }
        }

        prefabSpawnPositions.Clear();
        prefabSpawnRotations.Clear();

        UpdateTextInfo();

        Debug.Log("Generation complete!");
    }

    void UpdateTextInfo()
    {
        string spawnPositionsStr = "";

        foreach (KeyValuePair<GameObject, List<Vector3>> kvp in prefabSpawnPositions)
        {
            GameObject prefab = kvp.Key;
            List<Vector3> positions = kvp.Value;

            spawnPositionsStr += "Prefab: " + prefab.name + "\n";

            for (int i = 0; i < positions.Count; i++)
            {
                Vector3 position = positions[i];
                Quaternion rotation = Quaternion.identity;

                if (i < prefabSpawnRotations[prefab].Count)
                {
                    rotation = prefabSpawnRotations[prefab][i];
                }

                spawnPositionsStr += "  - Position: " + position.ToString() + " Rotation: " + rotation.eulerAngles.ToString() + "\n";
            }
        }

        prefabName = prefabToAdd != null ? prefabToAdd.name : "";
        spawnPositionsText = spawnPositionsStr;
    }
}