using FishNet.Object;
using GameKit.Dependencies.Utilities;
using OpenAI;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraController : NetworkBehaviour
{


    private void Update()
    {


        if (Input.GetKeyDown(KeyCode.A))
        {
            var postion = new Vector3(-2f, 1f, 90f);
            gameObject.transform.position = postion;


        }


        if (Input.GetKeyDown(KeyCode.Z))
        {
            var postion = new Vector3(-2f, 1f, 0f);
            gameObject.transform.position = postion;
        }


        if (Input.GetKeyDown(KeyCode.H))
        {
            OutSideOffAnimation();


        }



        if (Input.GetKeyDown(KeyCode.G))
        {

            InsideAnimation();



        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            var avatar1 = GameObject.Find("67f3ba669dc08cf26d294a6c");
            avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>().isclintactive = true;



        }


        if (Input.GetKeyDown(KeyCode.V))
        {
            var avatar1 = GameObject.Find("67f3ba2441caa74c6f98f226");
            avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>().isclintactive = true;



        }


        /* if (Input.GetKeyDown(KeyCode.A))
         {
             var postion = new Vector3(-2f, 1f, 90f);
             gameObject.transform.position = postion;


         }


         if (Input.GetKeyDown(KeyCode.Z))
         {
             var postion = new Vector3(-2f, 1f, 0f);
             gameObject.transform.position = postion;
         }






         if (Input.GetKeyDown(KeyCode.H))
         {
             OutSideOffAnimation();


         }



         if (Input.GetKeyDown(KeyCode.G))
         {

             InsideAnimation();



         }


         if (Input.GetKeyDown(KeyCode.F))
         {
             var avatar1 = GameObject.Find("65a7a6388d5515a6083f99ae");
             avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>().isclintactive = true;



         }


         if (Input.GetKeyDown(KeyCode.V))
         {
             var avatar1 = GameObject.Find("675fd86be1dbcc33e6c4acc3");
             avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>().isclintactive = true;



         }








     }


     private void InsideAnimation()
     {
         var avatar1 = GameObject.Find("65a7a6388d5515a6083f99ae");
         var chatgpt = avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>();
         chatgpt.StartAI("Hi I am vishal what about you");
     }



     private void OutSideOffAnimation()
     {
         var avatar1 = GameObject.Find("675fd86be1dbcc33e6c4acc3");
         var chatgpt = avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>();
         chatgpt.StartAI("Hi I am vishal what about you");

     }*/

    }


    private void InsideAnimation()
    {
        var avatar1 = GameObject.Find("67f3ba669dc08cf26d294a6c");
        var chatgpt = avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>();
        chatgpt.StartAI("Hi I am vishal what about you");
    }



    private void OutSideOffAnimation()
    {
        var avatar1 = GameObject.Find("67f3ba2441caa74c6f98f226");
        var chatgpt = avatar1.transform.parent.GetChild(3).GetComponent<ChatGPT>();
        chatgpt.StartAI("Hi I am vishal what about you");

    }


    void FlipCamera(string fake)
    {

        var postion = new Vector3(-2f, 1f, 90f);
        gameObject.transform.position = postion;




    }

      void FlipMe(string fake)
    {
        var postion = new Vector3(-2f, 1f, 0f);
        gameObject.transform.position = postion;


    }







}
