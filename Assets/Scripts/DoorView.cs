using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 演出のみ（ここだけ3Dに差し替え可能）
/// </summary>
public class DoorView : MonoBehaviour
{
    [Header("References")]
    public Image doorImage;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color successColor = Color.green;
    public Color failColor = Color.red;

    [Header("Animation")]
    [Range(0.05f, 2f)]
    public float flashDuration = 0.25f;

    private Coroutine _routine;

    private void Reset()
    {
        doorImage = GetComponentInChildren<Image>();
    }

    public void SetNormal()
    {
        if (_routine != null) StopCoroutine(_routine);
        if (doorImage != null) doorImage.color = normalColor;
    }

    public void FlashSuccess()
    {
        // DoorImageが非アクティブなら何もしない
        if (doorImage == null || !doorImage.gameObject.activeInHierarchy) return;
        Flash(successColor);
    }

    public void FlashFail()
    {
        // DoorImageが非アクティブなら何もしない
        if (doorImage == null || !doorImage.gameObject.activeInHierarchy) return;
        Flash(failColor);
    }

    private void Flash(Color c)
    {
        if (doorImage == null || !doorImage.gameObject.activeInHierarchy) return;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(FlashRoutine(c));
    }

    private IEnumerator FlashRoutine(Color c)
    {
        doorImage.color = c;
        yield return new WaitForSeconds(flashDuration);
        doorImage.color = normalColor;
        _routine = null;
    }
}
