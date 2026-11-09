using UnityEngine;

namespace TheLastStop
{
    [RequireComponent(typeof(CanvasGroup))]
    [AddComponentMenu("The Last Stop/Interaction Hint")]
    public sealed class InteractionHint : MonoBehaviour
    {
        [Tooltip("The interaction zone that displays this hint when the player enters.")]
        [SerializeField] private InteractionTrigger trigger;
        [Tooltip("Fade-in and fade-out duration in seconds.")]
        [SerializeField, Min(0.01f)] private float fadeDuration = 0.15f;

        private CanvasGroup canvasGroup;
        private bool visible;

        private void Awake() => canvasGroup = GetComponent<CanvasGroup>();

        private void OnEnable()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (trigger != null) trigger.AvailabilityChanged.AddListener(SetVisible);
            visible = trigger != null && trigger.IsAvailable;
            canvasGroup.alpha = visible ? 1 : 0;
        }

        private void OnDisable()
        {
            if (trigger != null) trigger.AvailabilityChanged.RemoveListener(SetVisible);
            if (canvasGroup != null) canvasGroup.alpha = 0;
        }

        private void Update()
        {
            visible = trigger != null && trigger.isActiveAndEnabled && trigger.IsAvailable;
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, visible ? 1 : 0,
                Time.unscaledDeltaTime / Mathf.Max(0.01f, fadeDuration));
        }

        private void SetVisible(bool value) => visible = value;
    }
}
