using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VK.UI.Samples
{
    public class GameHUD : UIHUD
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _healthText;
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _ammoText;
        [SerializeField] private Slider _shieldSlider;

        [SerializeField] private PlayerData _playerData = new PlayerData();

        protected override void Awake()
        {
            base.Awake();
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            _healthText.text = $"HP: {_playerData.Health}";
            _scoreText.text = $"Score: {_playerData.Score}";
            _ammoText.text = $"Ammo: {_playerData.Ammo}";
            _shieldSlider.value = _playerData.ShieldPercent;
        }


        protected override void OnShow()
        {
            Debug.Log("Game HUD Activated");
        }
        protected override void OnHide()
        {
            Debug.Log("Game HUD Deactivated");
        }
    }
}