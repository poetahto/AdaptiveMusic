using UnityEngine;

public class BasicAdaptiveController : MonoBehaviour
{
    public TemporalAudioSource[] sources;
    public int quantizationAmount = 4;
    public bool showGUI = true;
    
    private AudioVolumeFader[] _faders;
    private bool _fadeOut = true;
    
    private void Awake()
    {
        _faders = new AudioVolumeFader[sources.Length];
        
        CreateFaders();
    }

    private void CreateFaders()
    {
        GameObject faderHolder = new GameObject("Faders");
        faderHolder.transform.parent = transform;
        
        for (int i = 0; i < sources.Length; i++)
        {
            var currentSource = sources[i];
            var newFader = faderHolder.AddComponent<AudioVolumeFader>();
            newFader.audioSource = currentSource.source;
            _faders[i] = newFader;
        }
    }

    public void FadeIn(int index)
    {
        QuantizeFade(_faders[index], sources[index], FadeType.In);
    }

    public void FadeOut(int index)
    {
        QuantizeFade(_faders[index], sources[index], FadeType.Out);
    }

    private void QuantizeFade(Fader fader, TemporalAudioSource source, FadeType type)
    {
        var beatsToWait = quantizationAmount - (int) source.TotalBeats % quantizationAmount;

        if (_fadeOut)
        {
            source.ExecuteOn(
                () => fader.Fade(type),
                (int) source.TotalBeats + beatsToWait);
        }
        else
        {
            source.ExecuteOn(
                () => source.source.volume = type == FadeType.In ? 1 : 0,
                (int) source.TotalBeats + beatsToWait);
        }
    }
    
    private void OnGUI()
    {
        if (!showGUI)
            return;
        
        if (GUILayout.Button("Sync Songs"))
        {
            var referenceBeat = (int) sources[0].TotalBeats;

            foreach (var source in sources)
                source.JumpTo(referenceBeat);
        }

        _fadeOut = GUILayout.Toggle(_fadeOut, "Fade Out");
    }
}