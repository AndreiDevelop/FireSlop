using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.MissionHud
{
    public sealed class ContextActionView : ContextPromptView
    {
        [SerializeField] private Button _touchButton;
        [SerializeField] private TMP_Text _touchLabel;
        [SerializeField] private Image _touchIcon;

        private void Awake()
        {
            _touchButton.onClick.AddListener(Interact);
        }

        private void OnDestroy()
        {
            _touchButton.onClick.RemoveListener(Interact);
        }

        private void Interact()
        {
            Model.Interact();
        }

        protected override void Display(HudActionDefinition action, bool closed)
        {
            _touchLabel.text = action?.Label ?? string.Empty;
            _touchIcon.sprite = action?.Icon;
            _touchIcon.enabled = _touchIcon.sprite != null;
            _touchButton.gameObject.SetActive(action != null);
        }
    }
}
