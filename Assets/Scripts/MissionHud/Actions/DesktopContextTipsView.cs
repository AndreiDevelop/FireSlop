using FireSlop.Interaction;
using TMPro;
using UnityEngine;

namespace FireSlop.MissionHud
{
    public sealed class DesktopContextTipsView : ContextPromptView
    {
        [SerializeField] private TextMeshProUGUI _hint;

        protected override void Display(HudActionDefinition action, bool closed)
        {
            var label = action?.Label ?? string.Empty;
            _hint.text = closed ? label : "<sprite name=\"E\">  " + label;
            if (!closed && Model.IsCarrying && !Model.IsPilotingDrone)
            {
                _hint.text += "   |   <sprite name=\"MouseLeft\">  " + Actions.Get(HudActionId.Throw).Label;
            }

            var canShow = !Model.IsPilotingDrone || action?.Id == HudActionId.RemoveCat;
            _hint.gameObject.SetActive(action != null && canShow);
        }
    }
}
