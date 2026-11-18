using System;
using System.Collections.Generic;
using FireSlop.Interaction;
using UnityEngine;

namespace FireSlop.MissionHud
{
    [CreateAssetMenu(menuName = "Fire Slop/HUD/Action Catalog")]
    public sealed class HudActionCatalogSO : ScriptableObject
    {
        [SerializeField] private HudActionDefinition[] _actions = Array.Empty<HudActionDefinition>();

        public HudActionDefinition Get(HudActionId id)
        {
            foreach (var action in _actions)
            {
                if (action.Id == id)
                {
                    return action;
                }
            }

            throw new InvalidOperationException($"HUD action catalog '{name}' is missing '{id}'.");
        }

        public void Validate()
        {
            var ids = new HashSet<HudActionId>();
            foreach (var action in _actions)
            {
                if (action == null || action.Id == HudActionId.None ||
                    !Enum.IsDefined(typeof(HudActionId), action.Id) || !ids.Add(action.Id))
                {
                    throw new InvalidOperationException($"HUD action catalog '{name}' contains an invalid or repeated ID.");
                }
            }

            foreach (HudActionId id in Enum.GetValues(typeof(HudActionId)))
            {
                if (id != HudActionId.None && !ids.Contains(id))
                {
                    throw new InvalidOperationException($"HUD action catalog '{name}' is missing '{id}'.");
                }
            }
        }
    }
}
