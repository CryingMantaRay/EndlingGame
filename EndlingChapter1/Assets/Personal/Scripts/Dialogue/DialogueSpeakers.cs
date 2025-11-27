using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Dialogue Speakers")]
public class DialogueSpeakers : ScriptableObject
{
    [System.Serializable]
    public class Speaker
    {
        public string speakerName;
        public Sprite frameSprite;
    }

    public List<Speaker> speakers = new();

    public string[] GetSpeakerNames()
    {
        string[] names = new string[speakers.Count];

        for (int i = 0; i < speakers.Count; i++)
            names[i] = speakers[i].speakerName;

        return names;
    }

    public Sprite GetFrameSprite(string name)
    {
        for (int i = 0; i < speakers.Count; i++)
        {
            if (speakers[i].speakerName == name)
                return speakers[i].frameSprite;
        }

        return null;
    }
}
