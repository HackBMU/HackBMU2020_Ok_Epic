using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ParallelDots;
using Newtonsoft.Json.Linq;


public class nlpapitime : MonoBehaviour
{
     public Dictionary<string, string> emotionalsentences;

    string SpeechText = "I stand here today humbled by the task before us, grateful for the trust you have bestowed, mindful of the sacrifices borne by our ancestors. I thank President Bush for his service to our nation, as well as the generosity and cooperation he has shown throughout this transition.Forty-four Americans have now taken the presidential oath.The words have been spoken during rising tides of prosperity and the still waters of peace.Yet, every so often the oath is taken amidst gathering clouds and raging storms.At these moments, America has carried on not simply because of the skill or vision of those in high office, but because We the People have remained faithful to the ideals of our forbearers, and true to our founding documents";
    paralleldots pd = new paralleldots("TZXtKoVbRO6L7iEfUEHaUeuNgeeF7dHaQ7BN5NKqeKc");
    JArray bruh = JArray.Parse("[\"drugs are fun\", \"don\'t do drugs, stay in school\", \"lol you a fag son\", \"I have a throat infection\"]");

    Dictionary<string,string> organizespeech(string texty)
    {
        List<string> sentences = new List<string>();
        List<string> emotionsfinal = new List<string>();
        Dictionary<string,string> finalarri = new Dictionary<string, string>();

        //string[] spearator = { "."};
        string[] strlist = texty.Split('.');
        foreach (string s in strlist)
        {
            sentences.Add(s);
        }
        foreach (string i in sentences)
        {

            string emo = pd.emotion(i);
            
            JObject jsonObj = JObject.Parse(emo);
            Dictionary<string, object> emodict = jsonObj.ToObject<Dictionary<string, object>>();

           Dictionary<string, float> finalemodict = JObject.FromObject(emodict["emotion"]).ToObject<Dictionary<string, float>>();
            var bestemo = finalemodict.FirstOrDefault(x => x.Value == finalemodict.Values.Max()).Key;
            emotionsfinal.Add(bestemo);
            // print(finalemodict["Happy"]);

        }
        for(int i=0;i<emotionsfinal.Count();i++)
        {
            finalarri.Add(sentences[i], emotionsfinal[i]);
       
        }
        //print(finalarri.Count);

        return (finalarri);
    }

   
    // Start is called before the first frame update
    void Start()
    {//string emotion_batch = pd.emotion_batch(bruh);
     //  print("emotion batch");
     //print(emotion_batch);
        emotionalsentences = organizespeech(SpeechText);
        //print(emotionalsentences);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
