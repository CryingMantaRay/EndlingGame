using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EditorAttributes;

[System.Serializable]
public class DialogueChunk
{
    // Tab group that organizes this chunk into "General" and "Events" tabs
    [TabGroup(nameof(generalField), nameof(eventsField))] [SerializeField] Void groupHolder;

    [VerticalGroup(nameof(chunkText), nameof(delayAfterChunk))]
    [HideInInspector, SerializeField] Void generalField;

    [VerticalGroup(nameof(onChunkStart), nameof(onChunkEnd))]
    [HideInInspector, SerializeField] Void eventsField;

    [HideProperty, SerializeField, TextArea(1, 5)] string chunkText;
    [HideProperty, SerializeField] float delayAfterChunk;

    // Speaker selection per chunk, driven by dropdown values injected from DialogueDefinition

    
    [HideProperty, SerializeField] UnityEvent onChunkStart;
    [HideProperty, SerializeField] UnityEvent onChunkEnd;

    public string ChunkText => chunkText;
    public float DelayAfterChunk => delayAfterChunk;
    public UnityEvent OnChunkStart => onChunkStart;
    public UnityEvent OnChunkEnd => onChunkEnd;

    
}

public class DialogueDefinition : MonoBehaviour
{
    // Drag your DialogueSpeakers asset here
    public DialogueSpeakers dialogueSpeakers;
    [Dropdown(nameof(speakerNames))] public string speakerName;

    public bool autoAddSpacePerChunk = true;
    public List<DialogueChunk> chunks = new();
    public UnityEvent onDialogueStart;
    public UnityEvent onDialogueEnd;

    List<string> speakerNames = new();

    void OnValidate()
    {
        if (dialogueSpeakers == null)
        {
            speakerNames.Clear();
            return;
        }

        var source = dialogueSpeakers.GetSpeakerNames();
        speakerNames = source != null ? new List<string>(source) : new List<string>();
    }
}
