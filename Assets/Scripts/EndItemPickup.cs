using UnityEngine;
using UnityEngine.SceneManagement;

public class EndItemPickup : MonoBehaviour
{
    [SerializeField] private string endSceneName = "EndScene";
    [SerializeField] private float delayBeforeLoad = 0.5f;

    private bool picked;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (picked) return;

        if (other.CompareTag("Player"))
        {
            picked = true;
            StartCoroutine(EndSequence());
        }
    }

    private System.Collections.IEnumerator EndSequence()
    {

        yield return new WaitForSeconds(delayBeforeLoad);

        SceneManager.LoadScene(endSceneName);
    }
}
