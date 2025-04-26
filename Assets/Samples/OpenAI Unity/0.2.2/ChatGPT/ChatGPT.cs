using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using FishNet.Component.Animating;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

using System;
using Microsoft.CognitiveServices.Speech;
using FishNet.Object;
using FirstGearGames.LobbyAndWorld.Lobbies;
using FirstGearGames.LobbyAndWorld.Global.Canvases;
using FirstGearGames.LobbyAndWorld.Global;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using static ElevenlabsApi;
using UnityEngine.Networking;
using System.IO;

using NAudio.Wave;


namespace OpenAI
{
    public class ChatGPT : NetworkBehaviour
    {


        [Tooltip("Skinned Mesh Rendered target to be driven by Oculus Lipsync")]
        public SkinnedMeshRenderer skinnedMeshRenderer = null;

        [Tooltip("Blendshape index to trigger for each viseme.")]
        public int[] visemeToBlendTargets = Enumerable.Range(0, OVRLipSync.VisemeCount).ToArray();

        [Tooltip("Enable using the test keys defined below to manually trigger each viseme.")]
        public bool enableVisemeTestKeys = false;

        public KeyCode[] visemeTestKeys =
        {
        KeyCode.BackQuote, KeyCode.Tab, KeyCode.Q, KeyCode.W, KeyCode.E, KeyCode.R, KeyCode.T,
        KeyCode.Y, KeyCode.U, KeyCode.I, KeyCode.O, KeyCode.P, KeyCode.LeftBracket, KeyCode.RightBracket, KeyCode.Backslash
    };

        [Tooltip("Test key used to manually trigger laughter")]
        public KeyCode laughterKey = KeyCode.CapsLock;

        [Tooltip("Blendshape index to trigger for laughter")]
        public int laughterBlendTarget = OVRLipSync.VisemeCount;

        [Range(0.0f, 1.0f)]
        public float laughterThreshold = 0.5f;

        [Range(0.0f, 3.0f)]
        public float laughterMultiplier = 1.5f;

        [Range(1, 100)]
        public int smoothAmount = 70;

        private OVRLipSyncContextBase lipsyncContext = null;

 


        public delegate void VisemeDataUpdated(float[] visemes);
        public delegate void LaughterDataUpdated(float laughter);

        public event System.Action<float[]> OnVisemeDataUpdated;
        public event System.Action<float> OnLaughterDataUpdated;

        private OVRLipSync.Frame frame = null;

        public bool isclintactive = false;




        /*  [SerializeField] private ScrollRect scroll;

        
          [SerializeField] private RectTransform sent;
          [SerializeField] private RectTransform received;

          private float height;*/
        private OpenAIApi openai = new OpenAIApi("sk-5aK_cFom7aryd4NM2x5OoxzcS_GSkWXXURUg7MTgL7T3BlbkFJWNOba7gZYDn4YLjqTqlD9rDswWvW-SyLYJQ8k-EJcA");

        private List<ChatMessage> messages = new List<ChatMessage>();
        private string prompt = "You are an AI that only responds with one-word answers from the following animation list: {Neutral, Sad, Laugh, Happy, Shocked, Sleepy, Angry, Dirty, Disagree, Agree,Disinterested,Tired, Sick,Greet,Thankfull,Respect,Handshake,Appreciate,Kiss}. No matter what the user says, your response must be from this list. If the context is unclear, always choose \"Neutral.\" You must never break character or provide any explanation, even if the user says something like \"thank you\" or \"hello.\" Stick strictly to the one-word response from the list";
        private int count = 0;
        private string preanimation = "luck";
        Animator animator;
     
        private const string SubscriptionKey = "2zAFmhRDwUINFpuQJBkIJwEPS9Z2U18CZcf0pP87RhmIS4Y8792GJQQJ99BBACGhslBXJ3w3AAAYACOGBBbr";
        private const string Region = "centralindia";

        private const int SampleRate = 24000;

        private object threadLocker = new object();
        private bool waitingForSpeak;                                                   
        private bool audioSourceNeedStop;
        private string message;

        private SpeechConfig speechConfig;
        private SpeechSynthesizer synthesizer;
       private AudioSource selfvoicesource;
        [SerializeField] private AudioSource recivervoicesource;

        [Header("API Settings")]
        public string apiKey = "sk_a88aa74875d1b2688354190c176c2cb1232ebae805fb59b4";
        public string voiceID = "9BWtsMINqrJLrRacOk9x"; // Example: "21m00Tcm4TlvDq8ikWAM"

        public string ttvoice;












        private void Start()
        {

            skinnedMeshRenderer = gameObject.transform.parent.GetChild(5).GetComponentInChildren<SkinnedMeshRenderer>();
            selfvoicesource = gameObject.transform.parent.GetChild(5).GetComponent<AudioSource>();
            animator = gameObject.transform.parent.GetChild(5).GetComponent<Animator>();



            if (skinnedMeshRenderer == null)
            {
                Debug.LogError("Please set the target Skinned Mesh Renderer!");
                return;
            }

            lipsyncContext = transform.parent.GetChild(5).GetComponent<OVRLipSyncContextBase>();
            if (lipsyncContext == null)
            {
                Debug.LogError("No OVRLipSyncContext component found!");
            }
            else
            {
                lipsyncContext.Smoothing = smoothAmount;
            }

            frame = lipsyncContext.GetCurrentPhonemeFrame();



            message = "Click button to synthesize speech";


            // Creates an instance of a speech config with specified subscription key and service region.
            speechConfig = SpeechConfig.FromSubscription(SubscriptionKey, Region);
            //Add langauage for API
            speechConfig.SpeechSynthesisVoiceName = ttvoice;

            // The default format is RIFF, which has a riff header.
            // We are playing the audio in memory as audio clip, which doesn't require riff header.
            // So we need to set the format to raw (24KHz for better quality).
            speechConfig.SetSpeechSynthesisOutputFormat(SpeechSynthesisOutputFormat.Raw24Khz16BitMonoPcm);

            // Creates a speech synthesizer.
            // Make sure to dispose the synthesizer after use!
            synthesizer = new SpeechSynthesizer(speechConfig, null);

            synthesizer.SynthesisCanceled += (s, e) =>
            {
                var cancellation = SpeechSynthesisCancellationDetails.FromResult(e.Result);
                message = $"CANCELED:\nReason=[{cancellation.Reason}]\nErrorDetails=[{cancellation.ErrorDetails}]\nDid you update the subscription info?";
            };


          
           


        }



       


      public void StartAI(string text)
        {
          //  var speakTask = synthesizer.StartSpeakingTextAsync(text);
         //   StartCoroutine(SpeakRoutine(speakTask));
            SendReply(text);
            
        }

         public void GoSound(string text) {
            var speakTask = synthesizer.StartSpeakingTextAsync(text);
             StartCoroutine(SpeakRoutine(speakTask));
        }








        IEnumerator SpeakRoutine(Task<SpeechSynthesisResult> speakTask)
        {
            var startTime = DateTime.Now;

            while (!speakTask.IsCompleted)
            {
                yield return null;
            }

            var result = speakTask.Result;
            {
                if (result.Reason == ResultReason.SynthesizingAudioStarted)
                {
                    // Native playback is not supported on Unity yet (currently only supported on Windows/Linux Desktopl).
                    // Use the Unity API to play audio here as a short term solution.
                    // Native playback support will be added in the future release.
                    var audioDataStream = AudioDataStream.FromResult(result);
                    while (!audioDataStream.CanReadData(4092 * 2)) // audio clip requires 4096 samples before it's ready to play
                    {
                        yield return null;
                    }

                    var isFirstAudioChunk = true;
                    List<byte> audioBuffer = new List<byte>();
                    var audioClip = AudioClip.Create(
                        "Speech",
                        SampleRate * 600, // Can speak 10mins audio as maximum
                        1,
                        SampleRate,
                        true,
                        (float[] audioChunk) =>
                        {
                            var chunkSize = audioChunk.Length;
                            var audioChunkBytes = new byte[chunkSize * 2];
                            var readBytes = audioDataStream.ReadData(audioChunkBytes);

                            if (readBytes > 0)
                            {
                                audioBuffer.AddRange(audioChunkBytes.Take((int)readBytes));

                            }





                            if (isFirstAudioChunk && readBytes > 0)
                            {
                                var endTime = DateTime.Now;
                                var latency = endTime.Subtract(startTime).TotalMilliseconds;
                                message = $"Speech synthesis succeeded!\nLatency: {latency} ms.";
                                isFirstAudioChunk = false;
                            }

                            for (int i = 0; i < chunkSize; ++i)
                            {
                                if (i < readBytes / 2)
                                {
                                    audioChunk[i] = (short)(audioChunkBytes[i * 2 + 1] << 8 | audioChunkBytes[i * 2]) / 32768.0F;
                                }
                                else
                                {
                                    audioChunk[i] = 0.0f;
                                }
                            }

                            if (readBytes == 0)
                            {
                                Thread.Sleep(200); // Leave some time for the audioSource to finish playback
                                audioSourceNeedStop = true;

                                //Send Audio Over Network 

                                SendAudioOverNetwork(audioBuffer.ToArray(), frame.Visemes, frame.laughterScore);
                            }
                        });

                    selfvoicesource.clip = audioClip;
                    selfvoicesource.Play();





                }
            }
        }



        [ServerRpc]
        private void SendAudioOverNetwork(byte[] bytes, float[] visemes, float laughterScore)
        {
            PlayAudioonCLients(bytes,visemes,laughterScore);









        }

        [ObserversRpc]
        private void PlayAudioonCLients(byte[] audioData, float[] visemes, float laughterScore)
        {


            if (!IsOwner)
            {


               

                    var audioClip = AudioClip.Create("NetworkSpeech", audioData.Length / 2, 1, SampleRate, false);
                    float[] audiosamples = new float[audioData.Length / 2];

                    for (int i = 0; i < audiosamples.Length; ++i)
                    {
                        audiosamples[i] = (short)(audioData[i * 2 + 1] << 8 | audioData[i * 2]) / 32768.0F;
                    }


                    audioClip.SetData(audiosamples, 0);

                    recivervoicesource.clip = audioClip;

                    recivervoicesource.Play();


                   

                



            }






        }

        

      

        private async void SendReply(string msg)
        {


            //  Debug.Log(length);

            var newMessage = new ChatMessage()
            {
                Role = "user",
                Content = msg

            };

            //    AppendMessage(length);

            if (messages.Count == 0) newMessage.Content = prompt + "\n" + msg;

            messages.Add(newMessage);


            //  button.enabled = false;
            //  inputField.text = "";
            //  inputField.enabled = false;

            // Complete the instruction
            var completionResponse = await openai.CreateChatCompletion(new CreateChatCompletionRequest()
            {
                Model = "gpt-3.5-turbo-0125",
                Messages = messages

            });

            if (completionResponse.Choices != null && completionResponse.Choices.Count > 0)
            {
                var message = completionResponse.Choices[0].Message;

                message.Content = message.Content.Trim();
                Debug.Log(message.Content);



                messages.Add(message);
                //   var length = msg.Length;
                //  AppendMessage(length);

                if (message.Content == "Neutral" && preanimation != "isNeutral")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isNeutral", true);
                    preanimation = "isNeutral";
                }
                else if (message.Content == "Neutral" && preanimation == "isNeutral")
                {

                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isNeutral"));
                    preanimation = "isNeutral";


                }



                else if (message.Content == "Happy" && preanimation != "isHappy")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isHappy", true);
                    preanimation = "isHappy";
                }
                else if (message.Content == "Happy" && preanimation == "isHappy")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isHappy"));
                    preanimation = "isHappy";

                }
                else if (message.Content == "Laugh" && preanimation != "Islaugh")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("Islaugh", true);
                    preanimation = "Islaugh";
                }
                else if (message.Content == "Laugh" && preanimation == "Islaugh")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("Islaugh"));
                    preanimation = "Islaugh";
                }






                else if (message.Content == "Sad" && preanimation != "Issad")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("Issad", true);
                    preanimation = "Issad";
                }
                else if (message.Content == "Sad" && preanimation == "Issad")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("Issad"));
                    preanimation = "Issad";

                }
                else if (message.Content == "Dirty" && preanimation != "isCringe")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isCringe", true);
                    preanimation = "isCringe";
                }
                else if (message.Content == "Dirty" && preanimation == "isCringe")
                {
                   
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isCringe"));
                    preanimation = "isCringe";
                }
                else if (message.Content == "Shocked" && preanimation != "IsShock")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("IsShock", true);
                    preanimation = "IsShock";
                }
                else if (message.Content == "Shocked" && preanimation == "IsShock")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("IsShock"));
                    preanimation = "IsShock";


                }
                else if (message.Content == "Sleepy" && preanimation != "IsSleepy")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("IsSleepy", true);
                    preanimation = "IsSleepy";
                }
                else if (message.Content == "Sleepy" && preanimation == "IsSleepy")
                {

                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("IsSleepy"));
                    preanimation = "IsSleepy";


                }
                else if (message.Content == "Angry" && preanimation != "Isangry")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("Isangry", true);
                    preanimation = "Isangry";
                }
                else if (message.Content == "Angry" && preanimation == "Isangry")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("Isangry"));
                    preanimation = "Isangry";


                }
                else if (message.Content == "Agree" && preanimation != "isSayingYes")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isSayingYes", true);
                    preanimation = "isSayingYes";
                }
                else if (message.Content == "Agree" && preanimation == "isSayingYes")
                {

                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isSayingYes"));
                    preanimation = "isSayingYes";


                }






                else if (message.Content == "Disagree" && preanimation != "IsSayingNo")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("IsSayingNo", true);
                    preanimation = "IsSayingNo";
                }
                else if (message.Content == "Disagree" && preanimation == "IsSayingNo")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("IsSayingNo"));
                    preanimation = "IsSayingNo";

                }
               






                else if (message.Content == "Tired" && preanimation != "istired")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("istired", true);
                    preanimation = "istired";
                }
                else if (message.Content == "Tired" && preanimation == "istired")
                {

                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("istired"));
                    preanimation = "istired";

                }
                else if (message.Content == "Sick" && preanimation != "istired")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("istired", true);
                    preanimation = "istired";
                }
                else if (message.Content == "Sick" && preanimation == "istired")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("istired"));
                    preanimation = "istired";

                }
                else if (message.Content == "Greet" && preanimation != "Iswave")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("Iswave", true);
                    preanimation = "Iswave";
                }
                else if (message.Content == "Greet" && preanimation == "Iswave")
                {

                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("Iswave"));
                    preanimation = "Iswave";

                }
                else if (message.Content == "Thankfull" && preanimation != "isThank")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isThank", true);
                    preanimation = "isThank";
                }
                else if (message.Content == "Thankfull" && preanimation == "isThank")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isThank"));
                    preanimation = "isThank";

                }
                else if (message.Content == "Respect" && preanimation != "isBowing")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isBowing", true);
                    preanimation = "isBowing";
                }
                else if (message.Content == "Respect" && preanimation == "isBowing")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isBowing"));
                    preanimation = "isBowing";

                }
                else if (message.Content == "Handshake" && preanimation != "Isshakinghands")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("Isshakinghands", true);
                    preanimation = "Isshakinghands";
                }
                else if (message.Content == "Handshake" && preanimation == "Isshakinghands")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("Isshakinghands"));
                    preanimation = "Isshakinghands";

                }
                else if (message.Content == "Appreciate" && preanimation != "IsSalute")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("IsSalute", true);
                    preanimation = "IsSalute";
                }
                else if (message.Content == "Appreciate" && preanimation == "IsSalute")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("IsSalute"));
                    preanimation = "IsSalute";

                }
                else if (message.Content == "Kiss" && preanimation != "isKiss")
                {
                    animator.SetBool(preanimation, false);
                    animator.SetBool("isKiss", true);
                    preanimation = "isKiss";
                }
                else if (message.Content == "Kiss" && preanimation == "isKiss")
                {
                    animator.SetBool(preanimation, false);
                    StartCoroutine(getAnimationName("isKiss"));
                    preanimation = "isKiss";

                }
}

else
            {
                Debug.LogWarning("No text was generated from this prompt.");
            }

            //   button.enabled = true;
            //  inputField.enabled = true;
        }

        private IEnumerator getAnimationName(string v)
        {
            yield return new WaitForSecondsRealtime(1);
            animator.SetBool(v, true);
         

        }

        void Update()
        {
            if (lipsyncContext != null && skinnedMeshRenderer != null)
            {
            
                if (frame != null)
                {
                    SetVisemeToMorphTarget(frame);
                    SetLaughterToMorphTarget(frame);

                    OnVisemeDataUpdated?.Invoke(frame.Visemes);
                    OnLaughterDataUpdated?.Invoke(frame.laughterScore);
                }

                CheckForKeys();

                if (smoothAmount != lipsyncContext.Smoothing)
                {
                    lipsyncContext.Smoothing = smoothAmount;
                }
            }

            if (audioSourceNeedStop)
            {
                selfvoicesource.Stop();
                audioSourceNeedStop = false;
            }


                if(Input.GetKeyDown(KeyCode.L))
            {

                isclintactive = true;

            }


     




        }


        void CheckForKeys()
        {
            if (enableVisemeTestKeys)
            {
                for (int i = 0; i < OVRLipSync.VisemeCount; ++i)
                {
                    CheckVisemeKey(visemeTestKeys[i], i, 1);
                }
            }

            CheckLaughterKey();
        }

        void SetVisemeToMorphTarget(OVRLipSync.Frame frame)
        {

       for (int i = 0; i < visemeToBlendTargets.Length; i++)
            {
                if (visemeToBlendTargets[i] != -1)
                {
                    if (IsOwner) // Only update if we own the object
                    {
                        skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], frame.Visemes[i] * 1.0f);

                        
                          if(isclintactive == true)
                        {
                            SetVisemeDataOnNetwork(frame.Visemes);
                        }


                    
                        
                       
                    }
                }
            }
        }

        void SetLaughterToMorphTarget(OVRLipSync.Frame frame)
        {
            if (laughterBlendTarget != -1)
            {
                float laughterScore = frame.laughterScore;
                laughterScore = laughterScore < laughterThreshold ? 0.0f : laughterScore - laughterThreshold;
                laughterScore = Mathf.Min(laughterScore * laughterMultiplier, 1.0f);
                laughterScore *= 1.0f / laughterThreshold;

                if (IsOwner) // Only update if we own the object
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(laughterBlendTarget, laughterScore);


                   /* if (isclintactive == true)
                    {

                        SetLaughterDataOnNetwork(frame.laughterScore);

                    }*/

                }
            }
        }


        void CheckVisemeKey(KeyCode key, int viseme, int amount)
        {
            if (Input.GetKeyDown(key))
            {
                lipsyncContext.SetVisemeBlend(visemeToBlendTargets[viseme], amount);
            }
            if (Input.GetKeyUp(key))
            {
                lipsyncContext.SetVisemeBlend(visemeToBlendTargets[viseme], 0);
            }
        }

        void CheckLaughterKey()
        {
            if (Input.GetKeyDown(laughterKey))
            {
                lipsyncContext.SetLaughterBlend(1);
            }
            if (Input.GetKeyUp(laughterKey))
            {
                lipsyncContext.SetLaughterBlend(0);
            }
        }


        [ServerRpc]
        void SetVisemeDataOnNetwork(float[] visemes)
        {
            SetVisemeDataOnClients(visemes);

        }

        [ObserversRpc]
        private void SetVisemeDataOnClients(float[] visemes)
        {

            if (this == null || !gameObject.activeInHierarchy) return;

            if (!IsOwner)
            {
                StartCoroutine(ApplyVisemeData(visemes));
            }

          
             
            


      }

        private IEnumerator ApplyVisemeData(float[] visemes)
        {

            yield return new WaitForSecondsRealtime(3); 
            for (int i = 0; i < visemeToBlendTargets.Length; i++)
            {
                if (visemeToBlendTargets[i] != -1)
                {
                    skinnedMeshRenderer.SetBlendShapeWeight(visemeToBlendTargets[i], visemes[i] * 1.0f);
                }
            }
        }

        [ServerRpc]
        void SetLaughterDataOnNetwork(float laughter)
        {
         SetLaughterDataOnClients(laughter);

        }


       [ObserversRpc]
       private void SetLaughterDataOnClients(float laughter)
        {
         
            if(this == null || !gameObject.activeInHierarchy) return;
              

            if (!IsOwner)
            {

                StartCoroutine(SetLaughterherenow(laughter));
               
            }



        }

        private IEnumerator SetLaughterherenow(float laughter)
        {
            yield return new WaitForSecondsRealtime(3);
            skinnedMeshRenderer.SetBlendShapeWeight(laughterBlendTarget, laughter);
        }

        void OnDestroy()
        {
            if (synthesizer != null)
            {
                synthesizer.Dispose();
            }
        }


        public void GetAudio(string text)
        {
          //  StartCoroutine(SendTTSRequestAndStream(text));
        }

        /*IEnumerator SendTTSRequest(string text)
        {
            string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

            // Create request body
            TTSRequestBody requestBody = new TTSRequestBody
            {
                text = text,
                model_id = "eleven_multilingual_v2",
                voice_settings = new VoiceSettings
                {
                    stability = 0.5f,
                    similarity_boost = 0.5f
                }
            };

            string jsonBody = JsonUtility.ToJson(requestBody);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("xi-api-key", apiKey);
                webRequest.SetRequestHeader("accept", "audio/mpeg");


                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    AudioClip clip = DownloadHandlerAudioClip.GetContent(webRequest);
                    selfvoicesource.clip = clip;
                    selfvoicesource.Play();
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    
                }


            }
        }*/


        /*IEnumerator SendTTSRequestAndStream(string text)
        {
            string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceID}";

            TTSRequestBody requestBody = new TTSRequestBody
            {
                text = text,
                model_id = "eleven_multilingual_v2",
                voice_settings = new VoiceSettings
                {
                    stability = 0.5f,
                    similarity_boost = 0.5f
                }
            };

            string jsonBody = JsonUtility.ToJson(requestBody);

            using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                // Use DownloadHandlerBuffer to get the raw bytes
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.SetRequestHeader("xi-api-key", apiKey);
                webRequest.SetRequestHeader("accept", "audio/mpeg"); // Or audio/mp3, check ElevenLabs docs

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // Get the entire response as a byte array
                    byte[] audioBytes = webRequest.downloadHandler.data;
                    StartCoroutine(StreamAudio(audioBytes));
                }
                else
                {
                    Debug.LogError($"Error: {webRequest.error}");
                    
                    GoSound(text);

                }
            }
        }

       IEnumerator StreamAudio(byte[] fullAudioData)
        {
            var startTime = DateTime.Now;
            bool isFirstAudioChunk = true;
            List<byte> audioBuffer = new List<byte>();
            int chunkSize = 4096 * 2; // Target chunk size in *bytes* (for the converted PCM data)

            AudioClip audioClip = null;
            // Use NAudio to read the MP3 data
            using (MemoryStream mp3Stream = new MemoryStream(fullAudioData))
            using (Mp3FileReader mp3FileReader = new Mp3FileReader(mp3Stream))
            using (WaveStream pcmStream = WaveFormatConversionStream.CreatePcmStream(mp3FileReader)) // Convert to PCM
            {
                 audioClip = AudioClip.Create(
                    "Speech",
                    SampleRate * 600,  // Maximum length
                    pcmStream.WaveFormat.Channels, // Get channels from the PCM stream
                    pcmStream.WaveFormat.SampleRate, // Get sample rate from the PCM stream
                    true, // Streaming
                    (float[] audioChunk) =>
                    {
                        int bytesToRead = Mathf.Min(chunkSize, (int)(pcmStream.Length - pcmStream.Position));
                        byte[] buffer = new byte[bytesToRead];
                        int bytesRead = pcmStream.Read(buffer, 0, bytesToRead);

                        if (bytesRead > 0)
                        {

                            audioBuffer.AddRange(buffer);

                            // Convert the byte chunk to float samples
                            for (int i = 0; i < audioChunk.Length; i++)
                            {
                                if (i * 2 + 1 < bytesRead)
                                {
                                    // Convert the 16-bit PCM sample to a float
                                    short sample = (short)((buffer[i * 2 + 1] << 8) | buffer[i * 2]);
                                    audioChunk[i] = sample / 32768.0f;
                                }
                                else
                                {
                                    audioChunk[i] = 0.0f; // Fill with silence
                                }
                            }

                            if (isFirstAudioChunk)
                            {
                                var endTime = DateTime.Now;
                                var latency = endTime.Subtract(startTime).TotalMilliseconds;
                                Debug.Log($"ElevenLabs synthesis + decode latency: {latency} ms");
                                isFirstAudioChunk = false;
                            }
                        }


                        //Check if we've reached the end of the data
                        if (pcmStream.Position >= pcmStream.Length)
                        {
                            // Send last of audioBuffer
                            SendAudioOverNetworks(audioBuffer.ToArray(), frame.Visemes, frame.laughterScore); // Assuming you have this frame data
                            audioSourceNeedStop = true;
                        }

                    });

                selfvoicesource.clip = audioClip;
                selfvoicesource.Play();

                // Keep the coroutine alive
               while (selfvoicesource.isPlaying)
                {
                    if (audioSourceNeedStop && !selfvoicesource.isPlaying) // make audio source stop playing
                    {
                        selfvoicesource.Stop();
                        break; // Exit
                    }
                    yield return null;
                }
            }
            //Clean up
            audioClip = null;
            Resources.UnloadUnusedAssets(); // important to release resources.
        }

        [ServerRpc]
        private void SendAudioOverNetworks(byte[] bytes, float[] visemes, float laughterScore)
        {
            PlayAudioonCLientss(bytes, visemes, laughterScore);
        }


        [ObserversRpc]
        private void PlayAudioonCLientss(byte[] audioData, float[] visemes, float laughterScore)
        {
            if (!IsOwner)
            {
                //Client recives audio here

                StartCoroutine(PlayReceivedAudio(audioData));
            }
        }

        IEnumerator PlayReceivedAudio(byte[] audioData)
        {

            var audioClip = AudioClip.Create("NetworkSpeech", audioData.Length / 2, 1, SampleRate, false); // Not streaming on receiver
            float[] audiosamples = new float[audioData.Length / 2];

            for (int i = 0; i < audiosamples.Length; ++i)
            {
                audiosamples[i] = (short)(audioData[i * 2 + 1] << 8 | audioData[i * 2]) / 32768.0F;
            }


            audioClip.SetData(audiosamples, 0);
            recivervoicesource.clip = audioClip;
            recivervoicesource.Play();

            // Keep the coroutine alive while audio plays
            while (recivervoicesource.isPlaying)
            {

                yield return null;
            }

            // Cleanup
            audioClip = null; // Clear reference
            Resources.UnloadUnusedAssets();

        }















        [System.Serializable]
        public class TTSRequestBody
        {
            public string text;
            public string model_id;
            public VoiceSettings voice_settings;
        }

        [System.Serializable]
        public class VoiceSettings
        {
            public float stability;
            public float similarity_boost;
        }
*/






    }






}
