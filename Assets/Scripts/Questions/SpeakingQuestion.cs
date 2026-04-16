using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using TextSpeech;
using System;
using UnityEditor.UIElements;

public class SpeakingQuestion : QuestionBase
{
    private Label promptLabel;
    private Label titleLabel;
    private Button playSampleBtn;
    private Button tapToSpeakBtn;
    private Button checkBtn;
    private string recordStr = null;
    private string sampleAnswer;

    private IVisualElementScheduledItem speakPulseItem;
    private bool isSpeaking;
    private int _pointerID = -1;
    bool IsActive => _pointerID >= 0;

    private Dragger dragger;
    public SpeakingQuestion(VisualElement page, JToken data)
        : base(page, data)
    {
        InitPage();
    }

    protected override void InitPage()
    {
        // ---- Query UI ----
        titleLabel      = pageInstance.Q<Label>("TitleLabel");
        promptLabel     = pageInstance.Q<Label>("PromptLabel");
        playSampleBtn   = pageInstance.Q<Button>("PlaySampleBtn");
        tapToSpeakBtn   = pageInstance.Q<Button>("TapToSpeakBtn");
        tapToSpeakBtn.RemoveManipulator(tapToSpeakBtn.clickable);
        dragger = new Dragger(() =>
        {
            StartSpeakPulse(tapToSpeakBtn);
            StartRecording();
            Debug.Log("Start record...");

        }, 
        () =>
        {
            StopSpeakPulse(tapToSpeakBtn);
            StopRecording();
            Debug.Log("Stop record...");
        });
        tapToSpeakBtn.AddManipulator(dragger);
        SpeechToText.Instance.onResultCallback = OnResultSpeech;

        
        if (promptLabel == null || tapToSpeakBtn == null)
        {
            Debug.LogError("[SpeakingQuestion] Required UI elements missing.");
            return;
        }

        // ---- Read JSON ----
        string prompt = questionData.Value<string>("prompt");
        sampleAnswer  = questionData.Value<string>("sampleAnswer");

        // ---- Assign UI ----
        if (titleLabel != null)
        {
            if (sampleAnswer == "")
            {
                titleLabel.text = "Speak this sentence";
                sampleAnswer = prompt;
            }else
            {
                titleLabel.text = "Speak out the answer";
            }
        }

        promptLabel.text = prompt;

        // ---- Audio sample playback (optional hook) ----
        if (playSampleBtn != null)
        {
            playSampleBtn.clicked += () =>
            {
                // TODO: play TTS or prerecorded sampleAnswer
                // Example later:
                // SpeechAudioPlayer.Play(sampleAnswer);
            };
        }
        checkBtn = pageInstance.Q<Button>("CheckButton");
        checkBtn.clicked += () => {
#if UNITY_EDITOR
            recordStr = "Paracetamol";
#else
            if (checkBtn.ClassListContains("isDisabled"))
            {
                return;
            }
#endif
            onCheck(sampleAnswer == recordStr, sampleAnswer);
        };
    }

    public void StartRecording()
    {
#if UNITY_EDITOR
#else
        SpeechToText.Instance.StartRecording("Speak any");
#endif
    }

    public void StopRecording()
    {
#if UNITY_EDITOR
        // OnResultSpeech("Not support in editor.");
#else
        SpeechToText.Instance.StopRecording();
#endif
    }

    void OnResultSpeech(string _data)
    {
        recordStr = _data;
        if (!string.IsNullOrWhiteSpace(recordStr))
        {
            checkBtn.RemoveFromClassList("isDisabled");
        }
    }

    private void StartSpeakPulse(Button speakBtn)
    {
        if (isSpeaking)
            return;

        isSpeaking = true;

        // Ensure clean start
        speakBtn.RemoveFromClassList("SpeakButton--Enlarge");
        Debug.Log("Start anim");
        speakPulseItem = speakBtn.schedule
            .Execute(() =>
            {
                speakBtn.ToggleInClassList("SpeakButton--Enlarge");
            })
            .Every(300); // must match or slightly exceed transition-duration
    }

    private void StopSpeakPulse(Button speakBtn)
    {
        isSpeaking = false;

        speakPulseItem?.Pause();
        speakPulseItem = null;

        // Reset visual state
        speakBtn.RemoveFromClassList("SpeakButton--Enlarge");
        speakBtn.style.scale = Vector2.one;
    }

    public override void LeavePage(Action<VisualElement> cb)
    {
        if (tapToSpeakBtn != null)
        {
            tapToSpeakBtn.RemoveManipulator(dragger);
            StopSpeakPulse(tapToSpeakBtn);
        }
        base.LeavePage(cb);
    }
}
