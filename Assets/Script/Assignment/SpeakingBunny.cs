using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class SpeakingBunny : MonoBehaviour
{

    [SerializeField] GameObject[] goText;
    [SerializeField] int iRandomWaitInt;
    [SerializeField] int iRandomTextInt;

    private void Start()
    {
        StartCoroutine(SwapText());
    }

    IEnumerator SwapText()
    {
        iRandomWaitInt = Random.Range(2, 5);
        iRandomTextInt = Random.Range(0, 2); //Making sure I can get the value 0 or 1

        goText[iRandomTextInt].SetActive(true);

        yield return new WaitForSeconds(3);         //See the text for 3 seconds.
        goText[iRandomTextInt].SetActive(false);

        yield return new WaitForSeconds(iRandomWaitInt); //Wait before saying something again.
        StartCoroutine (SwapText());
    }
}
