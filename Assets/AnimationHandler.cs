using OpenAI;
using UnityEngine;

public class AnimationHandler : MonoBehaviour
{
   
   

    void Start()
    {
      
    



    }


     void Update()
    {

        if (Input.GetKeyDown(KeyCode.Y))
        {
            transform.parent.GetChild(3).GetComponent<ChatGPT>().GoSound("yaha aana hai to jaldi aao mai yaha pe hu , aur mushe bacha lo , vrna yaha pe mushe mar denge");
        }




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
