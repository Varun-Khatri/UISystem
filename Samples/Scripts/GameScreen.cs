using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VK.UI.Samples
{
    public class GameScreen : UIScreen
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _missionText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _backButton;

        [SerializeField] private UIManager _uiManager;

        protected override void Awake()
        {
            base.Awake();
            _pauseButton.onClick.AddListener(OnPauseClicked);
            _backButton.onClick.AddListener(OnBackClicked);
        }

        private void OnDestroy()
        {
            _pauseButton?.onClick.RemoveListener(OnPauseClicked);
            _backButton?.onClick.RemoveListener(OnBackClicked);
        }

        private void OnPauseClicked()
        {
            _uiManager.ShowUI<PausePanel>();
        }
        private void OnBackClicked()
        {
            _uiManager.GoBack();
            _uiManager.HideUI<GameHUD>();
        }

        protected override void OnShow()
        {
            _missionText.text = "Mission: Explore the Alpha Centauri System";
            Debug.Log("Game Screen Started");
        }
        protected override void OnHide()
        {
            _missionText.text = string.Empty;
            Debug.Log("Game Screen Ended");
        }
    }
}