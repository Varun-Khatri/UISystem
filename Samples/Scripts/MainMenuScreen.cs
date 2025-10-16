using UnityEngine;
using UnityEngine.UI;

namespace VK.UI.Samples
{
    // Game data structures
    public enum GameState { MainMenu, Playing, Paused, GameOver }

    [System.Serializable]
    public class PlayerData
    {
        public int Health = 100;
        public int Score = 0;
        public int Ammo = 30;
        public float ShieldPercent = 1.0f;
    }

    [System.Serializable]
    public class GameSettings
    {
        public float MusicVolume = 0.8f;
        public float SFXVolume = 0.8f;
        public bool Fullscreen = true;
    }
    public class MainMenuScreen : UIScreen
    {
        [Header("UI References")]
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
            _uiManager.ShowUI<GameScreen>();
            _uiManager.ShowUI<GameHUD>();
        }

        protected override void OnShow()
        {
            Debug.Log("Main Menu Opened");
        }
        protected override void OnHide()
        {
            Debug.Log("Main Menu Closed");
        }
    }
}
