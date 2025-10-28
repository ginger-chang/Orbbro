using UnityEngine;
using System.Collections;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(Greeting());
    }

    private IEnumerator Greeting()
    {
        yield return new WaitForSeconds(5);
        Debug.Log("hihi");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
