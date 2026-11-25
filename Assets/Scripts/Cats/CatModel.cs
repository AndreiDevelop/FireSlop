using System;

namespace FireSlop.Cats
{
    public sealed class CatModel
    {
        public CatState State { get; private set; }
        public float StateSeconds { get; private set; }
        public float WaterSeconds { get; private set; }
        public event Action StateChanged;
        public event Action Calmed;

        public void Advance(float seconds)
        {
            if (seconds > 0f && !float.IsInfinity(seconds))
            {
                StateSeconds += seconds;
            }
        }

        public void SetThreatened(bool threatened)
        {
            if (State == CatState.Idle && threatened)
            {
                ChangeState(CatState.Warning);
            }
            else if (State == CatState.Warning && !threatened)
            {
                ChangeState(CatState.Idle);
            }
        }

        public bool ApplyWater(float seconds, float requiredSeconds)
        {
            if ((State != CatState.Idle && State != CatState.Warning && State != CatState.Jumping) || seconds <= 0f ||
                float.IsInfinity(seconds))
            {
                return false;
            }

            WaterSeconds += seconds;
            if (State != CatState.Jumping && WaterSeconds < requiredSeconds)
            {
                return false;
            }

            ChangeState(CatState.Settled);
            Calmed?.Invoke();
            return true;
        }

        public bool TryJump(float warningSeconds)
        {
            if (State != CatState.Warning || StateSeconds < warningSeconds)
            {
                return false;
            }

            ChangeState(CatState.Jumping);
            return true;
        }

        public void FinishJump()
        {
            if (State == CatState.Jumping)
            {
                ChangeState(CatState.Attached);
            }
        }

        public void Remove()
        {
            if (State == CatState.Attached || State == CatState.Jumping)
            {
                ChangeState(CatState.Returning);
            }
        }

        public void FinishReturn()
        {
            if (State == CatState.Returning)
            {
                ChangeState(CatState.Settled);
            }
        }

        public bool TryRescue()
        {
            if (State != CatState.Settled)
            {
                return false;
            }

            ChangeState(CatState.Rescued);
            return true;
        }

        public bool TryEndCooldown(float cooldownSeconds, bool isCarried)
        {
            if (State != CatState.Settled || isCarried || StateSeconds < cooldownSeconds)
            {
                return false;
            }

            WaterSeconds = 0f;
            ChangeState(CatState.Idle);
            return true;
        }

        private void ChangeState(CatState state)
        {
            State = state;
            StateSeconds = 0f;
            StateChanged?.Invoke();
        }
    }
}
