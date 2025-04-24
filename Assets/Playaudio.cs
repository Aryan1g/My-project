using Newtonsoft.Json;
using OpenAI;
using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Playaudio : MonoBehaviour
{
    //  public UnityEngine.UI.Button sendButton;
    //  public InputField nputField;
    //  public ChatGPT tts;
    public ChatGPT tts;
    
    private const string ApiUrl = "https://api.openai.com/v1/chat/completions"; // OpenAI API endpoint
    private const string ApiKey = "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"; // Replace with your OpenAI API key



    void Start()
    {

        /*sendButton.onClick.AddListener(() =>
        {

            ConvertHinglishToHindiAsync(nputField.text);
            nputField.text = "";
        });*/
    }



    public void Convertext(string text){
                       ConvertHinglishToHindiAsync(text);
        }

    private async void ConvertHinglishToHindiAsync(string hinglishInput)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);

            // Construct the JSON payload
            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {


                    new { role = "system", content = "You are a Hinglish Normalizer. Convert Hinglish text into clear and readable Hindi text while keeping it conversational and informal, as spoken in everyday Hindi. Avoid adding extra words like \"हूँ\" or \"है\" unless they are explicitly written in the input.Do not replace words with their formal alternatives unless absolutely necessary. Preserve the original tone and informal words as they are. Examples:\n1. wo pro player hai → वो प्रो प्लेयर है\n2. tushe pata  → तुझे पता \n3. tm kaha ho → तुम कहाँ हो\n4. bhot bhot congratulation tumko → बहुत बहुत बधाई तुमको \n5.are bhai → अरे भाई"},


                  new { role = "user", content = hinglishInput }
                },
                temperature = 0.7
            };

            // Serialize using Newtonsoft.Json
            string jsonBody = JsonConvert.SerializeObject(requestBody);

            HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(ApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var openAIResponse = JsonConvert.DeserializeObject<OpenAIResponse>(responseBody);
                string hindiText = openAIResponse.choices[0].message.content;

                tts.GoSound(hindiText);
                Debug.Log("Converted Text: " + hindiText);
            }
            else
            {
                string errorResponse = await response.Content.ReadAsStringAsync();

                Debug.LogError("API Error: " + response.StatusCode + " - " + errorResponse);
               

            }
        }
    }
}


public class OpenAIResponse
{
    public Choice[] choices { get; set; }
}

public class Choice
{
    public Message message { get; set; }
}

public class Message
{
    public string role { get; set; }
    public string content { get; set; }
}

  

