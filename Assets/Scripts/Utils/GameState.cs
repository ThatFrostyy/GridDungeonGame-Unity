using System;
using UnityEngine;

/// <summary>
/// Global pause state, driven by the menus.
/// </summary>
public static class GameState
{
    private static int resumeFrame = -1;

    public static bool IsPaused { get; private set; }

    /// <summary>
    /// True when gameplay input should be processed. Skips the frame the game was resumed in, so the
    /// click that pressed a menu button does not also land on the map behind it.
    /// </summary>
    public static bool AcceptsInput => !IsPaused && Time.frameCount > resumeFrame;

    public static event Action<bool> PausedChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsPaused = false;
        resumeFrame = -1;
        PausedChanged = null;
        Time.timeScale = 1f;
    }

    public static void SetPaused(bool paused)
    {
        if (IsPaused == paused)
        {
            return;
        }

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        if (!paused)
        {
            resumeFrame = Time.frameCount;
        }

        PausedChanged?.Invoke(paused);
    }
}
