using UnityEngine;

public class AudioVolumeFader : Fader
{
    public AudioSource audioSource = default;

    protected override float Value
    {
        get => audioSource.volume;
        set => audioSource.volume = value;
    }

    private void Start()
    {
        if (audioSource == null)
        {
            Debug.LogWarning($"AudioFader: The GameObject \"{gameObject.name}\" has not been assigned " +
                             "an AudioSource, so it's AudioFader has been disabled.");
            FaderEnabled = false;
        }
    }
}