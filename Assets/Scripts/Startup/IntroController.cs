using System.Collections;
using FireSlop.Dispatch;
using FireSlop.Wardrobe;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.Startup
{
    public sealed class IntroController : MonoBehaviour
    {
        private const string MainScene = "Main";
        private const float ReadyToActivateProgress = 0.9f;

        [SerializeField] private LoadingProgressView _loadingProgress;

        [SerializeField] private GameObject _menu;
        [SerializeField] private Button _play;

        private AsyncOperation _loading;
        private DispatchModel _dispatch;
        private WardrobeModel _wardrobe;
        private bool _starting;

        [Inject]
        public void Construct(DispatchModel dispatch, WardrobeModel wardrobe)
        {
            _dispatch = dispatch;
            _wardrobe = wardrobe;
        }

        private void OnEnable()
        {
            _play.onClick.AddListener(Play);
        }

        private void Play()
        {
            if (_starting || _dispatch == null || _dispatch.Panel != DispatchPanel.None || _wardrobe.IsOpen)
            {
                return;
            }

            _starting = true;
            _play.interactable = false;
            _menu.SetActive(false);
            StartCoroutine(LoadMission());
        }

        private IEnumerator LoadMission()
        {
            if (_loadingProgress == null || !_loadingProgress.IsConfigured)
            {
                Debug.LogError("Assign the loading progress view and slider in Intro.", this);
                SceneManager.LoadSceneAsync(MainScene, LoadSceneMode.Single);
                yield break;
            }

            _loadingProgress.SetProgress(0f);
            _loading = SceneManager.LoadSceneAsync(MainScene, LoadSceneMode.Single);
            _loading.allowSceneActivation = false;

            while (_loading.progress < ReadyToActivateProgress)
            {
                _loadingProgress.SetProgress(_loading.progress / ReadyToActivateProgress);
                yield return null;
            }

            _loadingProgress.SetProgress(1f);
            yield return null;
            AllowSceneActivation();
        }

        private void OnDisable()
        {
            _play.onClick.RemoveListener(Play);
            StopAllCoroutines();
            AllowSceneActivation();
        }

        private void AllowSceneActivation()
        {
            if (_loading == null)
            {
                return;
            }

            _loading.allowSceneActivation = true;
            _loading = null;
        }
    }
}
