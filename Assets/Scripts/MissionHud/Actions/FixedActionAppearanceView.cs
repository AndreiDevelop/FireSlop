using FireSlop.Interaction;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class FixedActionAppearanceView : MonoBehaviour
    {
        [SerializeField] private HudActionId _actionId;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Image _icon;

        [Inject]
        public void Construct(HudActionCatalogSO actions)
        {
            if (_label == null || _icon == null)
            {
                Debug.LogError("Fixed HUD action requires a label and an icon image.", this);
                return;
            }

            var action = actions.Get(_actionId);
            _label.text = action.Label;
            _icon.sprite = action.Icon;
            _icon.enabled = action.Icon != null;
        }
    }
}
