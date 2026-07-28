using CocClear.Core;
using UnityEngine;

namespace CocClear.Runtime
{
    /// <summary>Asset-free title and visual-novel shell. Final illustration can replace DrawTitleBackground later.</summary>
    public sealed class VisualNovelController : MonoBehaviour
    {
        private const string SaveKey = "CocClear.VisualNovel.PrologueIndex";
        private const string DialogueSizeKey = "CocClear.VisualNovel.DialogueSize";
        private const int DefaultDialogueSize = 26;

        private enum ScreenMode
        {
            Title,
            Game,
            Archive,
            Settings,
        }

        private enum ArchiveTab
        {
            Records,
            Illustrations,
        }

        private NarrativeSequence sequence;
        private ScreenMode screenMode;
        private ArchiveTab archiveTab;
        private Vector2 archiveScrollPosition;
        private Texture2D[] galleryImages;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle speakerStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle menuButtonStyle;
        private GUIStyle bodyStyle;

        private void Awake()
        {
            sequence = new NarrativeSequence(PrologueScript.Create());
            galleryImages = Resources.LoadAll<Texture2D>("Archive");
            screenMode = ScreenMode.Title;
        }

        private void Update()
        {
            if (screenMode != ScreenMode.Game)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                Next();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Previous();
            }
            else if (Input.GetKeyDown(KeyCode.Escape))
            {
                screenMode = ScreenMode.Title;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();

            switch (screenMode)
            {
                case ScreenMode.Title:
                    DrawTitleScreen();
                    break;
                case ScreenMode.Game:
                    DrawGameScreen();
                    break;
                case ScreenMode.Archive:
                    DrawArchiveScreen();
                    break;
                case ScreenMode.Settings:
                    DrawSettingsScreen();
                    break;
            }
        }

        private void DrawTitleScreen()
        {
            DrawTitleBackground();

            var left = Mathf.Clamp(Screen.width * 0.09f, 42f, 180f);
            GUI.Label(new Rect(left, 56f, 520f, 62f), "CoC-Clear", titleStyle);
            GUI.Label(new Rect(left + 4f, 120f, 460f, 28f), "기록되지 않은 하루의 끝", subtitleStyle);
            GUI.Label(new Rect(left + 4f, 154f, 580f, 24f), "임시 타이틀 배경 · 일러스트가 도착하면 교체됩니다", speakerStyle);

            var menuBottom = Screen.height - 82f;
            var menuTop = Mathf.Max(240f, menuBottom - 210f);
            if (DrawMenuButton("처음부터", left, menuTop)) StartNewGame();
            if (DrawMenuButton("이어하기", left, menuTop + 52f, HasSave)) ContinueGame();
            if (DrawMenuButton("보관함", left, menuTop + 104f)) screenMode = ScreenMode.Archive;
            if (DrawMenuButton("설정", left, menuTop + 156f)) screenMode = ScreenMode.Settings;

            var saveMessage = HasSave
                ? $"이어하기: 프롤로그 {PlayerPrefs.GetInt(SaveKey) + 1}번째 대사"
                : "저장된 진행이 없습니다";
            GUI.Label(new Rect(left, menuBottom, 400f, 24f), saveMessage, speakerStyle);
        }

        private void DrawGameScreen()
        {
            DrawGameBackground();

            var width = Mathf.Min(Screen.width - 48f, 1120f);
            var left = (Screen.width - width) * 0.5f;
            var dialogueHeight = Mathf.Min(310f, Screen.height * 0.42f);
            var top = Screen.height - dialogueHeight - 32f;

            GUI.Label(new Rect(left, 28f, width, 42f), "CoC-Clear", titleStyle);
            GUI.Label(new Rect(left, 74f, width, 28f), "프롤로그 · 잔향의 도시", speakerStyle);
            DrawRect(new Rect(left, top, width, dialogueHeight), new Color(0.025f, 0.04f, 0.08f, 0.93f));
            GUI.Label(new Rect(left + 28f, top + 22f, width - 56f, 34f), sequence.Current.Speaker, speakerStyle);
            dialogueStyle.fontSize = PlayerPrefs.GetInt(DialogueSizeKey, DefaultDialogueSize);
            GUI.Label(new Rect(left + 28f, top + 68f, width - 56f, dialogueHeight - 135f), sequence.Current.Text, dialogueStyle);

            var buttonY = top + dialogueHeight - 50f;
            if (GUI.Button(new Rect(left + 28f, buttonY, 96f, 30f), "이전", menuButtonStyle)) Previous();
            if (GUI.Button(new Rect(left + 132f, buttonY, 96f, 30f), "다음", menuButtonStyle)) Next();
            if (GUI.Button(new Rect(left + width - 390f, buttonY, 82f, 30f), "저장", menuButtonStyle)) Save();
            if (GUI.Button(new Rect(left + width - 300f, buttonY, 82f, 30f), "불러오기", menuButtonStyle)) Load();
            if (GUI.Button(new Rect(left + width - 210f, buttonY, 82f, 30f), "설정", menuButtonStyle)) screenMode = ScreenMode.Settings;
            if (GUI.Button(new Rect(left + width - 120f, buttonY, 92f, 30f), "타이틀", menuButtonStyle)) screenMode = ScreenMode.Title;

            GUI.Label(new Rect(left, top - 28f, width, 22f), $"{sequence.CurrentIndex + 1} / {sequence.Count}   Space 또는 Enter로 다음 · Esc로 타이틀", speakerStyle);
        }

        private void DrawArchiveScreen()
        {
            DrawPanelBackground("보관함", "모든 시나리오 기록과 전달받은 일러스트를 전시합니다.");

            var width = Mathf.Min(Screen.width - 96f, 900f);
            var left = (Screen.width - width) * 0.5f;
            if (GUI.Button(new Rect(left, 177f, 130f, 34f), "시나리오 기록", menuButtonStyle)) archiveTab = ArchiveTab.Records;
            if (GUI.Button(new Rect(left + 140f, 177f, 130f, 34f), "일러스트", menuButtonStyle)) archiveTab = ArchiveTab.Illustrations;

            if (archiveTab == ArchiveTab.Records)
            {
                DrawArchiveRecords(left, width);
            }
            else
            {
                DrawArchiveGallery(left, width);
            }

            if (GUI.Button(new Rect(left, Screen.height - 78f, 140f, 36f), "타이틀로", menuButtonStyle)) screenMode = ScreenMode.Title;
        }

        private void DrawArchiveRecords(float left, float width)
        {
            var savedText = HasSave
                ? $"저장된 진행: 프롤로그 {PlayerPrefs.GetInt(SaveKey) + 1}번째 대사"
                : "저장된 진행이 없습니다. 게임 화면에서 저장을 눌러 기록하세요.";
            GUI.Label(new Rect(left, 220f, width, 30f), savedText, bodyStyle);

            var records = ScenarioArchive.CreateAll();
            var viewRect = new Rect(left, 258f, width, Screen.height - 356f);
            var contentHeight = 54f + (records.Length * 58f);
            archiveScrollPosition = GUI.BeginScrollView(viewRect, archiveScrollPosition, new Rect(0f, 0f, width - 24f, contentHeight));
            var y = 0f;
            var previousEpisode = string.Empty;
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (previousEpisode != record.EpisodeTitle)
                {
                    GUI.Label(new Rect(12f, y, width - 56f, 28f), record.EpisodeTitle, subtitleStyle);
                    y += 32f;
                    previousEpisode = record.EpisodeTitle;
                }

                DrawRect(new Rect(8f, y, width - 48f, 48f), new Color(0.06f, 0.11f, 0.2f, 0.72f));
                GUI.Label(new Rect(20f, y + 5f, width - 72f, 19f), $"{record.Order}. {record.Line.Speaker}", speakerStyle);
                GUI.Label(new Rect(20f, y + 25f, width - 72f, 21f), record.Line.Text, speakerStyle);
                y += 56f;
            }

            GUI.EndScrollView();
        }

        private void DrawArchiveGallery(float left, float width)
        {
            GUI.Label(new Rect(left, 220f, width, 30f), $"전시된 일러스트: {galleryImages.Length}장", bodyStyle);
            if (galleryImages.Length == 0)
            {
                GUI.Label(new Rect(left, 260f, width, 58f), "아직 전시할 일러스트가 없습니다. 다음에 전달해 주는 일러스트부터 전부 보관함에 등록합니다.", subtitleStyle);
                return;
            }

            const float cellWidth = 200f;
            const float cellHeight = 168f;
            var columns = Mathf.Max(1, Mathf.FloorToInt((width - 20f) / cellWidth));
            var rows = Mathf.CeilToInt(galleryImages.Length / (float)columns);
            var viewRect = new Rect(left, 258f, width, Screen.height - 356f);
            archiveScrollPosition = GUI.BeginScrollView(viewRect, archiveScrollPosition, new Rect(0f, 0f, width - 24f, rows * cellHeight));
            for (var i = 0; i < galleryImages.Length; i++)
            {
                var column = i % columns;
                var row = i / columns;
                var x = 8f + (column * cellWidth);
                var y = row * cellHeight;
                DrawRect(new Rect(x, y, cellWidth - 12f, cellHeight - 12f), new Color(0.06f, 0.11f, 0.2f, 0.82f));
                GUI.DrawTexture(new Rect(x + 8f, y + 8f, cellWidth - 28f, 112f), galleryImages[i], ScaleMode.ScaleToFit, true);
                GUI.Label(new Rect(x + 8f, y + 126f, cellWidth - 28f, 26f), galleryImages[i].name, speakerStyle);
            }

            GUI.EndScrollView();
        }

        private void DrawSettingsScreen()
        {
            DrawPanelBackground("설정", "임시 설정입니다. 정식 옵션 메뉴로 교체할 수 있습니다.");

            var width = Mathf.Min(Screen.width - 96f, 900f);
            var left = (Screen.width - width) * 0.5f;
            var top = 186f;
            var fontSize = PlayerPrefs.GetInt(DialogueSizeKey, DefaultDialogueSize);
            GUI.Label(new Rect(left, top, width, 32f), "대사 글자 크기", bodyStyle);
            GUI.Label(new Rect(left, top + 45f, 120f, 32f), $"{fontSize}px", speakerStyle);
            if (GUI.Button(new Rect(left + 140f, top + 40f, 42f, 34f), "−", menuButtonStyle)) SetDialogueSize(fontSize - 2);
            if (GUI.Button(new Rect(left + 190f, top + 40f, 42f, 34f), "+", menuButtonStyle)) SetDialogueSize(fontSize + 2);
            if (GUI.Button(new Rect(left + 246f, top + 40f, 106f, 34f), "기본값", menuButtonStyle)) SetDialogueSize(DefaultDialogueSize);
            GUI.Label(new Rect(left, top + 102f, width, 28f), "변경한 글자 크기는 게임을 다시 실행해도 유지됩니다.", speakerStyle);

            if (GUI.Button(new Rect(left, Screen.height - 78f, 140f, 36f), "타이틀로", menuButtonStyle)) screenMode = ScreenMode.Title;
        }

        private void DrawPanelBackground(string heading, string description)
        {
            DrawTitleBackground();
            var width = Mathf.Min(Screen.width - 64f, 980f);
            var left = (Screen.width - width) * 0.5f;
            DrawRect(new Rect(left - 28f, 52f, width + 56f, Screen.height - 104f), new Color(0.02f, 0.035f, 0.075f, 0.94f));
            GUI.Label(new Rect(left, 84f, width, 48f), heading, titleStyle);
            GUI.Label(new Rect(left, 134f, width, 28f), description, subtitleStyle);
        }

        private bool DrawMenuButton(string label, float x, float y, bool enabled = true)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            var clicked = GUI.Button(new Rect(x, y, 226f, 42f), label, menuButtonStyle);
            GUI.enabled = wasEnabled;
            return clicked;
        }

        private void StartNewGame()
        {
            sequence.SetIndex(0);
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            screenMode = ScreenMode.Game;
        }

        private void ContinueGame()
        {
            Load();
            screenMode = ScreenMode.Game;
        }

        private void Next()
        {
            sequence.MoveNext();
        }

        private void Previous()
        {
            sequence.MovePrevious();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SaveKey, sequence.CurrentIndex);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            if (HasSave)
            {
                sequence.SetIndex(PlayerPrefs.GetInt(SaveKey));
            }
        }

        private void SetDialogueSize(int size)
        {
            PlayerPrefs.SetInt(DialogueSizeKey, Mathf.Clamp(size, 18, 38));
            PlayerPrefs.Save();
        }

        private bool HasSave => PlayerPrefs.HasKey(SaveKey);

        private void DrawTitleBackground()
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.025f, 0.055f, 0.12f));
            DrawRect(new Rect(0f, Screen.height * 0.57f, Screen.width, Screen.height * 0.43f), new Color(0.015f, 0.025f, 0.06f));
            DrawRect(new Rect(Screen.width * 0.53f, Screen.height * 0.18f, Screen.width * 0.17f, Screen.height * 0.53f), new Color(0.045f, 0.085f, 0.16f));
            DrawRect(new Rect(Screen.width * 0.71f, Screen.height * 0.30f, Screen.width * 0.23f, Screen.height * 0.41f), new Color(0.035f, 0.07f, 0.135f));
            DrawRect(new Rect(Screen.width * 0.37f, Screen.height * 0.40f, Screen.width * 0.15f, Screen.height * 0.31f), new Color(0.03f, 0.065f, 0.13f));

            for (var row = 0; row < 7; row++)
            {
                for (var column = 0; column < 5; column++)
                {
                    var x = (Screen.width * 0.55f) + (column * Screen.width * 0.028f);
                    var y = (Screen.height * 0.23f) + (row * Screen.height * 0.061f);
                    DrawRect(new Rect(x, y, 10f, 18f), new Color(0.68f, 0.82f, 0.95f, 0.35f));
                }
            }

            DrawRect(new Rect(0f, Screen.height * 0.84f, Screen.width, 3f), new Color(0.38f, 0.7f, 0.92f, 0.5f));
        }

        private void DrawGameBackground()
        {
            DrawTitleBackground();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.01f, 0.02f, 0.05f, 0.56f));
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 38, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.88f, 0.94f, 1f) } };
            subtitleStyle = new GUIStyle(GUI.skin.label) { fontSize = 19, normal = { textColor = new Color(0.66f, 0.82f, 0.98f) } };
            speakerStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, normal = { textColor = new Color(0.68f, 0.82f, 0.95f) } };
            dialogueStyle = new GUIStyle(GUI.skin.label) { fontSize = DefaultDialogueSize, wordWrap = true, normal = { textColor = Color.white } };
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true, normal = { textColor = Color.white } };
            menuButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, alignment = TextAnchor.MiddleLeft, padding = new RectOffset(18, 18, 6, 6) };
        }
    }
}
