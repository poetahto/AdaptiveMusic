using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BasicAdaptiveController))]
public class BasicAdaptiveControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Generate Faders"))
            GenerateFaders();
    }

    private void GenerateFaders()
    {
        var controller = (BasicAdaptiveController) target;
        var faderHolder = new GameObject("Faders");
        
        controller.faders = new AudioVolumeFader[controller.sources.Length];
        faderHolder.transform.parent = controller.transform;
        
        for (int i = 0; i < controller.sources.Length; i++)
        {
            var currentSource = controller.sources[i];
            var newFader = faderHolder.AddComponent<AudioVolumeFader>();
            newFader.audioSource = currentSource.source;
            controller.faders[i] = newFader;
        }
    }
}
