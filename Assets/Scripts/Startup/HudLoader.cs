using UnityEngine;
using UnityEngine.SceneManagement;

namespace FireSlop.Startup
{
    public sealed class HudLoader : MonoBehaviour
    {
        private void Start()
        {
            if (!SceneManager.GetSceneByName("HUD").isLoaded)
            {
                SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
            }
        }
    }
}
