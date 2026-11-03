using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main menu, pause menu and settings (volume + keybinds). Builds its own UI at startup and
/// pauses the game while a menu is open.
/// </summary>
public class MenuController : MonoBehaviour
{
    private enum MenuScreen
    {
        None,
        Main,
        Pause,
        Settings
    }

    private static MenuController instance;

    private readonly Dictionary<InputAction, Button> keyButtons = new();

    private GameObject mainPanel;
    private GameObject pausePanel;
    private GameObject settingsPanel;

    private MenuScreen screen = MenuScreen.None;
    private MenuScreen settingsReturnScreen = MenuScreen.Main;
    private InputAction rebindingAction;
    private int rebindEndFrame = -10;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (instance != null)
        {
            return;
        }

        var go = new GameObject("Menus");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<MenuController>();
    }

    private void Awake()
    {
        BuildUI();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        Show(MenuScreen.Main);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        GameState.SetPaused(false);
    }

    private void Update()
    {
        if (!GameInput.Pause.WasPressedThisFrame() || IsRebinding())
        {
            return;
        }

        switch (screen)
        {
            case MenuScreen.None:
                Show(MenuScreen.Pause);
                break;
            case MenuScreen.Pause:
                Show(MenuScreen.None);
                break;
            case MenuScreen.Settings:
                CloseSettings();
                break;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Coming back from "Main Menu": the scene was reloaded, show the title screen again.
        if (mode == LoadSceneMode.Single && Time.frameCount > 1)
        {
            Show(MenuScreen.Main);
        }
    }

    // The Escape press that cancels a rebind must not also close the menu.
    private bool IsRebinding()
    {
        return rebindingAction != null || Time.frameCount <= rebindEndFrame + 1;
    }

    private void Show(MenuScreen next)
    {
        screen = next;
        mainPanel.SetActive(next == MenuScreen.Main);
        pausePanel.SetActive(next == MenuScreen.Pause);
        settingsPanel.SetActive(next == MenuScreen.Settings);

        GameState.SetPaused(next != MenuScreen.None);
    }

    #region Actions

    private void OpenSettings()
    {
        settingsReturnScreen = screen;
        RefreshKeyButtons();
        Show(MenuScreen.Settings);
    }

    private void CloseSettings()
    {
        GameSettings.Save();
        Show(settingsReturnScreen);
    }

    private void ReturnToMainMenu()
    {
        Show(MenuScreen.Main);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BeginRebind(InputAction action)
    {
        if (rebindingAction != null)
        {
            return;
        }

        rebindingAction = action;
        TMP_Text label = MenuUI.GetButtonLabel(keyButtons[action]);
        label.text = "Press a key...";

        GameInput.StartRebind(action, () =>
        {
            rebindingAction = null;
            rebindEndFrame = Time.frameCount;
            RefreshKeyButtons();
        });
    }

    private void RefreshKeyButtons()
    {
        foreach (KeyValuePair<InputAction, Button> pair in keyButtons)
        {
            MenuUI.GetButtonLabel(pair.Value).text = GameInput.GetBindingLabel(pair.Key);
        }
    }

    #endregion

    #region UI construction

    private void BuildUI()
    {
        var canvasObject = new GameObject("Menu Canvas");
        canvasObject.transform.SetParent(transform, false);

        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;

        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        mainPanel = BuildMainPanel(canvas.transform);
        pausePanel = BuildPausePanel(canvas.transform);
        settingsPanel = BuildSettingsPanel(canvas.transform);
    }

    private GameObject BuildMainPanel(Transform parent)
    {
        RectTransform panel = MenuUI.CreateFullscreenPanel(parent, "Main Menu");
        RectTransform column = MenuUI.CreateColumn(panel, 560f, 20f);

        MenuUI.CreateLabel(column, "GRID DUNGEON", 64f);
        MenuUI.CreateLabel(column, string.Empty, 30f);
        MenuUI.CreateButton(column, "Play", () => Show(MenuScreen.None));
        MenuUI.CreateButton(column, "Settings", OpenSettings);
        MenuUI.CreateButton(column, "Quit", QuitGame);
        return panel.gameObject;
    }

    private GameObject BuildPausePanel(Transform parent)
    {
        RectTransform panel = MenuUI.CreateFullscreenPanel(parent, "Pause Menu");
        RectTransform column = MenuUI.CreateColumn(panel, 460f, 20f);

        MenuUI.CreateLabel(column, "PAUSED", 72f);
        MenuUI.CreateLabel(column, string.Empty, 30f);
        MenuUI.CreateButton(column, "Resume", () => Show(MenuScreen.None));
        MenuUI.CreateButton(column, "Settings", OpenSettings);
        MenuUI.CreateButton(column, "Main Menu", ReturnToMainMenu);
        MenuUI.CreateButton(column, "Quit", QuitGame);
        return panel.gameObject;
    }

    private GameObject BuildSettingsPanel(Transform parent)
    {
        RectTransform panel = MenuUI.CreateFullscreenPanel(parent, "Settings Menu");
        RectTransform column = MenuUI.CreateColumn(panel, 820f, 14f);

        MenuUI.CreateLabel(column, "SETTINGS", 64f);

        MenuUI.CreateLabel(column, "Sound", 36f, TextAlignmentOptions.MidlineLeft);
        MenuUI.CreateSliderRow(column, "Master", GameSettings.MasterVolume, v => GameSettings.MasterVolume = v);
        MenuUI.CreateSliderRow(column, "Effects", GameSettings.SfxVolume, v => GameSettings.SfxVolume = v);

        MenuUI.CreateLabel(column, "Controls", 36f, TextAlignmentOptions.MidlineLeft);
        foreach (InputAction action in GameInput.Rebindable)
        {
            InputAction captured = action;
            keyButtons[action] = MenuUI.CreateKeyRow(column, action.name, () => BeginRebind(captured));
        }

        MenuUI.CreateButton(column, "Reset Controls", () =>
        {
            GameInput.ResetBindings();
            RefreshKeyButtons();
        }, 56f);
        MenuUI.CreateButton(column, "Back", CloseSettings);
        return panel.gameObject;
    }

    #endregion
}
