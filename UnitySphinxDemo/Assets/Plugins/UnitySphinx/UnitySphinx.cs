using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;


public enum SearchModel { jsgf = 1, kws = 0 }

public enum AudioDevice { Mic = 0, File = 1 }


public class UnitySphinx : MonoBehaviour {

    static AudioDevice audioDevice = AudioDevice.Mic;

    static string root = "/Plugins/UnitySphinx/Resources/UnitySphinx/model/en-us/";
    static UnitySphinx instance;
    static Coroutine recognize;


    public static SearchModel Model {
        get {
            var str = new StringBuilder(10);
            var len = str.Capacity;
            IsError(SphinxPlugin.Get_Search_Model(str,len));
            return searchModel;
        }
        set {
            Stop();
            //IsError(SphinxPlugin.Set_Search_Model((int) value));
            searchModel = value;
            Init();
            Run();
        }
    }
    static SearchModel searchModel;



    public static bool IsInitialized {
        get {
            if (SphinxPlugin.Is_Recognizer_Enabled()!=((isInitialized)?1:0))
                throw new Exception(
                    "DLL and UnitySphinx somehow got out of sync. This should be impossible.");
            return isInitialized;
        }
        private set { if (instance && !IsRunning) isInitialized = value; }
    } static bool isInitialized;


    public static bool IsRunning {
        get { return isRunning; }
        private set {
            if (isInitialized) isRunning = value;
            else Debug.LogWarning("Attempting to run a recognizer that has not been initialized. Aborted");
        }
    } static bool isRunning;


    public static bool IsUtteranceStarted {
        get { return SphinxPlugin.Is_utt_started()==1; } }


    public static int QueueLength {
        get { return SphinxPlugin.Get_Queue_Length(); } }


    void Awake() {
        if (instance) Destroy(gameObject);
        instance = this;
        root = Application.dataPath+root;
    }


    public static void Run() { IsRunning = true; }

    public static void Pause() { IsRunning = false; }


    static bool IsError(int code) {
        switch (code) {
            default:
                Debug.LogError("Unrecognized Error Code");
                goto case -1;
            case 0: return false;
            case -1: throw new Exception("Pocketsphinx recognizer object failed to initialize.");
            case -20:
                Debug.LogError("Config failed to initialize properly.");
                goto case -1;
            case -21:
                Debug.LogError("Check that all your dictionary, grammar, and acoustic model paths are correct.");
                goto case -1;
            case -31:
                Debug.LogError("Failed to open mic device.");
                goto case -1;
            case -32:
                Debug.LogError("Failed to start recording through mic.");
                goto case -1;
            case -33:
                Debug.LogError("Failed to start utterance.");
                goto case -1;
            case -60:
                Debug.LogWarning("Pocketsphinx failed to reinitialize while setting kws as search mode.");
                goto case -1;
            case -61:
                Debug.LogWarning("Pocketsphinx recognizer object was not initialized properly. Failed to set kws file.");
                goto case -1;
            case -62:
                Debug.LogWarning("Failed to set kws file. Ensure the path is valid.");
                goto case -1;
            case -70:
                Debug.LogWarning("Pocketsphinx failed to reinitialize while setting jsgf as search mode.");
                goto case -1;
            case -71:
                Debug.LogWarning("Pocketsphinx recognizer object was not initialized properly. Failed to set jsgf file.");
                goto case -1;
            case -72:
                Debug.LogWarning("Failed to set jsgf file. Ensure the path is valid.");
                goto case -1;
            case -81:
                Debug.LogWarning("Pocketsphinx recognizer object was not initialized properly.");
                goto case -1;
            case -82:
                Debug.LogWarning("The jsgf file was not configured properly.");
                goto case -1;
            case -83:
                Debug.LogWarning("The kws file was not configured properly.");
                goto case -1;
            case -91:
                Debug.LogWarning("Pocketsphinx recognizer object was not initialized properly.");
                goto case -1;
            case -92:
                Debug.LogWarning("Pocketsphinx has no search model enabled.");
                goto case -1;
        }
    }


    public static void Init() { Init(
        audioDevice: audioDevice,
        searchModel: SearchModel.kws,
        hmm: root+"en-us",
        lm: root+"en-us.lm.bin",
        dict: root+"cmudict-en-us.dict",
        jsgf: root+"animals.gram",
        kws: root+"keyphrase.list"); }


    public static void Init(
                    AudioDevice audioDevice,
                    SearchModel searchModel,
                    string hmm,
                    string lm,
                    string dict,
                    string jsgf,
                    string kws) {
        if (IsError(SphinxPlugin.Init_Recognizer(
            (int) audioDevice,
            (int) searchModel,
            new StringBuilder(hmm),
            new StringBuilder(lm),
            new StringBuilder(dict),
            new StringBuilder(jsgf),
            new StringBuilder(kws)))) return;

        IsInitialized = true;
        recognize = instance.StartCoroutine(instance.Recognize());
    }


    public static void Stop() {
        SphinxPlugin.Free_Recognizer();
        instance.StopCoroutine(recognize);
        IsInitialized = false;
        IsRunning = false;
    }


    IEnumerator Recognize() {
        while (true) {
            yield return new WaitForSeconds(0.1f);
            if (IsRunning) SphinxPlugin.Recognize_Mic();
        }
    }


    public static string DequeueString() {
        var strlen = SphinxPlugin.Request_Buffer_Length();
        var str = new StringBuilder(strlen);
        if (strlen>0) SphinxPlugin.Dequeue_String(str,strlen);
        return str.ToString();
    }


    public static void SetjsgfPath(string path) {
        Stop();
        IsError(SphinxPlugin.Set_jsgf(new StringBuilder(path)));
        recognize = instance.StartCoroutine(instance.Recognize());
        Model = SearchModel.jsgf;
        Init();
        Run();
    }


    public static void SetkwsPath(string path) {
        Stop();
        var str = new StringBuilder(path);
        IsError(SphinxPlugin.Set_kws(str));
        recognize = instance.StartCoroutine(instance.Recognize());
        Model = SearchModel.kws;
        Init();
        Run();
    }

    void OnApplicationQuit() { if (IsInitialized) Stop(); }
}



