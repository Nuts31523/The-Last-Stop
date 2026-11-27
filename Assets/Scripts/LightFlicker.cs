using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Light))]
public sealed class LightFlicker : MonoBehaviour
{
    private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

    [SerializeField] private Renderer diffuser = null;
    [SerializeField] private Transform player = null;
    [Tooltip("Optional landing whose height the player must reach to start the effect.")]
    [SerializeField] private Transform activationLanding = null;
    [Tooltip("Offset from the landing to the start of the next flight, in world X and Z metres.")]
    [SerializeField] private Vector2 landingApproachOffset = Vector2.zero;
    [Tooltip("Random time the lamp stays on, in seconds (min, max).")]
    [SerializeField] private Vector2 pauseRange = new Vector2(1.5f, 2.5f);
    [Tooltip("Random duration of each blackout, in seconds (min, max).")]
    [SerializeField] private Vector2 blackoutRange = new Vector2(3.5f, 4.5f);
    [Tooltip("Chance of a dim restart when there is no flicker burst.")]
    [SerializeField, Range(0f, 1f)] private float dimRestartChance = 0.45f;
    [Tooltip("Chance of a short flicker after a regular blackout.")]
    [SerializeField, Range(0f, 1f)] private float burstRestartChance = 0.3f;

    private Light lamp;
    private float normalIntensity;
    private Color normalEmission;
    private MaterialPropertyBlock originalProperties;
    private MaterialPropertyBlock blackoutProperties;
    private MaterialPropertyBlock dimProperties;
    private Coroutine flicker;

    private void OnEnable()
    {
        lamp = GetComponent<Light>();
        normalIntensity = lamp.intensity;
        var material = diffuser != null ? diffuser.sharedMaterial : null;
        if (material != null && material.HasProperty(EmissionColor))
        {
            originalProperties = new MaterialPropertyBlock();
            blackoutProperties = new MaterialPropertyBlock();
            dimProperties = new MaterialPropertyBlock();
            diffuser.GetPropertyBlock(originalProperties);
            diffuser.GetPropertyBlock(blackoutProperties);
            diffuser.GetPropertyBlock(dimProperties);
            normalEmission = originalProperties.HasColor(EmissionColor)
                ? originalProperties.GetColor(EmissionColor)
                : material.GetColor(EmissionColor);
            blackoutProperties.SetColor(EmissionColor, Color.black);
        }
    }

    private void OnDisable()
    {
        StopFlicker();
        originalProperties = null;
        blackoutProperties = null;
        dimProperties = null;
    }

    private void LateUpdate()
    {
        // Keep the effect running across small height changes at the edge of the landing.
        float heightTolerance = flicker != null ? 0.12f : 0f;
        if (activationLanding != null &&
            (player == null || player.position.y < activationLanding.position.y - heightTolerance))
        {
            StopFlicker();
            return;
        }

        if (flicker == null && lamp.enabled && HasReachedApproach())
            flicker = StartCoroutine(Flicker());
    }

    private bool HasReachedApproach()
    {
        if (activationLanding == null)
            return true;

        Vector3 landing = activationLanding.position;
        Vector3 playerPosition = player.position;
        Vector2 approach = new Vector2(landing.x, landing.z) + landingApproachOffset;
        Vector2 playerOffset = new Vector2(playerPosition.x, playerPosition.z) - approach;
        const float approachRadius = 1.05f;
        return playerOffset.sqrMagnitude <= approachRadius * approachRadius;
    }

    private void StopFlicker()
    {
        if (flicker == null)
            return;

        StopCoroutine(flicker);
        flicker = null;
        SetLampOn(true);
    }

    private IEnumerator Flicker()
    {
        yield return FlickerBurst();
        SetLampOn(true, Random.Range(0.2f, 0.4f));

        bool firstRestart = true;
        while (true)
        {
            yield return new WaitForSeconds(RandomDuration(pauseRange));

            SetLampOn(false);
            yield return new WaitForSeconds(RandomDuration(blackoutRange));

            float brightness = 1f;
            if (!firstRestart && Random.value < burstRestartChance)
            {
                yield return FlickerBurst();
            }
            else if (!firstRestart && Random.value < dimRestartChance)
            {
                brightness = Random.Range(0.2f, 0.4f);
            }

            SetLampOn(true, brightness);
            firstRestart = false;
        }
    }

    private IEnumerator FlickerBurst()
    {
        int pulseCount = Random.Range(2, 4);
        for (int pulse = 0; pulse < pulseCount; pulse++)
        {
            SetLampOn(true, Random.Range(0.45f, 0.85f));
            yield return new WaitForSeconds(Random.Range(0.15f, 0.23f));

            SetLampOn(false);
            yield return new WaitForSeconds(Random.Range(0.21f, 0.32f));
        }
    }

    private void SetLampOn(bool isOn, float brightness = 1f)
    {
        lamp.intensity = normalIntensity * brightness;
        lamp.enabled = isOn;

        if (diffuser == null || blackoutProperties == null)
            return;

        if (isOn && brightness < 1f)
        {
            dimProperties.SetColor(EmissionColor, Color.Lerp(Color.black, normalEmission, brightness));
            diffuser.SetPropertyBlock(dimProperties);
            return;
        }

        diffuser.SetPropertyBlock(isOn ? originalProperties : blackoutProperties);
    }

    private static float RandomDuration(Vector2 range)
    {
        float minimum = Mathf.Max(0.01f, Mathf.Min(range.x, range.y));
        float maximum = Mathf.Max(minimum, Mathf.Max(range.x, range.y));
        return Random.Range(minimum, maximum);
    }
}
