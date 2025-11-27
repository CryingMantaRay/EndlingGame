using System.Collections;
using System.Collections.Generic;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueBox : MonoBehaviour
{
    [Title("References")]
    public GameObject dialogueBox;
    public TMP_Text speakerText;
    public TMP_Text dialogueText;
    public Image frameImage;

    [Title("Settings")]
    public float textSpeed = 0.05f;
    public KeyCode continueKey = KeyCode.Mouse0;
    public List<DialogueConversation> conversations = new();

    [Title("Events")]
    public UnityEvent OnStartAnyDialogue;
    public UnityEvent OnEndAnyDialogue;

    int currentConversationIndex;
    int currentDialogueIndex;

    bool isTyping;
    bool canContinue;

    Coroutine dialogueRoutine;

    void Start()
    {
        ShowDialogueBox(false);
    }

    void Update()
    {
        if (!dialogueBox || !dialogueBox.activeInHierarchy)
            return;

        if (canContinue && Input.GetKeyDown(continueKey))
            AdvanceDialogue();
    }

    public void ShowDialogueBox(bool show)
    {
        if (!dialogueBox)
            return;

        dialogueBox.SetActive(show);
    }

    public void StartConversation(int conversationIndex)
    {
        if (conversationIndex < 0 || conversationIndex >= conversations.Count)
            return;

        DialogueConversation conversation = conversations[conversationIndex];
        if (!conversation || conversation.dialogues.Count == 0)
            return;

        OnStartAnyDialogue.Invoke();

        currentConversationIndex = conversationIndex;
        currentDialogueIndex = 0;

        if (conversation.talkIndicator)
            conversation.talkIndicator.SetActive(false);

        ShowDialogueBox(true);

        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(PlayCurrentDialogue());
    }

    IEnumerator PlayCurrentDialogue()
    {
        isTyping = true;
        canContinue = false;

        DialogueConversation conversation = conversations[currentConversationIndex];

        if (!conversation || conversation.dialogues.Count == 0)
        {
            isTyping = false;
            canContinue = false;
            yield break;
        }

        if (currentDialogueIndex < 0 || currentDialogueIndex >= conversation.dialogues.Count)
        {
            isTyping = false;
            canContinue = false;
            yield break;
        }

        DialogueDefinition dialogue = conversation.dialogues[currentDialogueIndex];

        if (!dialogue)
        {
            isTyping = false;
            canContinue = false;
            yield break;
        }

        if (dialogueText)
            dialogueText.text = string.Empty;

        if (dialogue.onDialogueStart != null)
            dialogue.onDialogueStart.Invoke();

        for (int i = 0; i < dialogue.chunks.Count; i++)
        {
            DialogueChunk chunk = dialogue.chunks[i];

            if (chunk == null)
                continue;

            // Per-chunk speaker and frame from DialogueSpeakers
            if (speakerText && !string.IsNullOrEmpty(dialogue.speakerName))
                speakerText.text = dialogue.speakerName;

            if (frameImage && dialogue.dialogueSpeakers)
            {
                Sprite frame = dialogue.dialogueSpeakers.GetFrameSprite(dialogue.speakerName);
                if (frame)
                    frameImage.sprite = frame;
            }

            if (chunk.OnChunkStart != null)
                chunk.OnChunkStart.Invoke();

            if (!string.IsNullOrEmpty(chunk.ChunkText))
            {
                foreach (char c in chunk.ChunkText)
                {
                    if (dialogueText)
                        dialogueText.text += c;

                    yield return new WaitForSeconds(textSpeed);
                }
            }

            if (dialogue.autoAddSpacePerChunk && dialogueText)
                dialogueText.text += " ";

            if (chunk.OnChunkEnd != null)
                chunk.OnChunkEnd.Invoke();

            if (chunk.DelayAfterChunk > 0f && i < dialogue.chunks.Count - 1)
                yield return new WaitForSeconds(chunk.DelayAfterChunk);
        }

        isTyping = false;
        canContinue = true;
    }

    void AdvanceDialogue()
    {
        DialogueConversation conversation = conversations[currentConversationIndex];

        if (!conversation || conversation.dialogues.Count == 0)
            return;

        if (currentDialogueIndex < 0 || currentDialogueIndex >= conversation.dialogues.Count)
            return;

        DialogueDefinition dialogue = conversation.dialogues[currentDialogueIndex];

        if (dialogue && dialogue.onDialogueEnd != null)
            dialogue.onDialogueEnd.Invoke();

        currentDialogueIndex++;

        if (currentDialogueIndex >= conversation.dialogues.Count)
        {
            EndConversation(conversation);
            return;
        }

        if (dialogueRoutine != null)
            StopCoroutine(dialogueRoutine);

        dialogueRoutine = StartCoroutine(PlayCurrentDialogue());
    }

    void EndConversation(DialogueConversation conversation)
    {
        ShowDialogueBox(false);

        if (conversation.talkIndicator)
        {
            Interactable interactable = conversation.talkIndicator.GetComponentInParent<Interactable>();
            if (interactable && interactable.CanInteract)
                conversation.talkIndicator.SetActive(true);
        }

        isTyping = false;
        canContinue = false;

        if (dialogueRoutine != null)
        {
            StopCoroutine(dialogueRoutine);
            dialogueRoutine = null;
        }

        OnEndAnyDialogue.Invoke();
    }
}
