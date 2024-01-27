using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements

public class playfabscore : MonoBehaviour
{
    [SerializeField] TMP_Text name;
    [SerializeField] TMP_Text score;
    [SerializeField] TextMeshProUGUI Msg;

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text = msg + '\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonGetLeaderboard()
    {
        var lbreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore", //playfab leaderboard statistic name
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(lbreq, OnLeaderboardGet, OnError);
    }
    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + "/" + item.PlayFabId + "/" + item.DisplayName + "/" + item.StatValue + "\n";
            LeaderboardStr += onerow; //combine all display into one string 1.
        }
        UpdateMsg(LeaderboardStr);
    }
    public void OnButtonSendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>{ //playfab leaderboard statistic name
                new StatisticUpdate{
                    StatisticName="highscore",
                    Value=int.Parse(score.text)
                }
            }
        };
        UpdateMsg("Submitting score:" + score.text);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }
    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent:" + r.ToString());
    }
}
