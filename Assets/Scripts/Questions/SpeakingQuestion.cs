using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Newtonsoft.Json.Linq;
using TextSpeech;

public class SpeakingQuestion : QuestionBase
{
    private Label promptLabel;
    private Label titleLabel;
    private Button playSampleBtn;
    private Button tapToSpeakBtn;
    private Button checkBtn;
    private string recordStr = null;
    private string sampleAnswer;

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
        SpeechToText.Instance.onResultCallback = OnResultSpeech;

        tapToSpeakBtn.RegisterCallback<PointerDownEvent>(e =>
        {
            StartRecording();
        }, TrickleDown.TrickleDown);
        tapToSpeakBtn.RegisterCallback<PointerUpEvent>(e =>
        {
            StopRecording();
        }, TrickleDown.TrickleDown);
        
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
}
