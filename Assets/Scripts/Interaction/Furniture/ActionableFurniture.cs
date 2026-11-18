using UnityEngine;

namespace FireSlop.Interaction
{
    [DisallowMultipleComponent]
    public sealed class ActionableFurniture : MonoBehaviour
    {
        [SerializeField] private FurnitureAction _action;
        [SerializeField] private CabinetView _openable;

        internal CabinetView Openable => isActiveAndEnabled && _action == FurnitureAction.Openable ? _openable : null;
    }
}
