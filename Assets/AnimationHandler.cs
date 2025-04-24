using OpenAI;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
   
   

    void Start()
    {
      
    



    }


     void Update()
    {
       





    }

      void doChats(string v)
    {

         


        transform.parent.GetChild(2).GetComponent<Playaudio>().Convertext(v);
        transform.parent.GetChild(3).GetComponent<ChatGPT>().StartAI(v);
        

    }


       void SetClientActive(string fake)
    {
        transform.parent.GetChild(3).GetComponent<ChatGPT>().isclintactive = true;
    }



}
