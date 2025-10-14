using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public RectTransform spawnArea;
    public Image prefabUIImage;
    public RectTransform target;
    public float duration;

    private void Awake()
    {
        instance = this;
    }

    public void SpawnAndAnimate(Vector3 worldPos)
    {
        Camera cam = Camera.main;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // ✅ Check if position is behind camera or outside screen bounds
        if (screenPos.z <= 0f ||
            screenPos.x < 0f || screenPos.x > Screen.width ||
            screenPos.y < 0f || screenPos.y > Screen.height)
        {
            return; // Don't spawn
        }

        // Create the image
        Image spawnedImg = Instantiate(prefabUIImage, spawnArea.transform);
        RectTransform rect = spawnedImg.rectTransform;
        rect.position = screenPos;

        rect.DOMove(target.position, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(spawnedImg.gameObject));
    }

}