using System;
using FireSlop.Interaction;
using UnityEngine;

namespace FireSlop.MissionHud
{
    [Serializable]
    public sealed class HudActionDefinition
    {
        [SerializeField] private HudActionId _id;
        [SerializeField] private string _label;
        [SerializeField, Tooltip("Leave Use Tool empty to display the selected tool icon.")]
        private Sprite _icon;

        public HudActionId Id => _id;
        public string Label => _label;
        public Sprite Icon => _icon;
    }
}
