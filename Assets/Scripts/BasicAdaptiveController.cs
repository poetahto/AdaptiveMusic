using UnityEngine;

public class BasicAdaptiveController : MonoBehaviour
{
    public TemporalAudioSource[] sources;
    public AudioVolumeFader[] faders;
    
    public int quantizationAmount = 4;
    public bool showGUI = true;
    private bool _fadeOut = true;
    
    public void FadeIn(int index)
    {
        QuantizeFade(faders[index], sources[index], FadeType.In);
    }

    public void FadeOut(int index)
    {
        QuantizeFade(faders[index], sources[index], FadeType.Out);
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