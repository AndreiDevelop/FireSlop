using System;
using System.Collections.Generic;
using UnityEngine;

namespace FireSlop.Wardrobe
{
    [Serializable]
    internal sealed class WardrobeSaveData
    {
        [SerializeField] internal int balance;
        [SerializeField] internal List<string> owned = new List<string>();
        [SerializeField] internal List<string> equipped = new List<string>();
    }
}
