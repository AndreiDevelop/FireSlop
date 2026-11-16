using UnityEngine;
using UnityEngine.SceneManagement;

namespace FireSlop.Startup
{
    public sealed class IntroController : MonoBehaviour
    {
        private void Start()
        {
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
        }
    }
}
