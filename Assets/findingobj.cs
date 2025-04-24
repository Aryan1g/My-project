using OpenAI;
using Unity.VisualScripting;
using UnityEngine;

public class findingobj : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

   public void EnableNew()
    {
        GetComponent<LipSyncNetworkBridge>().enabled = true;
        GetComponent<ChatGPT>().enabled = true;
    }




}
