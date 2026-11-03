using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Central place for the game's input actions (new Input System), with saved, rebindable keys.
/// Actions are created in code so no input asset needs to be wired up in the scene.
/// </summary>
public static class GameInput
{
    private const string SaveKey = "GridDungeon.Keybinds";

    private static InputActionMap map;
    private static InputAction click;
    private static InputAction point;
    private static InputAction endTurn;
    private static InputAction toggleInventory;
    private static InputAction pause;

    public static InputAction Click => Ensure(ref click);
    public static InputAction Point => Ensure(ref point);
    public static InputAction EndTurn => Ensure(ref endTurn);
    public static InputAction ToggleInventory => Ensure(ref toggleInventory);
    public static InputAction Pause => Ensure(ref pause);

    /// <summary>
    /// Actions the player may rebind in the settings menu.
    /// </summary>
    public static IReadOnlyList<InputAction> Rebindable => new[] { EndTurn, ToggleInventory, Pause };

    // Needed when "Enter Play Mode" skips the domain reload, otherwise stale actions survive.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        map?.Dispose();
        map = null;
        click = point = endTurn = toggleInventory = pause = null;
    }

    private static InputAction Ensure(ref InputAction action)
    {
        if (map == null)
        {
            CreateActions();
        }

        return action;
    }

    private static void CreateActions()
    {
        map = new InputActionMap("Game");
        click = map.AddAction("Click", InputActionType.Button, "<Mouse>/leftButton");
        point = map.AddAction("Point", InputActionType.Value, "<Mouse>/position");
        endTurn = map.AddAction("End Turn", InputActionType.Button, "<Keyboard>/space");
        toggleInventory = map.AddAction("Inventory", InputActionType.Button, "<Keyboard>/tab");
        pause = map.AddAction("Menu", InputActionType.Button, "<Keyboard>/escape");

        string saved = PlayerPrefs.GetString(SaveKey, string.Empty);
        if (!string.IsNullOrEmpty(saved))
        {
            map.LoadBindingOverridesFromJson(saved);
        }

        map.Enable();
    }

    /// <summary>
    /// Mouse position in screen pixels.
    /// </summary>
    public static Vector2 PointerPosition => Point.ReadValue<Vector2>();

    /// <summary>
    /// Human readable key for an action, e.g. "Space".
    /// </summary>
    public static string GetBindingLabel(InputAction action)
    {
        return action.GetBindingDisplayString(0);
    }

    /// <summary>
    /// Waits for the next key press and binds it to the action. Escape cancels.
    /// </summary>
    public static void StartRebind(InputAction action, Action onFinished)
    {
        action.Disable();

        action.PerformInteractiveRebinding(0)
            .WithControlsHavingToMatchPath("<Keyboard>")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                operation.Dispose();
                action.Enable();
                SaveBindings();
                onFinished?.Invoke();
            })
            .OnCancel(operation =>
            {
                operation.Dispose();
                action.Enable();
                onFinished?.Invoke();
            })
            .Start();
    }

    public static void ResetBindings()
    {
        Ensure(ref click);
        map.RemoveAllBindingOverrides();
        SaveBindings();
    }

    private static void SaveBindings()
    {
        PlayerPrefs.SetString(SaveKey, map.SaveBindingOverridesAsJson());
        PlayerPrefs.Save();
    }
}
