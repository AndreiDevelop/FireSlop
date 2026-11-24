using System;
using FireSlop.Wardrobe;
using TMPro;
using UnityEngine;
using Zenject;

namespace FireSlop.Dispatch
{
    public sealed class DispatchBalanceView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _amount;
        private WardrobeProfile _profile;

        [Inject]
        public void Construct(WardrobeProfile profile)
        {
            if (_amount == null)
            {
                throw new InvalidOperationException("Assign the Dispatch balance label.");
            }

            _profile = profile;
            if (isActiveAndEnabled)
            {
                OnEnable();
            }
        }

        private void OnEnable()
        {
            if (_profile != null)
            {
                _profile.Changed += Render;
                Render();
            }
        }

        private void OnDisable()
        {
            if (_profile != null)
            {
                _profile.Changed -= Render;
            }
        }

        private void Render()
        {
            _amount.text = _profile.LoadError == null
                ? $"<sprite name=\"Coin\"> {CompactNumber.Format(_profile.Balance)}" : "PROFILE UNAVAILABLE";
        }
    }
}
