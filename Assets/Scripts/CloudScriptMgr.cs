using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine.SceneManagement;

public class CloudScriptMgr : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Msg;
    [SerializeField] TMP_InputField inputtext;

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text += msg + '\n';
    }

    void OnExeSucc(ExecuteCloudScriptResult r)
    {
        UpdateMsg("Response fr server: " + r.FunctionResult.ToString());
    }

    void OnError(PlayFabError e)
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void GotoScene(string scenename)
    {
        SceneManager.LoadScene(scenename, LoadSceneMode.Single);
    }

    public void ExeCloudScript()
    {
        var req = new ExecuteCloudScriptRequest
        {
            FunctionName = "cldfn1",
            FunctionParameter = new
            {
                name = inputtext.text
            }
        };
        try
        {
            PlayFabClientAPI.ExecuteCloudScript(req, OnExeSucc, OnError);
        }catch(PlayFabException e)
        {
            UpdateMsg(e.ToString());
        }
    }

    public void ExeCloudAddMoney()
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AddMoney",
            FunctionParameter = new {},
            GeneratePlayStreamEvent = true
        },
        OnExeSucc, OnError);
    }
}
