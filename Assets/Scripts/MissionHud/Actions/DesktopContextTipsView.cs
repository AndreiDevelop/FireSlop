using TMPro;
using UnityEngine;

namespace FireSlop.MissionHud
{
    public sealed class DesktopContextTipsView : ContextPromptView
    {
        [SerializeField] private TextMeshProUGUI _hint;

        protected override void Display(string action, bool closed)
        {
            _hint.text = closed ? action : "<sprite name=\"E\">  " + action;
            if (!closed && Model.IsCarrying && !Model.IsPilotingDrone)
            {
                _hint.text += "   |   LMB  Throw";
            }

            _hint.gameObject.SetActive(action.Length > 0 && !Model.IsPilotingDrone);
        }
    }
}
