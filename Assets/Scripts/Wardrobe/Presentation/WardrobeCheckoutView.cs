using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FireSlop.Wardrobe
{
    public sealed class WardrobeCheckoutView : WardrobeView
    {
        private const string Coin = "<sprite name=\"Coin\">";
        [SerializeField] private TMP_Text _balance;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Button _button;

        protected override void Render()
        {
            _balance.text = $"{Coin} {CompactNumber.Format(Model.Profile.Balance)}";
            _label.text = Model.HasUnowned ? $"{Coin} {CompactNumber.Format(Model.Total)}  Buy" : "Save";
            _button.interactable = Model.Total <= Model.Profile.Balance;
        }

        public void Confirm()
        {
            Controller.Confirm();
        }
    }
}
