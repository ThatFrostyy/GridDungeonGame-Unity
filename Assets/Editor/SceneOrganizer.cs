using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// One-click tidy-up of the open scene: groups root objects under named folders and sorts them.
/// Run it from Tools > Grid Dungeon > Organize Scene.
/// </summary>
public static class SceneOrganizer
{
    private const string Managers = "--- Managers ---";
    private const string World = "--- World ---";
    private const string Actors = "--- Actors ---";
    private const string UI = "--- UI ---";

    [MenuItem("Tools/Grid Dungeon/Organize Scene")]
    public static void Organize()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid())
        {
            return;
        }

        Transform managers = GetOrCreateGroup(Managers);
        Transform world = GetOrCreateGroup(World);
        Transform actors = GetOrCreateGroup(Actors);
        Transform ui = GetOrCreateGroup(UI);

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Transform group = ChooseGroup(root, managers, world, actors, ui);
            if (group != null && root.transform != group)
            {
                Undo.SetTransformParent(root.transform, group, "Organize Scene");
            }
        }

        SortChildren(actors);
        SortChildren(world);
        SortChildren(managers);

        // Camera first, then the groups, so the hierarchy reads top to bottom.
        int index = 0;
        foreach (string name in new[] { "Main Camera", Managers, World, Actors, UI })
        {
            GameObject go = FindRoot(scene, name);
            if (go != null)
            {
                go.transform.SetSiblingIndex(index++);
            }
        }

        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Scene organized. Save the scene (Ctrl+S) to keep the new layout.");
    }

    private static Transform ChooseGroup(GameObject root, Transform managers, Transform world, Transform actors, Transform ui)
    {
        if (root.transform == managers || root.transform == world || root.transform == actors || root.transform == ui)
        {
            return null;
        }

        if (root.GetComponent<Camera>() != null)
        {
            return null;
        }

        if (root.GetComponentInChildren<Player>(true) != null || root.GetComponentInChildren<Enemy>(true) != null)
        {
            return actors;
        }

        if (root.GetComponent<Canvas>() != null)
        {
            return ui;
        }

        if (root.GetComponent<Grid>() != null || root.GetComponent<TileSelection>() != null)
        {
            return world;
        }

        // GameObjectLocator, TurnManager, DungeonGenerator, EventSystem, ...
        return managers;
    }

    private static Transform GetOrCreateGroup(string name)
    {
        GameObject existing = FindRoot(SceneManager.GetActiveScene(), name);
        if (existing != null)
        {
            return existing.transform;
        }

        var group = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(group, "Create group");
        return group.transform;
    }

    private static GameObject FindRoot(Scene scene, string name)
    {
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            if (root.name == name)
            {
                return root;
            }
        }

        return null;
    }

    private static void SortChildren(Transform parent)
    {
        var children = new List<Transform>();
        foreach (Transform child in parent)
        {
            children.Add(child);
        }

        children.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
        }
    }
}
