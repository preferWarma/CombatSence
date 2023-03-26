using System.Collections;
using DG.Tweening;
using UnityEngine;

public class AttackSense : MonoBehaviour
{
    private static AttackSense _instance;
    public static AttackSense Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<AttackSense>();
            return _instance;
        }
    }
    private bool isShake;

    public void HitPause(int duration)
    {
        StartCoroutine(Pause(duration));
    }

    private IEnumerator Pause(int duration)
    {
        var pauseTime = duration / 60f;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(pauseTime);
        Time.timeScale = 1;
    }

    public void CameraShake(float duration, float strength)
    {
        if (!isShake)
        {
            StartCoroutine(Shake(duration, strength));
        }
    }

    private IEnumerator Shake(float duration, float strength)
    {
        isShake = true;
        var cameraTransform = Camera.main.transform;
        var startPosition = cameraTransform.position;

        while (duration > 0)
        {
            cameraTransform.position = Random.insideUnitSphere * strength + startPosition;
            duration -= Time.deltaTime;
            yield return null;
        }
        cameraTransform.position = startPosition;
        isShake = false;
    }
}
