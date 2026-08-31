using System;
using CocClear.Core;
using UnityEngine;

namespace CocClear.Runtime
{
    /// <summary>Title and visual-novel shell. The supplied title art is loaded from Resources/Archive.</summary>
    public sealed class VisualNovelController : MonoBehaviour
    {
        private const string SaveKey = "CocClear.VisualNovel.PrologueIndex";
        private const string DialogueSizeKey = "CocClear.VisualNovel.DialogueSize";
        private const string TextSpeedKey = "CocClear.VisualNovel.TextSpeed";
        private const string AutoDelayKey = "CocClear.VisualNovel.AutoDelay";
        private const string FullScreenModeKey = "CocClear.VisualNovel.FullScreenMode";
        private const string GameTitle = "멸망한 세계의 회사원";
        private const float TitleFadeDuration = 1.35f;
        private const float LoadingDuration = 2.2f;
        private const float LoadingCompleteDisplayDuration = 0.85f;
        private const float BackgroundTransitionDuration = 0.5f;
        private const float SceneWipeDuration = 0.42f;
        private const float CorridorCrowdZoomDuration = 0.36f;
        private const int DefaultTextSpeed = 38;
        private const float DefaultAutoDelay = 1.1f;
        private const float SkipAdvanceDelay = 0.08f;
        private const int DefaultDialogueSize = 26;
        private const int WindowedWidth = 1600;
        private const int WindowedHeight = 900;
        private const float GameUiReferenceWidth = 1920f;
        private const float GameUiReferenceHeight = 1080f;
        private static readonly Color Ink = new Color(0.025f, 0.055f, 0.078f);
        private static readonly Color PanelInk = new Color(0.035f, 0.088f, 0.118f);
        private static readonly Color Accent = new Color(0.47f, 0.83f, 0.85f);
        private static readonly Color WarmAccent = new Color(0.92f, 0.65f, 0.36f);
        private static readonly Color FunctionKeyFill = new Color(0.285f, 0.27f, 0.275f, 0.98f);
        private static readonly Color FunctionKeyHover = new Color(0.365f, 0.345f, 0.35f, 1f);
        private static readonly Color FunctionKeyPressed = new Color(0.215f, 0.205f, 0.21f, 1f);
        private static readonly Color FunctionKeyGlyph = new Color(0.96f, 0.94f, 0.95f);
        private static readonly Color SoftPink = new Color(0.96f, 0.54f, 0.61f);
        private enum ScreenMode
        {
            Title,
            TitleExitConfirmation,
            Game,
            Backlog,
            Loading,
            GameMenu,
            GameExitMenu,
            Archive,
            Settings,
        }

        private enum ArchiveTab
        {
            Records,
            Illustrations,
        }

        private enum UtilityIcon
        {
            Log,
            Save,
            Skip,
        }

        private enum GameCommand
        {
            None,
            Skip,
            Auto,
            Save,
            Menu,
        }

        private NarrativeSequence sequence;
        private ScreenMode screenMode;
        private ScreenMode settingsReturnMode;
        private ArchiveTab archiveTab;
        private string archiveEpisodeTitle;
        private float titleFadeStartedAt;
        private float loadingStartedAt;
        private float loadingCompletedAt;
        private float backgroundTransitionStartedAt;
        private float sceneWipeStartedAt = -1f;
        private float corridorCrowdZoomStartedAt = -1f;
        private float typewriterElapsed;
        private float playbackElapsed;
        private float saveFeedbackUntil;
        private int displayedCharacterCount;
        private int typewriterLineIndex = -1;
        private bool loadingComplete;
        private bool autoMode;
        private bool skipMode;
        private bool interfaceHidden;
        private Vector2 archiveScrollPosition;
        private Vector2 backlogScrollPosition;
        private Texture2D[] galleryImages;
        private Texture2D selectedGalleryImage;
        private int selectedGalleryIndex = -1;
        private Texture2D titleBackground;
        private Texture2D titleWordmark;
        private Texture2D gameBackground;
        private Texture2D previousGameBackground;
        private Texture2D sceneTransitionBandTexture;
        private Texture2D prologueBedroomBackground;
        private Texture2D prologueOfficeBackground;
        private Texture2D prologueDeskBackground;
        private Texture2D prologueDeskSeatedBackground;
        private Texture2D cleanCorridorBackground;
        private string activeSceneId = string.Empty;
        private Texture2D kimYuinPortrait;
        private Font uiFont;
        private GUIStyle titleStyle;
        private GUIStyle titleAccentStyle;
        private GUIStyle titleShadowStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle speakerStyle;
        private GUIStyle affiliationStyle;
        private GUIStyle archiveSpeakerStyle;
        private GUIStyle archiveLogStyle;
        private GUIStyle galleryCaptionStyle;
        private GUIStyle dialogueStyle;
        private GUIStyle menuButtonStyle;
        private GUIStyle bodyStyle;
        private GUIStyle sliderTrackStyle;
        private GUIStyle sliderThumbStyle;
        private GUIStyle roundedPanelStyle;
        private GUIStyle compactIconButtonStyle;
        private GUIStyle compactIconGlyphStyle;
        private GUIStyle autoButtonStyle;
        private GUIStyle pauseButtonStyle;
        private GUIStyle pauseTitleStyle;
        private GUIStyle confirmationStyle;
        private GUIStyle screenHeadingStyle;
        private GUIStyle titleMenuButtonStyle;

        private void Awake()
        {
            sequence = new NarrativeSequence(PrologueScript.Create());
            titleBackground = Resources.Load<Texture2D>("Archive/TitleBackground");
            titleWordmark = Resources.Load<Texture2D>("Archive/TitleWordmark");
            prologueBedroomBackground = Resources.Load<Texture2D>("Backgrounds/PrologueBedroom");
            prologueOfficeBackground = Resources.Load<Texture2D>("Backgrounds/PrologueOffice");
            prologueDeskBackground = Resources.Load<Texture2D>("Backgrounds/PrologueDeskClose");
            prologueDeskSeatedBackground = Resources.Load<Texture2D>("Backgrounds/PrologueDeskSeated");
            cleanCorridorBackground = Resources.Load<Texture2D>("Backgrounds/CleanCorridor");
            galleryImages = Array.FindAll(new[]
            {
                prologueBedroomBackground,
                cleanCorridorBackground,
                prologueOfficeBackground,
                prologueDeskBackground,
                prologueDeskSeatedBackground,
            }, image => image != null);
            gameBackground = prologueBedroomBackground != null ? prologueBedroomBackground : prologueOfficeBackground;
            kimYuinPortrait = Resources.Load<Texture2D>("Characters/KimYuin");
            screenMode = ScreenMode.Title;
            archiveEpisodeTitle = ScenarioArchive.EpisodeTitles[0];
            titleFadeStartedAt = Time.unscaledTime;
            RestoreDisplayMode();
            ResetTypewriter();
        }

        private void Update()
        {
            if (screenMode == ScreenMode.Loading)
            {
                UpdateLoading();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleEscape();
                return;
            }

            if (selectedGalleryImage != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    SelectGalleryImage(selectedGalleryIndex - 1);
                }
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    SelectGalleryImage(selectedGalleryIndex + 1);
                }

                return;
            }

            if (screenMode != ScreenMode.Game)
            {
                return;
            }

            if (IsSceneWipeActive)
            {
                return;
            }

            if (interfaceHidden)
            {
                if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || IsAdvanceKeyPressed())
                {
                    interfaceHidden = false;
                }

                return;
            }

            UpdateTypewriter();

            if (Input.GetKeyDown(KeyCode.F11))
            {
                ToggleDisplayMode();
                return;
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                Save();
                return;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                OpenBacklog();
                return;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                ToggleAutoMode();
                return;
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                ToggleSkipMode();
                return;
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                autoMode = false;
                skipMode = false;
                interfaceHidden = true;
                return;
            }

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                var command = GetGameCommandAtPointer();
                if (command != GameCommand.None)
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        ActivateGameCommand(command);
                    }

                    return;
                }

                AdvanceFromPlayer();
                return;
            }

            if (IsAdvanceKeyPressed())
            {
                AdvanceFromPlayer();
                return;
            }

            // Process playback only after this frame's button and keyboard input.
            // A second SKIP press therefore disables fast playback before it can advance again.
            UpdatePlaybackModes();
        }

        private void HandleEscape()
        {
            if (selectedGalleryImage != null)
            {
                selectedGalleryImage = null;
                selectedGalleryIndex = -1;
                return;
            }

            switch (screenMode)
            {
                case ScreenMode.Title:
                    screenMode = ScreenMode.TitleExitConfirmation;
                    break;
                case ScreenMode.TitleExitConfirmation:
                    screenMode = ScreenMode.Title;
                    break;
                case ScreenMode.Game:
                    screenMode = ScreenMode.GameExitMenu;
                    break;
                case ScreenMode.Backlog:
                    screenMode = ScreenMode.Game;
                    break;
                case ScreenMode.GameExitMenu:
                case ScreenMode.GameMenu:
                    screenMode = ScreenMode.Game;
                    break;
                case ScreenMode.Settings:
                    screenMode = settingsReturnMode;
                    break;
                case ScreenMode.Archive:
                    screenMode = ScreenMode.Title;
                    break;
            }
        }

        private void OnGUI()
        {
            EnsureStyles();
            RefreshResponsiveStyles();

            if (selectedGalleryImage != null)
            {
                DrawGalleryPreview();
                return;
            }

            switch (screenMode)
            {
                case ScreenMode.Title:
                    DrawTitleScreen();
                    break;
                case ScreenMode.TitleExitConfirmation:
                    DrawTitleExitConfirmation();
                    break;
                case ScreenMode.Game:
                    DrawGameScreen();
                    break;
                case ScreenMode.Backlog:
                    DrawBacklogScreen();
                    break;
                case ScreenMode.Loading:
                    DrawLoadingScreen();
                    break;
                case ScreenMode.GameMenu:
                    DrawGameMenu();
                    break;
                case ScreenMode.GameExitMenu:
                    DrawGameExitMenu();
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
            if (titleWordmark != null)
            {
                GUI.DrawTexture(new Rect(left, 38f, 610f, 170f), titleWordmark, ScaleMode.ScaleToFit, true);
            }
            else
            {
                const string titlePrefix = "멸망한 세계의 ";
                const string titleAccent = "회사원";
                var prefixRect = new Rect(left, 56f, 600f, 70f);
                var prefixWidth = titleStyle.CalcSize(new GUIContent(titlePrefix)).x;
                var accentRect = new Rect(left + prefixWidth, 56f, 260f, 70f);
                GUI.Label(new Rect(prefixRect.x + 2f, prefixRect.y + 2f, prefixRect.width, prefixRect.height), titlePrefix, titleShadowStyle);
                GUI.Label(new Rect(accentRect.x + 2f, accentRect.y + 2f, accentRect.width, accentRect.height), titleAccent, titleShadowStyle);
                GUI.Label(prefixRect, titlePrefix, titleStyle);
                GUI.Label(accentRect, titleAccent, titleAccentStyle);
            }
            var menuTop = Mathf.Max(210f, Screen.height - 338f);
            var isFadingIn = TitleFadeAlpha > 0f;
            if (DrawTitleListButton("처음부터", left, menuTop, !isFadingIn)) StartNewGame();
            var nextMenuY = menuTop + 52f;
            var menuItemCount = 3;
            if (HasSave)
            {
                if (DrawTitleListButton("이어하기", left, nextMenuY, !isFadingIn)) ContinueGame();
                nextMenuY += 52f;
                menuItemCount++;
            }
            if (DrawTitleListButton("보관함", left, nextMenuY, !isFadingIn)) screenMode = ScreenMode.Archive;
            if (DrawTitleListButton("설정", left, nextMenuY + 52f, !isFadingIn)) OpenSettings(ScreenMode.Title);

            var lastPlayMessage = HasSave
                ? $"마지막 플레이 : 프롤로그 {PlayerPrefs.GetInt(SaveKey) + 1}번째 대사까지"
                : "마지막 플레이 : 아직 기록 없음";
            GUI.Label(new Rect(left, menuTop + (menuItemCount * 52f) + 8f, 440f, 26f), lastPlayMessage, speakerStyle);
            DrawTitleFadeOverlay();
        }

        private void DrawGameScreen()
        {
            DrawGameBackground();
            if (IsSceneWipeActive)
            {
                DrawSceneWipe();
                return;
            }

            DrawActivePortrait();

            if (interfaceHidden)
            {
                return;
            }

            var previousMatrix = GUI.matrix;
            GUI.matrix = GetGameUiMatrix();
            DrawGameDialogueInterface();
            GUI.matrix = previousMatrix;
        }

        private void DrawGameDialogueInterface()
        {
            var dialogueRect = GetGameDialogueRect();
            var dialogueHeight = dialogueRect.height;
            var chatContentTop = GameUiReferenceHeight - dialogueHeight - Mathf.Clamp(GameUiReferenceHeight * 0.14f, 112f, 154f);
            var speakerY = chatContentTop + dialogueHeight - 126f;
            var dialogueY = chatContentTop + dialogueHeight - 134f;
            var currentLine = sequence.Current;

            DrawRoundedRect(dialogueRect, new Color(0f, 0f, 0f, 0.76f));
            dialogueStyle.fontSize = PlayerPrefs.GetInt(DialogueSizeKey, DefaultDialogueSize);
            if (currentLine.IsNarration)
            {
                GUI.Label(new Rect(dialogueRect.x + 40f, dialogueY, dialogueRect.width - 80f, 72f), VisibleDialogueText, dialogueStyle);
            }
            else
            {
                var affiliation = GetSpeakerAffiliation(currentLine.Speaker);
                if (!string.IsNullOrEmpty(affiliation))
                {
                    var affiliationWidth = Mathf.Max(128f, affiliationStyle.CalcSize(new GUIContent(affiliation)).x + 34f);
                    var affiliationRect = new Rect(dialogueRect.x + 32f, speakerY - 28f, affiliationWidth, 22f);
                    DrawAffiliationTag(affiliationRect, affiliation);
                }

                DrawRect(new Rect(dialogueRect.x + 16f, speakerY, 5f, 34f), new Color(0.28f, 0.64f, 1f));
                GUI.Label(new Rect(dialogueRect.x + 32f, speakerY, 220f, 34f), currentLine.Speaker, speakerStyle);
                GUI.Label(new Rect(dialogueRect.x + 248f, dialogueY, dialogueRect.width - 490f, 72f), VisibleDialogueText, dialogueStyle);
            }

            const float commandWidth = 54f;
            const float commandHeight = 44f;
            const float commandGap = 10f;
            GetGameCommandLayout(dialogueRect, out var commandX, out var commandY);
            var skipRect = new Rect(commandX + 5f, commandY, commandHeight, commandHeight);
            var autoRect = new Rect(commandX + (commandWidth + commandGap), commandY, commandWidth, commandHeight);
            var saveRect = new Rect(commandX + (commandWidth + commandGap) * 2f + 5f, commandY, commandHeight, commandHeight);
            var menuRect = new Rect(commandX + (commandWidth + commandGap) * 3f + 5f, commandY, commandHeight, commandHeight);
            if (Time.unscaledTime < saveFeedbackUntil)
            {
                GUI.Label(new Rect(saveRect.center.x - 70f, saveRect.y - 30f, 140f, 24f), "저장되었습니다.", confirmationStyle);
            }
            if (skipMode) DrawRoundedRect(new Rect(skipRect.x - 2f, skipRect.y - 2f, skipRect.width + 4f, skipRect.height + 4f), new Color(SoftPink.r, SoftPink.g, SoftPink.b, 0.72f));
            if (autoMode) DrawRoundedRect(new Rect(autoRect.x - 2f, autoRect.y - 2f, autoRect.width + 4f, autoRect.height + 4f), new Color(SoftPink.r, SoftPink.g, SoftPink.b, 0.72f));
            if (DrawCompactIconButton(skipRect, UtilityIcon.Skip)) ToggleSkipMode();
            if (GUI.Button(autoRect, "AUTO", autoButtonStyle)) ToggleAutoMode();
            if (DrawCompactIconButton(saveRect, UtilityIcon.Save)) Save();
            if (DrawCompactIconButton(menuRect, UtilityIcon.Log)) OpenGameMenu();
        }

        private void DrawGameMenu()
        {
            DrawGameBackground();
            DrawActivePortrait();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.78f));

            const float panelWidth = 328f;
            const float buttonHeight = 42f;
            const float buttonGap = 8f;
            var panelLeft = (Screen.width - panelWidth) * 0.5f;
            var totalHeight = (buttonHeight * 6f) + (buttonGap * 5f);
            var top = Mathf.Max(142f, (Screen.height - totalHeight) * 0.5f + 46f);
            GUI.Label(new Rect(panelLeft, top - 58f, panelWidth, 34f), "PAUSE", pauseTitleStyle);
            DrawRect(new Rect(panelLeft, top - 18f, panelWidth, 2f), new Color(0.82f, 0.84f, 0.86f, 0.65f));

            if (DrawPauseMenuButton("BACK", panelLeft, top)) screenMode = ScreenMode.Game;
            if (DrawPauseMenuButton("HOME", panelLeft, top + (buttonHeight + buttonGap))) screenMode = ScreenMode.Title;
            var saveButtonY = top + (buttonHeight + buttonGap) * 2f;
            if (DrawPauseMenuButton("SAVE", panelLeft, saveButtonY)) Save();
            if (DrawPauseMenuButton("LOG", panelLeft, top + (buttonHeight + buttonGap) * 3f)) OpenBacklog();
            if (DrawPauseMenuButton("SETTINGS", panelLeft, top + (buttonHeight + buttonGap) * 4f)) OpenSettings(ScreenMode.GameMenu);
            if (DrawPauseMenuButton("EXIT", panelLeft, top + (buttonHeight + buttonGap) * 5f)) screenMode = ScreenMode.GameExitMenu;
            if (Time.unscaledTime < saveFeedbackUntil)
            {
                GUI.Label(new Rect(panelLeft, saveButtonY - 30f, panelWidth, 24f), "저장되었습니다.", confirmationStyle);
            }
        }

        private void DrawBacklogScreen()
        {
            DrawGameBackground();
            DrawActivePortrait();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.78f));

            var width = Mathf.Min(Screen.width - 96f, 940f);
            var left = (Screen.width - width) * 0.5f;
            var panel = new Rect(left, 54f, width, Screen.height - 108f);
            DrawRoundedRect(panel, new Color(0.045f, 0.048f, 0.055f, 0.96f));
            GUI.Label(new Rect(panel.x + 28f, panel.y + 20f, panel.width - 290f, 34f), "대사 로그", screenHeadingStyle);
            if (GUI.Button(new Rect(panel.x + panel.width - 210f, panel.y + 18f, 78f, 32f), "최신", menuButtonStyle))
            {
                backlogScrollPosition = new Vector2(0f, float.MaxValue);
            }
            if (GUI.Button(new Rect(panel.x + panel.width - 120f, panel.y + 18f, 92f, 32f), "닫기", menuButtonStyle)) screenMode = ScreenMode.Game;

            var lines = sequence.GetLinesThroughCurrent();
            const float entryHeight = 60f;
            const float entryGap = 4f;
            var contentHeight = Mathf.Max(panel.height - 96f, lines.Length * (entryHeight + entryGap));
            var viewRect = new Rect(panel.x + 20f, panel.y + 72f, panel.width - 40f, panel.height - 94f);
            backlogScrollPosition = GUI.BeginScrollView(viewRect, backlogScrollPosition, new Rect(0f, 0f, viewRect.width - 20f, contentHeight));
            for (var i = 0; i < lines.Length; i++)
            {
                var y = i * (entryHeight + entryGap);
                if (lines[i].IsNarration)
                {
                    GUI.Label(new Rect(22f, y + 6f, viewRect.width - 44f, 48f), lines[i].Text, archiveLogStyle);
                    continue;
                }

                var affiliation = GetSpeakerAffiliation(lines[i].Speaker);
                var affiliationWidth = Mathf.Max(118f, affiliationStyle.CalcSize(new GUIContent(affiliation)).x + 34f);
                var affiliationRect = new Rect(22f, y + 14f, affiliationWidth, 22f);
                DrawAffiliationTag(affiliationRect, affiliation);

                var nameX = affiliationRect.xMax + 14f;
                var nameWidth = archiveSpeakerStyle.CalcSize(new GUIContent(lines[i].Speaker)).x;
                GUI.Label(new Rect(nameX, y + 9f, nameWidth + 8f, 32f), lines[i].Speaker, archiveSpeakerStyle);

                var markerX = nameX + nameWidth + 16f;
                DrawRect(new Rect(markerX, y + 14f, 4f, 22f), new Color(0.28f, 0.64f, 1f));
                GUI.Label(new Rect(markerX + 16f, y + 6f, viewRect.width - markerX - 40f, 48f), lines[i].Text, archiveLogStyle);
            }

            GUI.EndScrollView();
        }

        private void DrawLoadingScreen()
        {
            DrawGrayscaleLoadingBackground();

            var panelWidth = Mathf.Min(340f, Screen.width - 64f);
            var panelLeft = Screen.width - panelWidth - 32f;
            var panelTop = Screen.height - 158f;
            var progress = LoadingProgress;
            var percent = Mathf.RoundToInt(progress * 100f);
            var status = loadingComplete ? "로딩 완료!" : "로딩 중...";

            GUI.Label(new Rect(panelLeft, panelTop, panelWidth, 32f), status, bodyStyle);
            var gauge = new Rect(panelLeft, panelTop + 44f, panelWidth, 16f);
            DrawFramedPanel(gauge, new Color(Ink.r, Ink.g, Ink.b, 0.94f), new Color(Accent.r, Accent.g, Accent.b, 0.62f));
            DrawRect(new Rect(gauge.x + 3f, gauge.y + 3f, (gauge.width - 6f) * progress, gauge.height - 6f), Accent);
            GUI.Label(new Rect(panelLeft, panelTop + 68f, panelWidth, 24f), $"{percent}%", speakerStyle);
        }

        private void DrawTitleExitConfirmation()
        {
            DrawTitleBackground();
            DrawExitConfirmation(ScreenMode.Title);
        }

        private void DrawGameExitMenu()
        {
            DrawGameBackground();
            DrawExitConfirmation(ScreenMode.Game);
        }

        private void DrawExitConfirmation(ScreenMode cancelTarget)
        {
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.82f));

            const float panelWidth = 328f;
            const float buttonHeight = 42f;
            const float buttonGap = 8f;
            var panelLeft = (Screen.width - panelWidth) * 0.5f;
            var top = Mathf.Max(186f, (Screen.height - ((buttonHeight * 2f) + buttonGap)) * 0.5f + 48f);
            GUI.Label(new Rect(panelLeft, top - 112f, panelWidth, 34f), "GAME EXIT", pauseTitleStyle);
            GUI.Label(new Rect(panelLeft, top - 70f, panelWidth, 28f), "게임을 종료하시겠습니까?", confirmationStyle);
            DrawRect(new Rect(panelLeft, top - 18f, panelWidth, 2f), new Color(0.82f, 0.84f, 0.86f, 0.65f));

            if (GUI.Button(new Rect(panelLeft, top, panelWidth, buttonHeight), "예", pauseButtonStyle)) QuitGame();
            if (GUI.Button(new Rect(panelLeft, top + buttonHeight + buttonGap, panelWidth, buttonHeight), "CANCEL", pauseButtonStyle)) screenMode = cancelTarget;
        }

        private void DrawArchiveScreen()
        {
            DrawPanelBackground("보관함", "모든 시나리오 기록과 전달받은 일러스트를 전시합니다.");

            var width = Mathf.Min(Screen.width - 96f, 900f);
            var left = (Screen.width - width) * 0.5f;
            var recordsTabRect = new Rect(left, 177f, 130f, 34f);
            var illustrationsTabRect = new Rect(left + 140f, 177f, 130f, 34f);
            if (GUI.Button(recordsTabRect, "시나리오 기록", menuButtonStyle)) archiveTab = ArchiveTab.Records;
            if (GUI.Button(illustrationsTabRect, "일러스트", menuButtonStyle)) archiveTab = ArchiveTab.Illustrations;
            var activeTabRect = archiveTab == ArchiveTab.Records ? recordsTabRect : illustrationsTabRect;
            DrawRect(new Rect(activeTabRect.x + 4f, activeTabRect.yMax + 5f, activeTabRect.width - 8f, 2f), Accent);

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
            const float episodeButtonGap = 8f;
            var episodeButtonWidth = (width - (episodeButtonGap * (ScenarioArchive.EpisodeTitles.Count - 1))) / ScenarioArchive.EpisodeTitles.Count;
            for (var i = 0; i < ScenarioArchive.EpisodeTitles.Count; i++)
            {
                var episodeTitle = ScenarioArchive.EpisodeTitles[i];
                var episodeRect = new Rect(left + (episodeButtonWidth + episodeButtonGap) * i, 220f, episodeButtonWidth, 34f);
                if (GUI.Button(episodeRect, episodeTitle, menuButtonStyle))
                {
                    archiveEpisodeTitle = episodeTitle;
                    archiveScrollPosition = Vector2.zero;
                }

                if (archiveEpisodeTitle == episodeTitle)
                {
                    DrawRect(new Rect(episodeRect.x + 4f, episodeRect.yMax + 4f, episodeRect.width - 8f, 2f), Accent);
                }
            }

            var records = ScenarioArchive.CreateForEpisode(archiveEpisodeTitle);
            var viewRect = new Rect(left, 276f, width, Screen.height - 374f);
            if (records.Length == 0)
            {
                GUI.Label(new Rect(left, 288f, width, 58f), $"{archiveEpisodeTitle}은(는) 아직 기록된 시나리오가 없습니다.", bodyStyle);
                return;
            }

            const float recordHeight = 60f;
            const float recordGap = 4f;
            var contentHeight = records.Length * (recordHeight + recordGap);
            archiveScrollPosition = GUI.BeginScrollView(viewRect, archiveScrollPosition, new Rect(0f, 0f, width - 24f, contentHeight));
            var y = 0f;
            for (var i = 0; i < records.Length; i++)
            {
                var record = records[i];
                if (record.Line.IsNarration)
                {
                    GUI.Label(new Rect(24f, y + 6f, width - 56f, 48f), record.Line.Text, archiveLogStyle);
                    y += recordHeight + recordGap;
                    continue;
                }

                var affiliation = GetSpeakerAffiliation(record.Line.Speaker);
                var affiliationWidth = Mathf.Max(118f, affiliationStyle.CalcSize(new GUIContent(affiliation)).x + 34f);
                var affiliationRect = new Rect(24f, y + 14f, affiliationWidth, 22f);
                DrawAffiliationTag(affiliationRect, affiliation);

                var nameX = affiliationRect.xMax + 14f;
                var nameWidth = archiveSpeakerStyle.CalcSize(new GUIContent(record.Line.Speaker)).x;
                GUI.Label(new Rect(nameX, y + 9f, nameWidth + 8f, 32f), record.Line.Speaker, archiveSpeakerStyle);

                var markerX = nameX + nameWidth + 16f;
                DrawRect(new Rect(markerX, y + 14f, 4f, 22f), new Color(0.28f, 0.64f, 1f));
                GUI.Label(new Rect(markerX + 16f, y + 6f, width - markerX - 56f, 48f), record.Line.Text, archiveLogStyle);
                y += recordHeight + recordGap;
            }

            GUI.EndScrollView();
        }

        private void DrawArchiveGallery(float left, float width)
        {
            GUI.Label(new Rect(left, 220f, width, 30f), $"등록된 CG: {galleryImages.Length}장", bodyStyle);
            if (galleryImages.Length == 0)
            {
                GUI.Label(new Rect(left, 260f, width, 58f), "아직 전시할 일러스트가 없습니다. 다음에 전달해 주는 일러스트부터 전부 보관함에 등록합니다.", subtitleStyle);
                return;
            }

            const float cellWidth = 280f;
            const float cellHeight = 184f;
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
                DrawRoundedRect(new Rect(x, y, cellWidth - 12f, cellHeight - 12f), new Color(0.075f, 0.08f, 0.09f, 0.98f));
                var imageRect = new Rect(x + 8f, y + 8f, cellWidth - 28f, 120f);
                var thumbnailClicked = GUI.Button(imageRect, GUIContent.none, GUIStyle.none);
                var currentEvent = Event.current;
                if (!thumbnailClicked && currentEvent.type == EventType.MouseDown && currentEvent.button == 1 && imageRect.Contains(currentEvent.mousePosition))
                {
                    thumbnailClicked = true;
                    currentEvent.Use();
                }

                if (thumbnailClicked)
                {
                    SelectGalleryImage(i);
                }

                GUI.DrawTexture(imageRect, galleryImages[i], ScaleMode.ScaleToFit, true);
                GUI.Label(new Rect(x + 8f, y + 136f, cellWidth - 28f, 34f), GetGalleryCaption(galleryImages[i]), galleryCaptionStyle);
            }

            GUI.EndScrollView();
        }

        private void DrawGalleryPreview()
        {
            var previewBounds = new Rect(48f, 48f, Screen.width - 96f, Screen.height - 132f);
            var textureAspect = selectedGalleryImage.width / (float)selectedGalleryImage.height;
            var boundsAspect = previewBounds.width / previewBounds.height;
            Rect imageRect;
            if (textureAspect > boundsAspect)
            {
                var height = previewBounds.width / textureAspect;
                imageRect = new Rect(previewBounds.x, previewBounds.y + ((previewBounds.height - height) * 0.5f), previewBounds.width, height);
            }
            else
            {
                var width = previewBounds.height * textureAspect;
                imageRect = new Rect(previewBounds.x + ((previewBounds.width - width) * 0.5f), previewBounds.y, width, previewBounds.height);
            }

            var currentEvent = Event.current;
            var previousRect = new Rect(28f, (Screen.height - 52f) * 0.5f, 54f, 52f);
            var nextRect = new Rect(Screen.width - 82f, (Screen.height - 52f) * 0.5f, 54f, 52f);
            if (currentEvent.type == EventType.MouseDown && (currentEvent.button == 0 || currentEvent.button == 1))
            {
                if (galleryImages.Length > 1 && previousRect.Contains(currentEvent.mousePosition))
                {
                    SelectGalleryImage(selectedGalleryIndex - 1);
                    currentEvent.Use();
                    return;
                }

                if (galleryImages.Length > 1 && nextRect.Contains(currentEvent.mousePosition))
                {
                    SelectGalleryImage(selectedGalleryIndex + 1);
                    currentEvent.Use();
                    return;
                }

                if (!imageRect.Contains(currentEvent.mousePosition))
                {
                    selectedGalleryImage = null;
                    selectedGalleryIndex = -1;
                    currentEvent.Use();
                    return;
                }
            }

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.94f));
            GUI.DrawTexture(imageRect, selectedGalleryImage, ScaleMode.ScaleToFit, true);
            if (galleryImages.Length > 1)
            {
                DrawRoundedRect(previousRect, new Color(0.12f, 0.13f, 0.15f, 0.90f));
                DrawRoundedRect(nextRect, new Color(0.12f, 0.13f, 0.15f, 0.90f));
                GUI.Label(previousRect, "‹", screenHeadingStyle);
                GUI.Label(nextRect, "›", screenHeadingStyle);
            }

            GUI.Label(new Rect(48f, 18f, Screen.width - 96f, 26f), $"{selectedGalleryIndex + 1} / {galleryImages.Length}  ·  {GetGalleryCaption(selectedGalleryImage)}", speakerStyle);
        }

        private void SelectGalleryImage(int index)
        {
            if (galleryImages == null || galleryImages.Length == 0)
            {
                selectedGalleryImage = null;
                selectedGalleryIndex = -1;
                return;
            }

            selectedGalleryIndex = (index % galleryImages.Length + galleryImages.Length) % galleryImages.Length;
            selectedGalleryImage = galleryImages[selectedGalleryIndex];
        }

        private static string GetGalleryCaption(Texture2D image)
        {
            if (image == null)
            {
                return string.Empty;
            }

            switch (image.name)
            {
                case "PrologueBedroom": return "침실 1";
                case "CleanCorridor": return "회사 복도 1";
                case "PrologueOffice": return "사무실 1";
                case "PrologueDeskClose": return "사무실 자리 1";
                case "PrologueDeskSeated": return "사무실 자리 2";
                default: return "일러스트";
            }
        }

        private void DrawSettingsScreen()
        {
            DrawPanelBackground("게임 설정", "변경 사항은 즉시 게임 화면에 반영되고 다음 실행에도 유지됩니다.");

            var width = Mathf.Min(Screen.width - 96f, 900f);
            var left = (Screen.width - width) * 0.5f;
            var top = 186f;
            var fontSize = PlayerPrefs.GetInt(DialogueSizeKey, DefaultDialogueSize);
            GUI.Label(new Rect(left, top, width, 32f), "대사 글자 크기", bodyStyle);
            GUI.Label(new Rect(left, top + 45f, 120f, 32f), $"{fontSize}px", speakerStyle);
            if (GUI.Button(new Rect(left + 140f, top + 40f, 42f, 34f), "−", menuButtonStyle)) SetDialogueSize(fontSize - 2);
            if (GUI.Button(new Rect(left + 190f, top + 40f, 42f, 34f), "+", menuButtonStyle)) SetDialogueSize(fontSize + 2);
            if (GUI.Button(new Rect(left + 246f, top + 40f, 106f, 34f), "기본값", menuButtonStyle)) SetDialogueSize(DefaultDialogueSize);

            var textSpeed = PlayerPrefs.GetInt(TextSpeedKey, DefaultTextSpeed);
            GUI.Label(new Rect(left, top + 102f, width, 32f), "글자 출력 속도", bodyStyle);
            GUI.Label(new Rect(left, top + 147f, 120f, 32f), $"{textSpeed}자/초", speakerStyle);
            if (GUI.Button(new Rect(left + 140f, top + 142f, 42f, 34f), "−", menuButtonStyle)) SetTextSpeed(textSpeed - 4);
            if (GUI.Button(new Rect(left + 190f, top + 142f, 42f, 34f), "+", menuButtonStyle)) SetTextSpeed(textSpeed + 4);
            if (GUI.Button(new Rect(left + 246f, top + 142f, 106f, 34f), "기본값", menuButtonStyle)) SetTextSpeed(DefaultTextSpeed);

            var autoDelay = PlayerPrefs.GetFloat(AutoDelayKey, DefaultAutoDelay);
            GUI.Label(new Rect(left, top + 204f, width, 32f), "자동 진행 대기 시간", bodyStyle);
            var updatedAutoDelay = GUI.HorizontalSlider(new Rect(left, top + 248f, 260f, 22f), autoDelay, 0.35f, 3f, sliderTrackStyle, sliderThumbStyle);
            if (!Mathf.Approximately(autoDelay, updatedAutoDelay)) SetAutoDelay(updatedAutoDelay);
            GUI.Label(new Rect(left + 276f, top + 242f, 120f, 28f), $"{updatedAutoDelay:0.0}초", speakerStyle);
            GUI.Label(new Rect(left, top + 298f, width, 32f), "화면 모드", bodyStyle);
            var displayModeLabel = IsWindowed ? "전체 화면으로" : "창모드로";
            if (GUI.Button(new Rect(left, top + 338f, 180f, 36f), displayModeLabel, menuButtonStyle)) ToggleDisplayMode();
            GUI.Label(new Rect(left + 198f, top + 342f, width - 198f, 28f), IsWindowed ? "창 테두리를 드래그해 원하는 비율로 조절할 수 있습니다." : "F11 또는 버튼으로 창모드로 전환할 수 있습니다.", speakerStyle);
            GUI.Label(new Rect(left, top + 394f, width, 28f), "F5 저장 · L 로그 · A 자동 · S 스킵 · H UI 숨김 · F11 화면 모드", speakerStyle);

            if (GUI.Button(new Rect(left, Screen.height - 78f, 140f, 36f), "돌아가기", menuButtonStyle)) screenMode = settingsReturnMode;
        }

        private void DrawPanelBackground(string heading, string description)
        {
            DrawGameBackground();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.76f));
            var width = Mathf.Min(Screen.width - 64f, 980f);
            var left = (Screen.width - width) * 0.5f;
            GUI.Label(new Rect(left, 78f, width, 38f), heading, screenHeadingStyle);
            DrawRect(new Rect(left, 126f, width, 2f), new Color(0.82f, 0.84f, 0.86f, 0.58f));
            GUI.Label(new Rect(left, 140f, width, 28f), description, speakerStyle);
        }

        private bool DrawTitleListButton(string label, float x, float y, bool enabled)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            var clicked = GUI.Button(new Rect(x, y, 286f, 44f), label, titleMenuButtonStyle);
            GUI.enabled = wasEnabled;
            return clicked;
        }

        private bool DrawPauseMenuButton(string label, float x, float y)
        {
            return GUI.Button(new Rect(x, y, 328f, 42f), label, pauseButtonStyle);
        }

        private bool DrawCompactIconButton(Rect rect, UtilityIcon icon, bool enabled = true)
        {
            var wasEnabled = GUI.enabled;
            GUI.enabled = enabled;
            var clicked = GUI.Button(rect, GUIContent.none, compactIconButtonStyle);
            DrawCompactIcon(rect, icon, enabled);
            GUI.enabled = wasEnabled;
            return clicked;
        }

        private void DrawCompactIcon(Rect rect, UtilityIcon icon, bool enabled)
        {
            var ink = enabled ? FunctionKeyGlyph : new Color(0.48f, 0.46f, 0.47f);
            var shade = enabled ? new Color(0.30f, 0.285f, 0.29f) : new Color(0.22f, 0.21f, 0.215f);
            switch (icon)
            {
                case UtilityIcon.Log:
                    DrawRect(new Rect(rect.x + 11f, rect.y + 12f, rect.width - 22f, 3f), ink);
                    DrawRect(new Rect(rect.x + 11f, rect.y + 20f, rect.width - 22f, 3f), ink);
                    DrawRect(new Rect(rect.x + 11f, rect.y + 28f, rect.width - 22f, 3f), ink);
                    break;
                case UtilityIcon.Save:
                    DrawRect(new Rect(rect.x + 11f, rect.y + 9f, rect.width - 22f, rect.height - 18f), ink);
                    DrawRect(new Rect(rect.x + 16f, rect.y + 12f, rect.width - 32f, 8f), shade);
                    DrawRect(new Rect(rect.x + 16f, rect.y + 26f, rect.width - 32f, 7f), shade);
                    break;
                case UtilityIcon.Skip:
                    GUI.Label(rect, "▶▶", compactIconGlyphStyle);
                    break;
            }
        }

        private void StartNewGame()
        {
            sequence.SetIndex(0);
            ResetTypewriter();
            ResetPlaybackModes();
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.Save();
            screenMode = ScreenMode.Game;
        }

        private void ContinueGame()
        {
            sequence.SetIndex(PlayerPrefs.GetInt(SaveKey, 0));
            ResetTypewriter();
            ResetPlaybackModes();
            screenMode = ScreenMode.Game;
        }

        private void AdvanceFromPlayer()
        {
            ResetPlaybackModes();
            Next();
        }

        private void Next()
        {
            if (!IsCurrentLineFullyVisible)
            {
                RevealCurrentLine();
                return;
            }

            if (!sequence.MoveNext())
            {
                BeginLoading();
                return;
            }

            ResetTypewriter();
        }

        private void BeginLoading()
        {
            Save();
            loadingStartedAt = Time.unscaledTime;
            loadingCompletedAt = 0f;
            loadingComplete = false;
            screenMode = ScreenMode.Loading;
        }

        private void UpdateLoading()
        {
            if (!loadingComplete && Time.unscaledTime - loadingStartedAt >= LoadingDuration)
            {
                loadingComplete = true;
                loadingCompletedAt = Time.unscaledTime;
                return;
            }

            if (loadingComplete && Time.unscaledTime - loadingCompletedAt >= LoadingCompleteDisplayDuration)
            {
                screenMode = ScreenMode.Title;
            }
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SaveKey, sequence.CurrentIndex);
            PlayerPrefs.Save();
            saveFeedbackUntil = Time.unscaledTime + 1.4f;
        }

        private void SaveAndReturnToTitle()
        {
            Save();
            screenMode = ScreenMode.Title;
        }

        private static void QuitGame()
        {
            Application.Quit();
        }

        private void SetDialogueSize(int size)
        {
            PlayerPrefs.SetInt(DialogueSizeKey, Mathf.Clamp(size, 18, 38));
            PlayerPrefs.Save();
        }

        private void SetTextSpeed(int charactersPerSecond)
        {
            PlayerPrefs.SetInt(TextSpeedKey, Mathf.Clamp(charactersPerSecond, 20, 90));
            PlayerPrefs.Save();
        }

        private void SetAutoDelay(float delay)
        {
            PlayerPrefs.SetFloat(AutoDelayKey, Mathf.Clamp(delay, 0.35f, 3f));
            PlayerPrefs.Save();
        }

        private void ToggleAutoMode()
        {
            autoMode = !autoMode;
            skipMode = false;
            playbackElapsed = 0f;
        }

        private void ToggleSkipMode()
        {
            skipMode = !skipMode;
            autoMode = false;
            playbackElapsed = 0f;
        }

        private void ResetPlaybackModes()
        {
            autoMode = false;
            skipMode = false;
            playbackElapsed = 0f;
        }

        private void OpenBacklog()
        {
            ResetPlaybackModes();
            backlogScrollPosition = new Vector2(0f, float.MaxValue);
            screenMode = ScreenMode.Backlog;
        }

        private void OpenGameMenu()
        {
            ResetPlaybackModes();
            screenMode = ScreenMode.GameMenu;
        }

        private void OpenSettings(ScreenMode returnMode)
        {
            settingsReturnMode = returnMode;
            screenMode = ScreenMode.Settings;
        }

        private bool HasSave => PlayerPrefs.HasKey(SaveKey);

        private bool IsAdvanceKeyPressed()
        {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.RightArrow);
        }

        private bool IsWindowed => Screen.fullScreenMode == FullScreenMode.Windowed;

        private void RestoreDisplayMode()
        {
            if (!PlayerPrefs.HasKey(FullScreenModeKey))
            {
                return;
            }

            SetDisplayMode(PlayerPrefs.GetInt(FullScreenModeKey, 0) != 0);
        }

        private void ToggleDisplayMode()
        {
            SetDisplayMode(!IsWindowed);
        }

        private void SetDisplayMode(bool windowed)
        {
            if (windowed)
            {
                Screen.SetResolution(WindowedWidth, WindowedHeight, FullScreenMode.Windowed);
            }
            else
            {
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            }

            PlayerPrefs.SetInt(FullScreenModeKey, windowed ? 1 : 0);
            PlayerPrefs.Save();
        }

        private static Rect GetGameDialogueRect()
        {
            var horizontalInset = Mathf.Clamp(GameUiReferenceWidth * 0.055f, 48f, 112f);
            var dialogueHeight = Mathf.Min(310f, GameUiReferenceHeight * 0.36f);
            return new Rect(horizontalInset, GameUiReferenceHeight - dialogueHeight - 26f, GameUiReferenceWidth - (horizontalInset * 2f), dialogueHeight);
        }

        private static void GetGameCommandLayout(Rect dialogueRect, out float commandX, out float commandY)
        {
            const float commandWidth = 54f;
            const float commandGap = 10f;
            var commandRight = dialogueRect.xMax - 24f;
            commandX = commandRight - ((commandWidth + commandGap) * 4f) + commandGap;
            commandY = dialogueRect.yMax - 56f;
        }

        private static float GameUiScale => Mathf.Max(0.01f, Mathf.Min(Screen.width / GameUiReferenceWidth, Screen.height / GameUiReferenceHeight));

        private static Vector2 GameUiOffset
        {
            get
            {
                var scale = GameUiScale;
                return new Vector2((Screen.width - (GameUiReferenceWidth * scale)) * 0.5f, (Screen.height - (GameUiReferenceHeight * scale)) * 0.5f);
            }
        }

        private static Matrix4x4 GetGameUiMatrix()
        {
            var scale = GameUiScale;
            var offset = GameUiOffset;
            return Matrix4x4.TRS(new Vector3(offset.x, offset.y, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        }

        private static Vector2 GetGameUiPointer()
        {
            var scale = GameUiScale;
            var offset = GameUiOffset;
            return new Vector2((Input.mousePosition.x - offset.x) / scale, (Screen.height - Input.mousePosition.y - offset.y) / scale);
        }

        private GameCommand GetGameCommandAtPointer()
        {
            var dialogueRect = GetGameDialogueRect();
            const float commandWidth = 54f;
            const float commandHeight = 44f;
            const float commandGap = 10f;
            GetGameCommandLayout(dialogueRect, out var commandX, out var commandY);
            var pointer = GetGameUiPointer();

            if (new Rect(commandX + 5f, commandY, commandHeight, commandHeight).Contains(pointer)) return GameCommand.Skip;
            if (new Rect(commandX + (commandWidth + commandGap), commandY, commandWidth, commandHeight).Contains(pointer)) return GameCommand.Auto;
            if (new Rect(commandX + (commandWidth + commandGap) * 2f + 5f, commandY, commandHeight, commandHeight).Contains(pointer)) return GameCommand.Save;
            if (new Rect(commandX + (commandWidth + commandGap) * 3f + 5f, commandY, commandHeight, commandHeight).Contains(pointer)) return GameCommand.Menu;
            return GameCommand.None;
        }

        private void ActivateGameCommand(GameCommand command)
        {
            switch (command)
            {
                case GameCommand.Skip:
                    ToggleSkipMode();
                    break;
                case GameCommand.Auto:
                    ToggleAutoMode();
                    break;
                case GameCommand.Save:
                    Save();
                    break;
                case GameCommand.Menu:
                    OpenGameMenu();
                    break;
            }
        }

        private bool IsCurrentLineFullyVisible => displayedCharacterCount >= sequence.Current.Text.Length;

        private string VisibleDialogueText
        {
            get
            {
                SynchronizeTypewriterLine();
                return sequence.Current.Text.Substring(0, displayedCharacterCount);
            }
        }

        private void UpdateTypewriter()
        {
            SynchronizeTypewriterLine();
            if (IsCurrentLineFullyVisible)
            {
                return;
            }

            typewriterElapsed += Time.unscaledDeltaTime;
            displayedCharacterCount = Mathf.Min(
                sequence.Current.Text.Length,
                Mathf.FloorToInt(typewriterElapsed * PlayerPrefs.GetInt(TextSpeedKey, DefaultTextSpeed)));
        }

        private void UpdatePlaybackModes()
        {
            if (!autoMode && !skipMode)
            {
                return;
            }

            if (skipMode && !IsCurrentLineFullyVisible)
            {
                RevealCurrentLine();
                return;
            }

            if (!IsCurrentLineFullyVisible)
            {
                return;
            }

            playbackElapsed += Time.unscaledDeltaTime;
            var delay = skipMode ? SkipAdvanceDelay : PlayerPrefs.GetFloat(AutoDelayKey, DefaultAutoDelay);
            if (playbackElapsed < delay)
            {
                return;
            }

            playbackElapsed = 0f;
            Next();
        }

        private void ResetTypewriter()
        {
            typewriterLineIndex = sequence.CurrentIndex;
            typewriterElapsed = 0f;
            displayedCharacterCount = 0;
            playbackElapsed = 0f;
            UpdateGameBackgroundForCurrentLine();
            if (sequence.Current.SceneTransition == SceneTransitionStyle.FastBottomToTop)
            {
                sceneWipeStartedAt = Time.unscaledTime;
            }
        }

        private void SynchronizeTypewriterLine()
        {
            if (typewriterLineIndex != sequence.CurrentIndex)
            {
                ResetTypewriter();
            }
        }

        private void RevealCurrentLine()
        {
            SynchronizeTypewriterLine();
            displayedCharacterCount = sequence.Current.Text.Length;
            playbackElapsed = 0f;
        }

        private float TitleFadeAlpha => 1f - Mathf.Clamp01((Time.unscaledTime - titleFadeStartedAt) / TitleFadeDuration);

        private float LoadingProgress => loadingComplete
            ? 1f
            : Mathf.Clamp01((Time.unscaledTime - loadingStartedAt) / LoadingDuration);

        private void DrawTitleFadeOverlay()
        {
            var alpha = TitleFadeAlpha;
            if (alpha > 0f)
            {
                DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, alpha));
            }
        }

        private void DrawTitleBackground()
        {
            if (titleBackground != null)
            {
                GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), titleBackground, ScaleMode.ScaleAndCrop, true);
                DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0.015f, 0.025f, 0.05f, 0.08f));
                DrawRect(new Rect(0f, Screen.height * 0.70f, Screen.width, Screen.height * 0.30f), new Color(0.005f, 0.01f, 0.025f, 0.18f));
                return;
            }

            DrawGameBackground();
        }

        private void DrawGrayscaleLoadingBackground()
        {
            DrawGameBackground();
            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(0f, 0f, 0f, 0.52f));
            DrawRect(new Rect(0f, Screen.height * 0.64f, Screen.width, 2f), new Color(Accent.r, Accent.g, Accent.b, 0.42f));
        }

        private void DrawGameBackground()
        {
            if (gameBackground != null)
            {
                var backgroundRect = new Rect(0f, 0f, Screen.width, Screen.height);
                if (activeSceneId == "corridor-crowd")
                {
                    var zoomProgress = Mathf.Clamp01((Time.unscaledTime - corridorCrowdZoomStartedAt) / CorridorCrowdZoomDuration);
                    var zoomScale = Mathf.Lerp(1f, 1.16f, zoomProgress);
                    var zoomWidth = Screen.width * zoomScale;
                    var zoomHeight = Screen.height * zoomScale;
                    backgroundRect = new Rect((Screen.width - zoomWidth) * 0.5f, (Screen.height - zoomHeight) * 0.5f, zoomWidth, zoomHeight);
                }

                var transitionProgress = Mathf.Clamp01((Time.unscaledTime - backgroundTransitionStartedAt) / BackgroundTransitionDuration);
                if (previousGameBackground != null && transitionProgress < 1f)
                {
                    GUI.DrawTexture(backgroundRect, previousGameBackground, ScaleMode.ScaleAndCrop, true);
                    var previousColor = GUI.color;
                    GUI.color = new Color(1f, 1f, 1f, transitionProgress);
                    GUI.DrawTexture(backgroundRect, gameBackground, ScaleMode.ScaleAndCrop, true);
                    GUI.color = previousColor;
                }
                else
                {
                    previousGameBackground = null;
                    GUI.DrawTexture(backgroundRect, gameBackground, ScaleMode.ScaleAndCrop, true);
                }

                DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), new Color(Ink.r, Ink.g, Ink.b, 0.18f));
                DrawRect(new Rect(0f, Screen.height * 0.72f, Screen.width, Screen.height * 0.28f), new Color(Ink.r, Ink.g, Ink.b, 0.10f));
                return;
            }

            DrawRect(new Rect(0f, 0f, Screen.width, Screen.height), Ink);
        }

        private void UpdateGameBackgroundForCurrentLine()
        {
            var reachedLines = sequence.GetLinesThroughCurrent();
            var sceneId = string.Empty;
            for (var i = reachedLines.Length - 1; i >= 0; i--)
            {
                if (!string.IsNullOrEmpty(reachedLines[i].SceneId))
                {
                    sceneId = reachedLines[i].SceneId;
                    break;
                }
            }

            var sceneChanged = sceneId != activeSceneId;
            activeSceneId = sceneId;
            if (sceneChanged && activeSceneId == "corridor-crowd")
            {
                corridorCrowdZoomStartedAt = Time.unscaledTime;
            }

            Texture2D nextBackground;
            switch (sceneId)
            {
                case "bedroom":
                    nextBackground = prologueBedroomBackground;
                    break;
                case "corridor":
                case "corridor-crowd":
                    nextBackground = cleanCorridorBackground;
                    break;
                case "desk":
                    nextBackground = prologueDeskBackground;
                    break;
                case "office":
                default:
                    nextBackground = prologueOfficeBackground;
                    break;
            }

            if (nextBackground == null || nextBackground == gameBackground)
            {
                return;
            }

            previousGameBackground = gameBackground;
            gameBackground = nextBackground;
            backgroundTransitionStartedAt = Time.unscaledTime;
        }

        private bool IsSceneWipeActive => sceneWipeStartedAt >= 0f && Time.unscaledTime - sceneWipeStartedAt < SceneWipeDuration;

        private void DrawSceneWipe()
        {
            var progress = Mathf.Clamp01((Time.unscaledTime - sceneWipeStartedAt) / SceneWipeDuration);
            var easedProgress = Mathf.SmoothStep(0f, 1f, progress);
            if (sceneTransitionBandTexture == null)
            {
                sceneTransitionBandTexture = CreateTransitionBandTexture();
            }

            var bandHeight = Screen.height * 1.55f;
            var bandTop = Mathf.Lerp(Screen.height, -bandHeight, easedProgress);
            var previousColor = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, bandTop, Screen.width, bandHeight), sceneTransitionBandTexture, ScaleMode.StretchToFill, true);
            GUI.color = previousColor;
        }

        private static void DrawRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawActivePortrait()
        {
            if (kimYuinPortrait == null || sequence.Current.Speaker != "김유인")
            {
                return;
            }

            var height = Mathf.Min(Screen.height * 0.96f, 980f);
            var naturalWidth = height * kimYuinPortrait.width / kimYuinPortrait.height;
            var width = naturalWidth;
            var dialogueWidth = Mathf.Min(Screen.width - 48f, 1120f);
            var dialogueRight = (Screen.width + dialogueWidth) * 0.5f;
            var portraitBottom = Screen.height + 42f;
            var portraitTop = Mathf.Max(18f, portraitBottom - height);
            var portraitCenterX = dialogueRight - (naturalWidth * 0.85f) - 64f;
            var portraitRect = new Rect(portraitCenterX - width * 0.5f, portraitTop, width, height);
            GUI.DrawTexture(portraitRect, kimYuinPortrait, ScaleMode.ScaleToFit, true);
        }

        private void DrawAffiliationTag(Rect rect, string affiliation)
        {
            DrawRoundedRect(rect, new Color(0.60f, 0.43f, 0.22f, 0.98f));
            DrawRoundedRect(new Rect(rect.x + 1.5f, rect.y + 1.5f, rect.width - 3f, rect.height - 3f), new Color(0.15f, 0.085f, 0.035f, 0.98f));
            GUI.Label(new Rect(rect.x + 10f, rect.y, rect.width - 20f, rect.height), affiliation, affiliationStyle);
        }

        private static string GetSpeakerAffiliation(string speaker)
        {
            switch (speaker)
            {
                case "나":
                    return "TF 도서판매부";
                case "김유인":
                case "김민희":
                    return "TF 도서판매부";
                case "안내 방송":
                    return "사내 안내 방송";
                default:
                    return "???";
            }
        }

        private void DrawFramedPanel(Rect rect, Color fill, Color line)
        {
            DrawRoundedRect(rect, line);
            DrawRoundedRect(new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, rect.height - 2f), fill);
        }

        private void DrawRoundedRect(Rect rect, Color color)
        {
            var previousColor = GUI.color;
            GUI.color = color;
            GUI.Box(rect, GUIContent.none, roundedPanelStyle);
            GUI.color = previousColor;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;

            uiFont = CreateKoreanUiFont();
            var titleFont = CreateTitleFont();
            titleStyle = new GUIStyle(GUI.skin.label) { font = titleFont, fontSize = 46, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.95f, 0.96f, 0.98f) } };
            titleAccentStyle = new GUIStyle(titleStyle) { normal = { textColor = new Color(1f, 0.76f, 0.24f) } };
            titleShadowStyle = new GUIStyle(titleStyle) { normal = { textColor = new Color(0.02f, 0.02f, 0.025f, 0.82f) } };
            subtitleStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 19, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.86f, 0.87f, 0.89f) } };
            speakerStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 17, normal = { textColor = new Color(0.78f, 0.79f, 0.81f) } };
            affiliationStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 12, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.98f, 0.94f, 0.82f) } };
            archiveSpeakerStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 16, fontStyle = FontStyle.Bold, normal = { textColor = new Color(0.86f, 0.87f, 0.89f) } };
            archiveLogStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 20, wordWrap = true, alignment = TextAnchor.MiddleLeft, normal = { textColor = new Color(0.96f, 0.98f, 1f) } };
            galleryCaptionStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 15, wordWrap = true, alignment = TextAnchor.UpperLeft, normal = { textColor = new Color(0.78f, 0.79f, 0.81f) } };
            dialogueStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = DefaultDialogueSize, wordWrap = true, normal = { textColor = new Color(0.98f, 0.99f, 1f) } };
            bodyStyle = new GUIStyle(GUI.skin.label) { font = uiFont, fontSize = 22, wordWrap = true, normal = { textColor = new Color(0.92f, 0.93f, 0.95f) } };
            menuButtonStyle = new GUIStyle(GUI.skin.button)
            {
                font = uiFont,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(10, 10, 10, 10),
                normal = { background = CreateRoundedTexture(new Color(0.075f, 0.08f, 0.09f, 0.98f)), textColor = new Color(0.90f, 0.91f, 0.93f) },
                hover = { background = CreateRoundedTexture(new Color(0.16f, 0.17f, 0.18f, 1f)), textColor = Color.white },
                active = { background = CreateRoundedTexture(new Color(0.27f, 0.28f, 0.30f, 1f)), textColor = Color.white },
            };
            compactIconButtonStyle = new GUIStyle(GUI.skin.button)
            {
                border = new RectOffset(9, 9, 9, 9),
                normal = { background = CreateRoundedTexture(FunctionKeyFill) },
                hover = { background = CreateRoundedTexture(FunctionKeyHover) },
                active = { background = CreateRoundedTexture(FunctionKeyPressed) },
            };
            compactIconGlyphStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = FunctionKeyGlyph },
            };
            autoButtonStyle = new GUIStyle(GUI.skin.button)
            {
                font = uiFont,
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(9, 9, 9, 9),
                normal = { background = CreateRoundedTexture(FunctionKeyFill), textColor = FunctionKeyGlyph },
                hover = { background = CreateRoundedTexture(FunctionKeyHover), textColor = Color.white },
                active = { background = CreateRoundedTexture(FunctionKeyPressed), textColor = Color.white },
            };
            titleMenuButtonStyle = new GUIStyle(menuButtonStyle)
            {
                fontSize = 18,
            };
            pauseTitleStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.96f, 0.97f, 0.98f) },
            };
            confirmationStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 17,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = new Color(0.84f, 0.85f, 0.87f) },
            };
            screenHeadingStyle = new GUIStyle(GUI.skin.label)
            {
                font = uiFont,
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.96f, 0.97f, 0.98f) },
            };
            pauseButtonStyle = new GUIStyle(GUI.skin.button)
            {
                font = uiFont,
                fontSize = 17,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                border = new RectOffset(10, 10, 10, 10),
                normal = { background = CreateRoundedTexture(new Color(0.075f, 0.08f, 0.09f, 0.98f)), textColor = new Color(0.90f, 0.91f, 0.93f) },
                hover = { background = CreateRoundedTexture(new Color(0.16f, 0.17f, 0.18f, 1f)), textColor = Color.white },
                active = { background = CreateRoundedTexture(new Color(0.27f, 0.28f, 0.30f, 1f)), textColor = Color.white },
            };
            roundedPanelStyle = new GUIStyle(GUI.skin.box)
            {
                border = new RectOffset(10, 10, 10, 10),
                normal = { background = CreateRoundedTexture(Color.white) },
            };
            sliderTrackStyle = new GUIStyle(GUI.skin.horizontalSlider)
            {
                fixedHeight = 6f,
                border = new RectOffset(3, 3, 3, 3),
                normal = { background = CreateRoundedTexture(new Color(0.30f, 0.31f, 0.33f, 0.82f)) },
            };
            sliderThumbStyle = new GUIStyle(GUI.skin.horizontalSliderThumb)
            {
                fixedWidth = 12f,
                fixedHeight = 20f,
                normal = { background = CreateRoundedTexture(new Color(0.88f, 0.89f, 0.91f)) },
                hover = { background = CreateRoundedTexture(Color.white) },
                active = { background = CreateRoundedTexture(new Color(0.68f, 0.69f, 0.72f)) },
            };
        }

        private void RefreshResponsiveStyles()
        {
            // The dialogue UI has its own reference-canvas transform. All other screens
            // keep their screen-relative layout while their typography follows window size.
            var scale = screenMode == ScreenMode.Game
                ? 1f
                : Mathf.Clamp(Mathf.Min(Screen.width / GameUiReferenceWidth, Screen.height / GameUiReferenceHeight), 0.72f, 1.18f);
            var dialogueSize = PlayerPrefs.GetInt(DialogueSizeKey, DefaultDialogueSize);

            titleStyle.fontSize = Mathf.RoundToInt(46f * scale);
            titleAccentStyle.fontSize = titleStyle.fontSize;
            titleShadowStyle.fontSize = titleStyle.fontSize;
            subtitleStyle.fontSize = Mathf.RoundToInt(19f * scale);
            speakerStyle.fontSize = Mathf.RoundToInt(17f * scale);
            affiliationStyle.fontSize = Mathf.RoundToInt(12f * scale);
            archiveSpeakerStyle.fontSize = Mathf.RoundToInt(16f * scale);
            archiveLogStyle.fontSize = Mathf.RoundToInt(20f * scale);
            galleryCaptionStyle.fontSize = Mathf.RoundToInt(15f * scale);
            dialogueStyle.fontSize = Mathf.RoundToInt(dialogueSize * scale);
            bodyStyle.fontSize = Mathf.RoundToInt(22f * scale);
            menuButtonStyle.fontSize = Mathf.RoundToInt(17f * scale);
            titleMenuButtonStyle.fontSize = Mathf.RoundToInt(18f * scale);
            pauseTitleStyle.fontSize = Mathf.RoundToInt(24f * scale);
            confirmationStyle.fontSize = Mathf.RoundToInt(17f * scale);
            screenHeadingStyle.fontSize = Mathf.RoundToInt(26f * scale);
            compactIconGlyphStyle.fontSize = Mathf.RoundToInt(14f * scale);
            autoButtonStyle.fontSize = Mathf.RoundToInt(12f * scale);
            pauseButtonStyle.fontSize = Mathf.RoundToInt(17f * scale);
            sliderTrackStyle.fixedHeight = Mathf.RoundToInt(6f * scale);
            sliderThumbStyle.fixedWidth = Mathf.RoundToInt(12f * scale);
            sliderThumbStyle.fixedHeight = Mathf.RoundToInt(20f * scale);
        }

        private static Font CreateKoreanUiFont()
        {
            // Keep dialogue, backlog, and archive text in the same Korean family.
            // Malgun Gothic and Arial remain safe fallbacks for machines without Hancom Gothic.
            var preferredFonts = new[] { "Hancom Gothic", "Malgun Gothic", "Arial" };
            foreach (var fontName in preferredFonts)
            {
                try
                {
                    var font = Font.CreateDynamicFontFromOSFont(fontName, 18);
                    if (font != null)
                    {
                        return font;
                    }
                }
                catch (System.ArgumentException)
                {
                    // Try the next locally available fallback font.
                }
            }

            return GUI.skin.font;
        }

        private static Font CreateTitleFont()
        {
            var preferredFonts = new[] { "Hancom Gothic", "Batang", "Malgun Gothic" };
            foreach (var fontName in preferredFonts)
            {
                try
                {
                    var font = Font.CreateDynamicFontFromOSFont(fontName, 24);
                    if (font != null)
                    {
                        return font;
                    }
                }
                catch (System.ArgumentException)
                {
                    // Try a compatible Korean display-font fallback.
                }
            }

            return GUI.skin.font;
        }

        private static Texture2D CreateRoundedTexture(Color color)
        {
            const int size = 32;
            const float radius = 10f;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var nearestX = Mathf.Clamp(x, radius, size - radius - 1f);
                    var nearestY = Mathf.Clamp(y, radius, size - radius - 1f);
                    var distance = Vector2.Distance(new Vector2(x, y), new Vector2(nearestX, nearestY));
                    var pixel = color;
                    pixel.a *= Mathf.Clamp01(radius - distance + 0.5f);
                    texture.SetPixel(x, y, pixel);
                }
            }
            texture.Apply();
            return texture;
        }

        private static Texture2D CreateTransitionBandTexture()
        {
            const int width = 192;
            const int height = 256;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
            };

            for (var y = 0; y < height; y++)
            {
                var vertical = y / (float)(height - 1);
                for (var x = 0; x < width; x++)
                {
                    var tooth = Mathf.PingPong(x * 0.095f, 1f);
                    var edgeDepth = Mathf.Lerp(0.07f, 0.31f, tooth);
                    var topFade = Mathf.SmoothStep(0f, edgeDepth, vertical);
                    var bottomFade = 1f - Mathf.SmoothStep(1f - edgeDepth, 1f, vertical);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, topFade * bottomFade));
                }
            }

            texture.Apply();
            return texture;
        }
    }
}
