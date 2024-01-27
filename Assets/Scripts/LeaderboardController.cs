using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LeaderboardController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Msg;
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] TextMeshProUGUI Score;
    [SerializeField] TextMeshProUGUI Rank;

    private float totalTime;

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text = msg + '\n';
    }

    void UpdateName(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Name.text = msg + '\n';
    }

    void UpdateScore(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Score.text = msg + '\n';
    }

    void UpdateRank(string msg)
    {
        Debug.Log(msg);
        Rank.text = msg + "\n";
    }

    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }

    public void OnButtonGetGlobalLeaderboardName()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGetName, OnError);
    }

    public void OnButtonGetGlobalLeaderboardScore()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGetScore, OnError);
    }

    void OnLeaderboardGetName(GetLeaderboardResult r)
    {
        string LeaderboardStr = "";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.DisplayName + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateName(LeaderboardStr);
    }

    void OnLeaderboardGetScore(GetLeaderboardResult r)
    {
        string LeaderboardStr = "";
        foreach (var item in r.Leaderboard)
        {
            string onerow =  item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateScore(LeaderboardStr);
    }

    public void OnButtonGetGlobalLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGetGlobal, OnError);
    }

    void OnLeaderboardGetGlobal(GetLeaderboardResult r)
    {
        OnLeaderboardGetName(r);
        OnLeaderboardGetScore(r);
    }

    public void OnButtonGetNearbyLeaderboard()
    {
        var lbreq = new GetLeaderboardAroundPlayerRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboardAroundPlayer(lbreq, OnLeaderboardGetNearby, OnError);
    }

    void OnLeaderboardGetNearby(GetLeaderboardAroundPlayerResult r)
    {
        OnLeaderboardGetNearbyName(r);
        OnLeaderboardGetNearbyScore(r);
        OnLeaderboardGetNearbyRank(r);
    }

    void OnLeaderboardGetNearbyName(GetLeaderboardAroundPlayerResult r)
    {
        string LeaderboardStr = "";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.DisplayName + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateName(LeaderboardStr);
    }

    void OnLeaderboardGetNearbyScore(GetLeaderboardAroundPlayerResult r)
    {
        string LeaderboardStr = "";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateScore(LeaderboardStr);
    }

    void OnLeaderboardGetNearbyRank(GetLeaderboardAroundPlayerResult r)
    {
        string LeaderboardStr = "";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateRank(LeaderboardStr);
    }

    public void GotoScene(string scenename)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenename);
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        totalTime += Time.deltaTime;
        if (totalTime >= 30.0f)
        {
            if (SceneManager.GetActiveScene().name == "GlobalLeaderboardScene")
            {
                OnButtonGetGlobalLeaderboard();
                totalTime = 0;
            }
            else if (SceneManager.GetActiveScene().name == "NearbyLeaderboardScene")
            {
                OnButtonGetNearbyLeaderboard();
                totalTime = 0;
            }
        }
    }
}
