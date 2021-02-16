using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct TAS_Events
{
    public UnityEvent       onStartAudio;
    public UnityEvent       onStopAudio;
    public UnityEvent<int>  onBeat;    // passes the beat it was called on
}

public class TemporalAudioSource : MonoBehaviour
{
    [Header("Dependencies")]
    public AudioSource source = default;
    
    [Header("Required Audio Information")]
    public float       beatsPerMinute = 60;
    public float       firstBeatOffset = 0;
    
    [Header("Optional Settings")]
    public bool        playOnAwake = true;
    public bool        showGUI = false;
    
    [Header("Events")]
    public TAS_Events  events = default;

    public float       TotalBeats { get; private set; } = 0;
    public float       LoopBeats { get; private set; } = 0;
    public float       Seconds { get; private set; } = 0;
    public float       Loops { get; private set; } = 0;
    public float       LoopAnalog { get; private set; } = 0;
    
    private float      SecondsPerBeat { get; set; }
    private float      BeatsPerLoop { get; set; }
    private float      DspSongTime { get; set; } = 0;
    private int        LastBeatEvent { get; set; } = -1;
    
    // Key: A beat of the song
    // Value: The callbacks to execute on the requested beat: order matters! (which is why we use this data structure)
    private readonly Dictionary<int, LinkedList<Action>> _requests = new Dictionary<int, LinkedList<Action>>();
    
    // IMGUI window information
    private static int  _globalID = 0;
    private int         _localID = 0;
    private int         _playAtBeatTarget = 0;
    private Rect        _windowRect = new Rect(0, 50, 300, 300);

    public void Play()
    {
        TotalBeats = 0;
        Loops = 0;
        LoopAnalog = 0;
        DspSongTime = (float) AudioSettings.dspTime;
        
        events.onStartAudio.Invoke();
        source.Play();
    }

    public void JumpTo(int beat)
    {
        Loops = (int) (beat / BeatsPerLoop);
        source.time = SecondsPerBeat * (beat % BeatsPerLoop);
        
        // Create a fake starting time AS IF we had actually been playing this whole time
        DspSongTime = (float) AudioSettings.dspTime - SecondsPerBeat * beat;
        
        UpdatePublicValues();
    }

    public void Stop()
    {
        events.onStopAudio.Invoke();
        source.Stop();
    }

    // Executes a callback on the specified beat (loop independent)
    // TODO: an option for repeating callbacks on beats might be useful
    public void ExecuteOn(Action callback, int beat)
    {
        if (!_requests.ContainsKey(beat))
            _requests.Add(beat, new LinkedList<Action>());

        _requests[beat].AddLast(callback);
    }
    
    private void Awake()
    {
        _localID = _globalID++; // initialization for the IMGUI windows 
        
        SecondsPerBeat = 60f / beatsPerMinute;
        BeatsPerLoop = (source.clip.length - firstBeatOffset) / SecondsPerBeat;
        
        events.onBeat.AddListener(ExecuteCallbacks);
        
        if (playOnAwake)
            Play();
    }
    
    private void ExecuteCallbacks(int beat)
    {
        if (!_requests.ContainsKey(beat))
            return;

        foreach (var callback in _requests[beat])
            callback.Invoke();

        _requests.Remove(beat);
    }

    private void Update()
    {
        if (!source.isPlaying)
            return;

        CheckForLoop();
        UpdatePublicValues();
        CheckShouldFireBeatEvent();
    }

    private void CheckForLoop()
    {
        if (TotalBeats >= (Loops + 1) * BeatsPerLoop)
            Loops++;
    }
    
    private void UpdatePublicValues()
    {
        Seconds    = (float) (AudioSettings.dspTime - DspSongTime - firstBeatOffset);
        TotalBeats = Seconds / SecondsPerBeat;
        LoopBeats  = TotalBeats - Loops * BeatsPerLoop;
        LoopAnalog = LoopBeats / BeatsPerLoop;
    }

    private void CheckShouldFireBeatEvent()
    {
        // cast to int - we only fire an event when a whole beat has passed
        if (LastBeatEvent != (int) TotalBeats && TotalBeats >= 0)
        {
            // ensure that no beats are ever skipped
            for (int i = 0; i < (int) (TotalBeats - LastBeatEvent); i++)
                events.onBeat.Invoke(LastBeatEvent + i);

            LastBeatEvent = (int) TotalBeats;
        }
    }

    private void OnGUI()
    {
        if (!showGUI)
            return;
        
        _windowRect = GUI.Window(_localID, _windowRect, delegate
        {
            GUILayout.Label("Total Beats: " + TotalBeats);
            GUILayout.Label("Loop Beats: " + LoopBeats);
            GUILayout.Label("Seconds: " + Seconds);
            GUILayout.Label("Loops: " + Loops);
            GUILayout.Label("Analog Loop: " + LoopAnalog);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Play Audio"))
                Play();
            if (GUILayout.Button("Stop Audio"))
                Stop();
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();

            int.TryParse(GUILayout.TextField(_playAtBeatTarget.ToString()), out _playAtBeatTarget);

            if (GUILayout.Button("Play Audio At:"))
                JumpTo(_playAtBeatTarget);

            GUILayout.EndHorizontal();
            
            foreach (var beatCallback in _requests)
                GUILayout.Label($"Beat { beatCallback.Key }: { beatCallback.Value.Count } callbacks");    
            
            GUI.DragWindow();
            
        }, $"TemporalAudioSource: { name }");
    }
}