using CocClear.Core;
using UnityEngine;

namespace CocClear.Runtime
{
    /// <summary>Small, asset-free visual novel shell. It stays playable while art and final UI are undecided.</summary>
    public sealed class VisualNovelController : MonoBehaviour
    {
        private const string SaveKey = "CocClear.VisualNovel.PrologueIndex";

        private NarrativeSequence sequence;
        private GUIStyle titleStyle;
        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle buttonStyle;

        private void Awake()
        {
            sequence = new NarrativeSequence(PrologueScript.Create());
            Load();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Next();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Previous();
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            var width = Mathf.Min(Screen.width - 48f, 1120f);
            var left = (Screen.width - width) * 0.5f;
            var dialogueHeight = Mathf.Min(310f, Screen.height * 0.42f);
            var top = Screen.height - dialogueHeight - 32f;

            GUI.Box(new Rect(0f, 0f, Screen.width, Screen.height), GUIContent.none);
            GUI.Label(new Rect(left, 32f, width, 42f), "CoC-Clear", titleStyle);
            GUI.Label(new Rect(left, 76f, width, 28f), "프롤로그 · 잔향의 도시", speakerStyle);

            GUI.Box(new Rect(left, top, width, dialogueHeight), GUIContent.none);
            GUI.Label(new Rect(left + 28f, top + 22f, width - 56f, 34f), sequence.Current.Speaker, speakerStyle);
            GUI.Label(new Rect(left + 28f, top + 68f, width - 56f, dialogueHeight - 135f), sequence.Current.Text, dialogueStyle);

            var buttonY = top + dialogueHeight - 50f;
            if (GUI.Button(new Rect(left + 28f, buttonY, 96f, 30f), "이전", buttonStyle)) Previous();
            if (GUI.Button(new Rect(left + 132f, buttonY, 96f, 30f), "다음", buttonStyle)) Next();
            if (GUI.Button(new Rect(left + width - 300f, buttonY, 82f, 30f), "저장", buttonStyle)) Save();
            if (GUI.Button(new Rect(left + width - 210f, buttonY, 82f, 30f), "불러오기", buttonStyle)) Load();
            if (GUI.Button(new Rect(left + width - 120f, buttonY, 92f, 30f), "처음부터", buttonStyle)) ResetProgress();

            GUI.Label(new Rect(left, top - 28f, width, 22f), $"{sequence.CurrentIndex + 1} / {sequence.Count}   Space 또는 Enter로 다음", speakerStyle);
        }

        private void Next()
        {
            if (sequence.MoveNext()) Save();
        }

        private void Previous()
        {
            if (sequence.MovePrevious()) Save();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SaveKey, sequence.CurrentIndex);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            sequence.SetIndex(PlayerPrefs.GetInt(SaveKey, 0));
        }

        private void ResetProgress()
        {
            sequence.SetIndex(0);
            Save();
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontSize = 32, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.84f, 0.91f, 1f) } };
            speakerStyle = new GUIStyle(GUI.skin.label) { fontSize = 18, normal = { textColor = new Color(0.68f, 0.82f, 0.95f) } };
            dialogueStyle = new GUIStyle(GUI.skin.label) { fontSize = 26, wordWrap = true, normal = { textColor = Color.white } };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 14 };
        }
    }
}
