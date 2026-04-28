using UnityEditor;
using UnityEngine;

public static class VacuumableSetup
{
    // Modifies the prefab asset so ALL instances of that prefab are affected.
    // Select one chair → every chair of that type in the scene becomes vacuumable.
    [MenuItem("AIPhobia/Make Selected Objects Vacuumable (Edit Prefab)")]
    static void MakeSelectedVacuumableViaPrefab()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            Debug.LogWarning("[VacuumableSetup] No objects selected.");
            return;
        }

        int prefabsModified = 0;
        int instancesModified = 0;

        foreach (GameObject go in selected)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

            if (!string.IsNullOrEmpty(prefabPath) && prefabPath.EndsWith(".prefab"))
            {
                // True prefab asset — modify it so every instance in the scene is affected.
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                ApplyVacuumable(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                prefabsModified++;
                Debug.Log($"[VacuumableSetup] Modified prefab asset: {prefabPath}");
            }
            else
            {
                // FBX/model source or plain scene object — modify this instance directly.
                ApplyVacuumable(go);
                instancesModified++;
                Debug.Log($"[VacuumableSetup] Modified scene object: {go.name} (source: {(string.IsNullOrEmpty(prefabPath) ? "none" : prefabPath)})");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[VacuumableSetup] Done — {prefabsModified} prefab(s) and {instancesModified} scene object(s) updated.");
    }

    static void ApplyVacuumable(GameObject go)
    {
        // 1. Remove Static flag
        GameObjectUtility.SetStaticEditorFlags(go, 0);

        // 2. Add BoxCollider if no collider exists
        if (go.GetComponent<Collider>() == null)
            go.AddComponent<BoxCollider>();

        // 3. Add Rigidbody
        if (go.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.mass = 10f;
            rb.linearDamping = 3f;
            rb.angularDamping = 1f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }

        // 4. Add Vacuumable
        if (go.GetComponent<Vacuumable>() == null)
            go.AddComponent<Vacuumable>();
    }

    // -------------------------------------------------------------------------

    [MenuItem("AIPhobia/Make Selected Objects Un-Vacuumable (Edit Prefab)")]
    static void MakeSelectedUnVacuumableViaPrefab()
    {
        GameObject[] selected = Selection.gameObjects;

        if (selected.Length == 0)
        {
            Debug.LogWarning("[VacuumableSetup] No objects selected.");
            return;
        }

        int prefabsModified = 0;
        int instancesModified = 0;

        foreach (GameObject go in selected)
        {
            string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go);

            if (!string.IsNullOrEmpty(prefabPath) && prefabPath.EndsWith(".prefab"))
            {
                GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
                RemoveVacuumable(prefabRoot);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                prefabsModified++;
                Debug.Log($"[VacuumableSetup] Reverted prefab asset: {prefabPath}");
            }
            else
            {
                RemoveVacuumable(go);
                instancesModified++;
                Debug.Log($"[VacuumableSetup] Reverted scene object: {go.name}");
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"[VacuumableSetup] Done — {prefabsModified} prefab(s) and {instancesModified} scene object(s) reverted.");
    }

    static void RemoveVacuumable(GameObject go)
    {
        // 1. Restore Static flag
        GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.ContributeGI |
                                                    StaticEditorFlags.OccluderStatic |
                                                    StaticEditorFlags.BatchingStatic |
                                                    StaticEditorFlags.OccludeeStatic);

        // 2. Remove Vacuumable
        Vacuumable vacuumable = go.GetComponent<Vacuumable>();
        if (vacuumable != null)
            Object.DestroyImmediate(vacuumable, true);

        // 3. Remove Rigidbody
        Rigidbody rb = go.GetComponent<Rigidbody>();
        if (rb != null)
            Object.DestroyImmediate(rb, true);
    }
}
