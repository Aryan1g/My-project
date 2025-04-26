using FishNet.Component.Animating;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using GLTFast.Schema;
using OpenAI;
using ReadyPlayerMe.Core;
using System;
using System.Collections;

using UnityEngine;

public class AvatarAssigner : NetworkBehaviour
{

    [SerializeField]
    private string avatarurls;
    [SerializeField]
    private GameObject parentRef;

    [SerializeField]
    private Transform avatarspawnpostion;

    [SerializeField]
    private string faceMapperAssetName = "Facemap";


    private readonly SyncVar<string> avatarurl_nw = new SyncVar<string>();

    private GameObject avatar;

    private bool avatarloaded;

    private const string PARENT = "ParentRef";



    private void Start()
    {
        avatarloaded = false;
    }


    private void FixedUpdate()
    {
        if (avatarloaded) return;

        if (avatarurls == null) return;

        if (!GetAvatarUrl().Equals(avatarurls))
        {
            avatarurls = GetAvatarUrl();
        }

        StartCoroutine(LoadAvatarAsync(GetAvatarUrl()));

       avatarloaded = true;
    }



    public void SetAvatarUrl(string _avatarUrl)
    {
        avatarurls = _avatarUrl;
        avatarurl_nw.Value = _avatarUrl;
    }

    private string GetAvatarUrl()
    {
        return avatarurl_nw.Value;
    }




    private  IEnumerator LoadAvatarAsync(string v)
    {

         


        var avatarLoader = new AvatarObjectLoader();

        avatarLoader.OnCompleted += (_, args) =>
        {
            avatar = args.Avatar;


            if (args.Metadata.OutfitGender == OutfitGender.Masculine)
            {
                var animator = avatar.GetComponent<Animator>();
                animator.runtimeAnimatorController = transform.GetComponent<Animator>().runtimeAnimatorController;
                var networkaniamtor = transform.GetComponent<NetworkAnimator>();
                networkaniamtor.SetAnimator(animator);
                avatar.AddComponent<OVRLipSyncContext>();
                avatar.AddComponent<AnimationHandler>();
              //  avatar.AddComponent<FaceActor>();
                var gpt = transform.GetComponentInChildren<ChatGPT>();
                gpt.enabled = true;
                gpt.ttvoice = "hi-IN-AaravNeural";
                gpt.isclintactive = true;
              
                avatar.transform.localScale = new Vector3(0.95f, 0.95f, 0.95f);

                 
               


            }
            else
            {
                var animator = avatar.GetComponent<Animator>();
                animator.runtimeAnimatorController = transform.GetComponent<Animator>().runtimeAnimatorController;
                var networkaniamtor = transform.GetComponent<NetworkAnimator>();
                networkaniamtor.SetAnimator(animator);
                avatar.AddComponent<OVRLipSyncContext>();
                avatar.AddComponent<AnimationHandler>();
                var gpt = transform.GetComponentInChildren<ChatGPT>();
                gpt.enabled = true;
                gpt.ttvoice = "hi-IN-AartiNeural";
                gpt.isclintactive = true;







            }


         
                  if(parentRef.name == v)
            {
                avatar.transform.parent = transform;
                avatar.transform.SetPositionAndRotation(avatarspawnpostion.position, avatarspawnpostion.rotation);
               


            }
               
               

            


            parentRef.name = PARENT;




        };


        avatarLoader.LoadAvatar(v);
         parentRef.name = v;
     
        yield return new WaitUntil(() => !avatarloaded);






    }

   

    private void OnDestroy()
    {
       

        StopAllCoroutines();
        if (avatar != null)
        {

            if (Application.isPlaying)
            {
                Destroy(avatar);



            }   else{
                DestroyImmediate(avatar);

            }







           
        }

    }


}
