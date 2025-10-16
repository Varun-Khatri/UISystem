using UnityEngine;
using UnityEngine.UI;

namespace VK.UI.Samples
{
    public class PausePanel : UIPanel
    {

        [SerializeField] private Button _playButton;
        [SerializeField] private UIManager _uiManager;

        protected override void Awake()
        {
            base.Awake();
            _playButton.onClick.AddListener(OnPlayClicked);
        }

        private void OnDestroy()
        {
            _playButton.onClick.RemoveAllListeners();
        }

        private void OnPlayClicked()
        {
            _uiManager.HideUI<PausePanel>();
        }

        protected override void OnShow()
        {
            Debug.Log("Game Paused");
            Time.timeScale = 0f;
        }

        protected override void OnHide()
        {
            Debug.Log("Game Resumed");
            Time.timeScale = 1f;
        }
    }
}