using UnityEngine;

namespace FireSlop.Interaction
{
    internal sealed class BrickDustEffect
    {
        private ParticleSystem _dust;

        public void Play(RaycastHit hit, ParticleSystem prefab)
        {
            if (_dust == null)
            {
                if (prefab == null)
                {
                    Debug.LogError("Assign the brick dust prefab in the interaction configuration.", hit.collider);
                    return;
                }

                var parameters = new InstantiateParameters { scene = hit.collider.gameObject.scene };
                _dust = Object.Instantiate(prefab, parameters);
            }

            _dust.transform.SetPositionAndRotation(hit.point, Quaternion.LookRotation(hit.normal));
            _dust.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            _dust.Play(false);
        }
    }
}
