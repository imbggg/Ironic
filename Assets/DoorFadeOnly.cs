using System.Collections;
using UnityEngine;

public class DoorFadeOnly : MonoBehaviour
{
    private ScreenFader fader;
    private bool isFading = false;

    void Start()
    {
        fader = FindAnyObjectByType<ScreenFader>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isFading) return;

        if (other.CompareTag("Player"))
        {
            StartCoroutine(FadeTest());
        }
    }

    IEnumerator FadeTest()
    {
        isFading = true;

        yield return StartCoroutine(fader.FadeOut());

        yield return new WaitForSeconds(0.3f);

        yield return StartCoroutine(fader.FadeIn());

        isFading = false;
    }
}