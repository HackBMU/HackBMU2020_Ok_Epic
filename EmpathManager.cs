using System;
using System.IO;
using System.Threading;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class EmpathManager : MonoBehaviour
{
    [SerializeField]
    private string m_subscriptionKey = string.Empty;

    [SerializeField]
    int i = 0;
    public Text rightpanel;
    public Text leftpanel;

    private Text empathResult;
    public Text screentext;
    private int count = 0;
    private AudioClip micClip;
    private float[] microphoneBuffer;
    private bool flag = true;
    private int head;
    private int position;
    private bool isRecording;
    public GameObject scriptcont;
    public int maxRecordingTime;
    private const int samplingFrequency = 11025;
    private string micDeviceName;
    Dictionary<string, string> nlpresult;
    const int HEADER_SIZE = 44;
    const float rescaleFactor = 32767; //to convert float to Int16

    public void Start()
    {
        nlpresult = scriptcont.GetComponent<nlpapitime>().emotionalsentences;
       // print(nlpresult.Keys.ElementAt(1));

        micDeviceName = Microphone.devices[2];
        foreach (var device in Microphone.devices)
        {
            Debug.Log("Name: " + device);
        }
    }

    public void ButtonClicked()
    {
        rightpanel.text = "";
        leftpanel.text = "";
        if (!isRecording)
        {
            StartCoroutine(RecordingForEmpathAPI());
        }


    }

    public IEnumerator RecordingForEmpathAPI()
    {
        print(nlpresult.Keys.ElementAt(i));
        screentext.text = nlpresult.Keys.ElementAt(i);
        RecordingStart();
        yield return new WaitForSeconds(maxRecordingTime);

        RecordingStop();

        yield return null;

    }

    public void RecordingStart()
    {
        StartCoroutine(WavRecording(micDeviceName, maxRecordingTime, samplingFrequency));
    }

    public void RecordingStop()
    {
        isRecording = false;
        position = Microphone.GetPosition(null);
        Microphone.End(micDeviceName);
        Debug.Log("Recording end");
        byte[] empathByte = WavUtility.FromAudioClip(micClip);
        StartCoroutine(Upload(empathByte));
    }

    IEnumerator Upload(byte[] wavbyte)
    {
        WWWForm form = new WWWForm();
        form.AddField("apikey", m_subscriptionKey);
        form.AddBinaryData("wav", wavbyte);
        string receivedJson = null;

        using (UnityWebRequest www = UnityWebRequest.Post("https://api.webempath.net/v2/analyzeWav", form))         {             yield return www.SendWebRequest();              if (www.isNetworkError || www.isHttpError)             {                 Debug.Log(www.error);             }             else             {                 receivedJson = www.downloadHandler.text;
                //  Debug.Log(receivedJson);
            }         }          EmpathData empathData = ConvertEmpathToJson(receivedJson);         empathResult.text = ConvertEmpathDataToString(empathData);

    }

    public IEnumerator WavRecording(string micDeviceName, int maxRecordingTime, int samplingFrequency)
    {
        Debug.Log("Recording start");
        //Recording開始
        isRecording = true;

        //Buffer
        microphoneBuffer = new float[maxRecordingTime * samplingFrequency];
        //録音開始
        micClip = Microphone.Start(deviceName: micDeviceName, loop: false,
                                   lengthSec: maxRecordingTime, frequency: samplingFrequency);
        yield return null;
    }

    public EmpathData ConvertEmpathToJson(string json)
    {
        Debug.AssertFormat(!string.IsNullOrEmpty(json), "Jsonの取得に失敗しています。");

        EmpathData empathData = null;

        try
        {
            empathData = JsonUtility.FromJson<EmpathData>(json);
        }
        catch (System.Exception i_exception)
        {
            Debug.LogWarningFormat("Jsonをクラスへ変換することに失敗しました。exception={0}", i_exception);
            empathData = null;
        }
        return empathData;
    }

    public string ConvertEmpathDataToString(EmpathData empathData)
    {
        string result;
        if (empathData.error == 0)
        {
            int calm = empathData.calm;
            int anger = empathData.anger;
            int joy = empathData.joy;
            int sorrow = empathData.sorrow;
            int energy = empathData.energy;
            Dictionary<string, int> finalemo = new Dictionary<string, int>() { { "Happy", joy }, { "Sad", sorrow }, { "Excited", energy }, { "Angry", anger }, { "Fear", 1 - calm }, { "Bored", 1 - energy } };
            string nlpemotion = nlpresult.Values.ElementAt(i);
            string bestemo = finalemo.FirstOrDefault(x => x.Value == finalemo.Values.Max()).Key;
            rightpanel.text = bestemo;
            leftpanel.text = nlpemotion;
            result = "Emotion of line:" + i + " ,actual emotion : " + bestemo + ",expected emotion " + nlpemotion;
            i = i + 1;

        }
        else
        {
            int error = empathData.error;
            string msg = empathData.msg;
            result = "error : " + error +
                     "\nmsg : " + msg;
        }
        print(result);
        return result;
    }
}
