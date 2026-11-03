using UnityEngine;

/// <summary>
/// Persisted player settings (sound volumes).
/// </summary>
public static class GameSettings
{
    private const string MasterKey = "GridDungeon.MasterVolume";
    private const string SfxKey = "GridDungeon.SfxVolume";

    /// <summary>
    /// Overall volume, applied to everything that is heard.
    /// </summary>
    public static float MasterVolume
    {
        get => PlayerPrefs.GetFloat(MasterKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(MasterKey, Mathf.Clamp01(value));
            ApplyAudio();
        }
    }

    /// <summary>
    /// Volume of sound effects (hits, footsteps, ...) on top of the master volume.
    /// </summary>
    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, 1f);
        set => PlayerPrefs.SetFloat(SfxKey, Mathf.Clamp01(value));
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ApplyAudio()
    {
        AudioListener.volume = MasterVolume;
    }

    public static void Save()
    {
        PlayerPrefs.Save();
    }
}
