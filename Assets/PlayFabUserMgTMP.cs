using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Collections.Generic;
using UnityEngine;

public class PlayFabUserMgTMP : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName, currentScore, displayName;
    [SerializeField] TextMeshProUGUI Msg;

    void UpdateMsg(string msg)
    {
        Debug.Log(msg);
        Msg.text = msg;
    }

    void OnError(PlayFabError e)
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonRegUser()
    {
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            Username = userName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }

    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Registration success!");

        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = displayName.text,
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }

    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated!" + r.DisplayName);
    }

    void OnLoginSuccess(LoginResult r)
    {
        UpdateMsg("Login Success" + r.PlayFabId + r.InfoResultPayload.PlayerProfile.DisplayName);
    }

    public void OnButtonLoginEmail()
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = userEmail.text,
            Password = userPassword.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }

    public void OnButtonLoginUserName()
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = userName.text,
            Password = userPassword.text,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnError);
    }

    public void OnButtonGetLeaderboard()
    {
        var Ibreq = new GetLeaderboardRequest
        {
            StatisticName = "highscore",
            StartPosition = 0,
            MaxResultsCount = 10
        };
        PlayFabClientAPI.GetLeaderboard(Ibreq, OnLeaderboardGet, OnError);
    }

    void OnLeaderboardGet(GetLeaderboardResult r)
    {
        string LeaderboardStr = "Leaderboard\n";
        foreach (var item in r.Leaderboard)
        {
            string onerow = item.Position + "/" +item.PlayFabId + "/" + item.DisplayName + "/" + item.StatValue + "\n";
            LeaderboardStr += onerow;
        }
        UpdateMsg(LeaderboardStr);
    }

    public void OnButtonSendLeaderboard()
    {
        var req = new UpdatePlayerStatisticsRequest
        {
            Statistics = new List<StatisticUpdate>
            {
                new StatisticUpdate
                {
                    StatisticName = "highscore",
                    Value = int.Parse(currentScore.text)
                }
            }
        };
        UpdateMsg("Submitting score:" + currentScore.text);
        PlayFabClientAPI.UpdatePlayerStatistics(req, OnLeaderboardUpdate, OnError);
    }

    void OnLeaderboardUpdate(UpdatePlayerStatisticsResult r)
    {
        UpdateMsg("Successful leaderboard sent:" + r.ToString());
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
