using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayerboxManager : MonoBehaviour
{
    [SerializeField] Playerbox[] PlayerBoxes;
    public void SendJSON()
    {
        List<Player> playerList = new List<Player>();
        foreach (var item in PlayerBoxes) playerList.Add(item.ReturnClass());
        string stringListAsJson = JsonUtility.ToJson(new JSListWrapper<Player>(playerList));
        Debug.Log("JSON data prepared:" + stringListAsJson);
        var req = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                {"Player", stringListAsJson }
            }
        };
        PlayFabClientAPI.UpdateUserData(req, result => Debug.Log("Data sent success"), OnError);
    }

    public void LoadJSON()
    {
        
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnJSONDataReceived, OnError);
    }

    void OnJSONDataReceived(GetUserDataResult r)
    {
        Debug.Log("received JSON data");
        if (r.Data != null && r.Data.ContainsKey("Player"))
        {
            Debug.Log(r.Data["Player"].Value);
            JSListWrapper<Player> jlw = JsonUtility.FromJson<JSListWrapper<Player>>(r.Data["Player"].Value);
            for (int i = 0; i < PlayerBoxes.Length; i++)
            {
                PlayerBoxes[i].SetUI(jlw.list[i]);
            }
            
        }
    }

    void OnError(PlayFabError e)
    {
        Debug.Log("Error" + e.GenerateErrorReport());
    }

    public void BacktoMaininScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }

    [System.Serializable]
    public class JSListWrapper<T>
    {
        public List<T> list;
        public JSListWrapper(List<T> list) => this.list = list;
    }
}
