using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class DoorView : MonoBehaviour
{
    public Image doorImage;
    public Color normalColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;
    public float animationDuration = 0.5f;

    private Coroutine currentAnimation;

    public void PlayOpenAnimation(Action onComplete)
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(OpenAnimationCoroutine(onComplete));
    }

    public void PlayCloseAnimation(Action onComplete)
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(CloseAnimationCoroutine(onComplete));
    }

    public void PlayFailAnimation(Action onComplete)
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        currentAnimation = StartCoroutine(FailAnimationCoroutine(onComplete));
    }

    public void ResetView()
    {
        if (currentAnimation != null) StopCoroutine(currentAnimation);
        doorImage.color = normalColor;
        doorImage.transform.localScale = Vector3.one;
        doorImage.transform.localRotation = Quaternion.identity;
    }

    private IEnumerator OpenAnimationCoroutine(Action onComplete)
    {
        float elapsed = 0f;
        Quaternion startRot = Quaternion.identity;
        Quaternion endRot = Quaternion.Euler(0, 90, 0);

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            doorImage.color = Color.Lerp(normalColor, successColor, t);
            doorImage.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        doorImage.color = successColor;
        doorImage.transform.localRotation = endRot;
        onComplete?.Invoke();
    }

    private IEnumerator CloseAnimationCoroutine(Action onComplete)
    {
        float elapsed = 0f;
        Quaternion startRot = Quaternion.Euler(0, 90, 0);
        Quaternion endRot = Quaternion.identity;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            doorImage.color = Color.Lerp(successColor, normalColor, t);
            doorImage.transform.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }

        doorImage.color = normalColor;
        doorImage.transform.localRotation = endRot;
        onComplete?.Invoke();
    }

    private IEnumerator FailAnimationCoroutine(Action onComplete)
    {
        doorImage.color = failColor;
        
        for (int i = 0; i < 3; i++)
        {
            doorImage.transform.localScale = Vector3.one * 1.1f;
            yield return new WaitForSeconds(0.1f);
            doorImage.transform.localScale = Vector3.one;
            yield return new WaitForSeconds(0.1f);
        }

        onComplete?.Invoke();
    }
}