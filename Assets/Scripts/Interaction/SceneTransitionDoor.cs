using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionDoor : MonoBehaviour, IInteractable
{
    [Header("Сцена и тайминги")]
    [SerializeField] private string targetSceneName = "Apart";
    [SerializeField] private float minLoadDuration = 1.8f; // Сколько секунд идут проценты

    [Header("UI элементы")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text percentText;

    [Header("Атмосфера (мерцание и дрожание)")]
    [SerializeField] private bool enableFlicker = true;
    [SerializeField] private float shakeIntensity = 1.5f;

    private bool isLoading;
    private Vector3 percentBasePos;

    private void Awake()
    {
        if (percentText != null)
            percentBasePos = percentText.rectTransform.anchoredPosition;
    }

    public void Interact()
    {
        if (isLoading) return;
        StartCoroutine(LoadSceneWithProgress());
    }

    private IEnumerator LoadSceneWithProgress()
    {
        isLoading = true;

        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        if (statusText != null)
            statusText.text = "КТО-ТО ОТКРЫВАЕТ ЗАМОК...";

        if (percentText != null)
            percentText.text = "0%";

        yield return null;

        AsyncOperation operation = SceneManager.LoadSceneAsync(targetSceneName);
        operation.allowSceneActivation = false;

        float elapsed = 0f;
        int lastShownPercent = -1;

        while (!operation.isDone)
        {
            elapsed += Time.deltaTime;

            float sceneProgress = Mathf.Clamp01(operation.progress / 0.9f);
            float timeProgress = Mathf.Clamp01(elapsed / minLoadDuration);
            float combined = Mathf.Min(sceneProgress, timeProgress);

            int percent = Mathf.RoundToInt(combined * 100f);

            if (percentText != null && percent != lastShownPercent)
            {
                lastShownPercent = percent;
                percentText.text = $"{percent}%";
            }

            if (statusText != null && percent >= 75)
                statusText.text = "НЕ ОСТАВАЙСЯ В КОРИДОРЕ...";

            if (enableFlicker && percentText != null)
            {
                float noise = Mathf.PerlinNoise(Time.time * 18f, 0.5f);
                Color baseCol = percent >= 85 ? new Color(0.68f, 0.16f, 0.16f) : new Color(0.78f, 0.78f, 0.78f);
                baseCol.a = Mathf.Lerp(0.45f, 1f, noise);
                percentText.color = baseCol;

                float offsetX = Random.Range(-shakeIntensity, shakeIntensity);
                float offsetY = Random.Range(-shakeIntensity, shakeIntensity);
                percentText.rectTransform.anchoredPosition = percentBasePos + new Vector3(offsetX, offsetY, 0f);
            }

            if (operation.progress >= 0.9f && elapsed >= minLoadDuration)
            {
                if (percentText != null)
                {
                    percentText.rectTransform.anchoredPosition = percentBasePos;
                    percentText.text = "100%";
                }

                yield return new WaitForSeconds(0.25f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}