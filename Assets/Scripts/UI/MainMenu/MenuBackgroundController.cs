using System.Collections;
using UnityEngine;

namespace TheLastStop.Menu
{
    public sealed class MenuBackgroundController : MonoBehaviour
    {
        [SerializeField] private Light lamp;
        [SerializeField] private Renderer diffuser;
        [SerializeField] private Vector2 flickerInterval = new(9f, 23f);
        [SerializeField, Range(0f, 1f)] private float dimmedIntensity = .58f;

        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        private MaterialPropertyBlock originalProperties;
        private MaterialPropertyBlock flickerProperties;
        private Color originalEmission;
        private float originalIntensity;
        private Coroutine flicker;

        private void OnEnable()
        {
            if (!lamp)
                return;

            originalIntensity = lamp.intensity;
            if (diffuser && diffuser.sharedMaterial && diffuser.sharedMaterial.HasProperty(EmissionColor))
            {
                originalProperties ??= new MaterialPropertyBlock();
                flickerProperties ??= new MaterialPropertyBlock();
                diffuser.GetPropertyBlock(originalProperties);
                diffuser.GetPropertyBlock(flickerProperties);
                originalEmission = originalProperties.HasColor(EmissionColor)
                    ? originalProperties.GetColor(EmissionColor)
                    : diffuser.sharedMaterial.GetColor(EmissionColor);
            }

            flicker = StartCoroutine(Flicker());
        }

        private void OnDisable()
        {
            if (flicker == null)
                return;

            StopCoroutine(flicker);
            flicker = null;
            if (lamp)
                lamp.intensity = originalIntensity;
            if (diffuser && originalProperties != null)
                diffuser.SetPropertyBlock(originalProperties.isEmpty ? null : originalProperties);
        }

        private IEnumerator Flicker()
        {
            while (true)
            {
                float min = Mathf.Max(1f, flickerInterval.x);
                float max = Mathf.Max(min, flickerInterval.y);
                yield return new WaitForSecondsRealtime(Random.Range(min, max));

                SetLamp(dimmedIntensity);
                yield return new WaitForSecondsRealtime(Random.Range(.052f, .115f));
                SetLamp(1f);

                if (Random.value < .4f)
                {
                    yield return new WaitForSecondsRealtime(.08f);
                    SetLamp(Mathf.Lerp(dimmedIntensity, 1f, .45f));
                    yield return new WaitForSecondsRealtime(.055f);
                    SetLamp(1f);
                }
            }
        }

        private void SetLamp(float gain)
        {
            if (lamp)
                lamp.intensity = originalIntensity * gain;
            if (diffuser && flickerProperties != null)
            {
                flickerProperties.SetColor(EmissionColor, originalEmission * gain);
                diffuser.SetPropertyBlock(flickerProperties);
            }
        }
    }
}

