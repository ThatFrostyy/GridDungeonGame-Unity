using UnityEngine;

public static class AudioUtils
{
    private const float MinPitch = 0.8f;
    private const float MaxPitch = 1.2f;

    /// <summary>
    /// Plays a clip once, optionally with a random pitch. Safe to call with missing references.
    /// </summary>
    public static void PlayOneShot(AudioSource source, AudioClip clip, bool randomizePitch = false)
    {
        if (source == null || clip == null)
        {
            return;
        }

        // Pitch is a property of the source, so it must be set before the clip plays
        // and stay set (resetting it right away would cancel the variation).
        source.pitch = randomizePitch ? Random.Range(MinPitch, MaxPitch) : 1f;
        source.PlayOneShot(clip, GameSettings.SfxVolume);
    }
}
