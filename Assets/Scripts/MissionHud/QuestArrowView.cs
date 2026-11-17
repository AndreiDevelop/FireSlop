using FireSlop.Interaction;
using FireSlop.Mission;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace FireSlop.MissionHud
{
    public sealed class QuestArrowView : MonoBehaviour
    {
        public const string EntranceId = "QuestEntrance";

        [SerializeField] private RectTransform _arrow;
        [SerializeField] private Text _objective;
        [SerializeField] private string _entranceText;
        [SerializeField] private string _prototypeRescueText;
        [SerializeField] private string _findText;
        [SerializeField] private string _pickupText;
        [SerializeField] private string _deliveryText;

        private Camera _camera;
        private QuestTargetModel _quest;
        private Transform _entrance;
        private Transform _resident;
        private Transform _rescueZone;
        private Transform _target;

        [Inject]
        public void Construct(Camera camera, QuestTargetModel quest, [Inject(Id = EntranceId)] Transform entrance,
            [InjectOptional] ResidentView resident, [InjectOptional] RescueZoneView rescueZone)
        {
            _camera = camera;
            _quest = quest;
            _entrance = entrance;
            _resident = resident != null ? resident.transform : null;
            _rescueZone = rescueZone != null ? rescueZone.transform : null;

            if (isActiveAndEnabled)
            {
                Subscribe();
            }
        }

        private void OnEnable()
        {
            if (_quest != null)
            {
                Subscribe();
            }
        }

        private void OnDisable()
        {
            if (_quest != null)
            {
                _quest.Changed -= Render;
            }
        }

        private void Subscribe()
        {
            _quest.Changed += Render;
            Render();
        }

        private void Render()
        {
            switch (_quest.Step)
            {
                case QuestStep.ReachEntrance:
                    _objective.text = _entranceText;
                    _target = _entrance;
                    break;
                case QuestStep.RescuePrototypeResident:
                    _objective.text = _prototypeRescueText;
                    _target = _entrance;
                    break;
                case QuestStep.FindResident:
                    _objective.text = _findText;
                    _target = _resident;
                    break;
                case QuestStep.PickUpResident:
                    _objective.text = _pickupText;
                    _target = _resident;
                    break;
                case QuestStep.DeliverResident:
                    _objective.text = _deliveryText;
                    _target = _rescueZone;
                    break;
                case QuestStep.Complete:
                    gameObject.SetActive(false);
                    break;
            }
        }

        private void LateUpdate()
        {
            if (_camera == null || _target == null)
            {
                return;
            }

            var offset = _target.position - _camera.transform.position;
            var heading = Vector3.ProjectOnPlane(_camera.transform.forward, Vector3.up);
            var direction = Vector3.ProjectOnPlane(offset, Vector3.up);
            var angle = Vector3.SignedAngle(heading, direction, Vector3.up);
            _arrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
        }
    }
}
