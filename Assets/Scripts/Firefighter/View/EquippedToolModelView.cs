using FireSlop.Tools;
using UnityEngine;

namespace FireSlop.Firefighter
{
    public sealed class EquippedToolModelView : MonoBehaviour
    {
        [SerializeField] private ToolDefinitionSO _tool;

        private Renderer[] _renderers;

        public string ToolId => _tool ? _tool.Id : null;

        public void Prepare()
        {
            _renderers = GetComponentsInChildren<Renderer>(true);
        }

        public void Render(bool equipped, bool hideRenderers)
        {
            foreach (var renderer in _renderers)
            {
                renderer.forceRenderingOff = hideRenderers;
            }

            gameObject.SetActive(equipped);
        }
    }
}
