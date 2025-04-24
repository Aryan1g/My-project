using FirstGearGames.LobbyAndWorld.Global.Canvases;
using FirstGearGames.LobbyAndWorld.Global;
using FishNet;
using FishNet.Component.Animating;
using FishNet.Object;
using UnityEngine;

public class Animatorfinder : NetworkBehaviour
{

    private Animator animator;
    private NetworkAnimator networkAnimator;




    void Start()
    {
        animator = transform.GetComponent<Animator>();
        networkAnimator = transform.parent.GetComponent<NetworkAnimator>();
        networkAnimator.SetAnimator(animator);
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {

            if ((base.IsOwner))
            {
                animator.SetBool("Islaugh", true);
            }

                 
                    

                
             }  else if (Input.GetKeyDown(KeyCode.M)){

            if ((base.IsOwner))
            {
                animator.SetBool("Islaugh", false);
            }

        }


    }
}
