using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements

public class PlayFabUserMgtTMP : MonoBehaviour
{
    [SerializeField] TMP_InputField regUserEmail, regUserPassword, regUserName, regDisplayName, greetings;
    [SerializeField] TMP_InputField logUserEmail, logUserPassword, logUserName, logDisplayName, changeDisplayName, friendDisplayname;
    [SerializeField] TextMeshProUGUI Msg;

    public static string custom_id = string.Empty; // custom id for other platforms
   
    //public string playfabid = string.Empty;
    List<FriendInfo> _friends = null;
    //string removeFriendId = null;

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text=msg+'\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    public void OnButtonRegUser()
    { //for button click
        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = regUserEmail.text,
            Password = regUserPassword.text,
            Username = regUserName.text
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegSuccess, OnError);
    }
    void OnRegSuccess(RegisterPlayFabUserResult r)
    {
        UpdateMsg("Registration success!");

        //To create a player display name 
        var req = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = regDisplayName.text,
        };
        // update to profile
        PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
    }
    void OnDisplayNameUpdate(UpdateUserTitleDisplayNameResult r)
    {
        UpdateMsg("display name updated! " + r.DisplayName);
    }

    void OnFriendAdded(AddFriendResult r)
    {
        UpdateMsg("friend added!");
    }

    void OnLoginSuccess(LoginResult r)
    {
        UpdateMsg("Login Success!" + r.PlayFabId);
        ClientGetTitleData(); //MOTD
        GetUserData(); //Player Data
    }
    public void OnButtonLoginEmail() //login using email + password
    {
        var loginRequest = new LoginWithEmailAddressRequest
        {
            Email = logUserEmail.text,
            Password = logUserPassword.text,
            //to get player profile, to get displayname
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginRequest, OnLoginSuccess, OnError);
    }
    public void OnButtonLoginUserName() //login using username + password
    {
        var loginRequest = new LoginWithPlayFabRequest
        {
            Username = logUserName.text,
            Password = logUserPassword.text,
            //to get player profile, including displayname
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
            {
                GetPlayerProfile = true
            }
        };
        PlayFabClientAPI.LoginWithPlayFab(loginRequest, OnLoginSuccess, OnError);
    }

    public void LoginWithDeviceId()
    {
        GetDeviceId();
        Debug.Log("Using custom device ID: " + custom_id);
        var request = new LoginWithCustomIDRequest
        {
            CustomId = custom_id,
            TitleId = PlayFabSettings.TitleId,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            Debug.Log("Login Success!" + result.PlayFabId);
            UpdateDisplayName("Guest");
        }, OnError) ;  
        //PlayFabClientAPI.UpdateUserTitleDisplayName(req, OnDisplayNameUpdate, OnError);
        Debug.Log("login");
    }
   

    public void GetFriends()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
        }, result =>
        {
            _friends = result.Friends;
            DisplayFriends(_friends);
           
        }, OnError);
    }

    public void GetFriendRequests()
    {
        PlayFabClientAPI.GetFriendsList(new GetFriendsListRequest
        {
        }, result =>
        {
            _friends = result.Friends;
            DisplayFriendRequests(_friends);

        }, OnError);
    }

    void DisplayFriends(List<FriendInfo> friendsCache) 
    {
        foreach (FriendInfo friend in friendsCache)
        {
            if (friend.Tags != null)
            {
                if (friend.Tags.ToString() != "requestee")
                {
                    Msg.text += friend.TitleDisplayName + "\n";
                }
            }
        }
        //friendsCache.ForEach(f => UpdateMsg(f.TitleDisplayName)); 
        //friendsCache.ForEach(
        //        f => Msg.text += f.TitleDisplayName + f.Tags + "\n");

    }

    void DisplayFriendRequests(List<FriendInfo> friendsCache)
    {
        foreach (FriendInfo friend in friendsCache)
        {
            if (friend.Tags != null)
            {
                if (friend.Tags.ToString() == "requester")
                {
                    Msg.text += friend.TitleDisplayName + "\n";
                }
            }

        }
    }

    public void AddFriend()
    {
        var request = new AddFriendRequest();
        request.FriendTitleDisplayName = friendDisplayname.text;
        PlayFabClientAPI.AddFriend(request, OnFriendAdded, OnError);
    }

    void OnCloudScriptReqSent(string friendreqid)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "SendFriendRequest",
            FunctionParameter = new
            {
                playfabid = friendreqid
            },
            GeneratePlayStreamEvent = true
        }, OnExecSucc, OnError);
    }

    public void SendFriendReq()
    {
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = friendDisplayname.text
        };
        PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
        {
            OnCloudScriptReqSent(result.AccountInfo.PlayFabId);
        }, OnError);
    }

    void OnCloudScriptAcceptFriend(string friendaccpid)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "AcceptFriendRequest",
            FunctionParameter = new
            {
                playfabid = friendaccpid
            },
            GeneratePlayStreamEvent = true
        }, OnExecSucc, OnError);

    }

    public void AcceptFriendReq()
    {
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = friendDisplayname.text
        };
        PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
        {
            OnCloudScriptAcceptFriend(result.AccountInfo.PlayFabId);
        }, OnError);
    }

    void OnCloudScriptDenyFriend(string frienddenyid)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "DenyFriendRequest",
            FunctionParameter = new
            {
                playfabid = frienddenyid
            },
            GeneratePlayStreamEvent = true
        }, OnExecSucc, OnError);
    }

    public void DenyFriendReq()
    {
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = friendDisplayname.text
        };
        PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
        {
            OnCloudScriptDenyFriend(result.AccountInfo.PlayFabId);
        }, OnError);
    }

    public void RemoveFriend()
    {
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = friendDisplayname.text
        };
        PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
        {
            OnFriendRemoved(result.AccountInfo.PlayFabId);
        }, OnError);

        //var removeFriendReq = new RemoveFriendRequest();
        //removeFriendReq.FriendPlayFabId = removeFriendId;
        //PlayFabClientAPI.RemoveFriend(removeFriendReq, OnFriendRemoved, OnError);   
    }

    void OnFriendRemoved(string removeFriendId)
    {
        var removeFriendReq = new RemoveFriendRequest();
        removeFriendReq.FriendPlayFabId = removeFriendId;
        PlayFabClientAPI.RemoveFriend(removeFriendReq, result => UpdateMsg("friend removed!"), OnError);
    }

    public void UpdateDisplayName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = changeDisplayName.text.ToString();
            Debug.Log(changeDisplayName.text);
        }
       
        Debug.Log(name);
        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = name
        };
        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnDisplayNameUpdate, OnError);
    }

    public static bool GetDeviceId(bool silent = false) // silent suppresses the error
    {
        custom_id = SystemInfo.deviceUniqueIdentifier;
        return false;
        
    }

    public void OnButtonLogout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        UpdateMsg("logged out");
    }
    public void PasswordResetRequest()
    {
        var req = new SendAccountRecoveryEmailRequest
        {
            Email = logUserEmail.text,
            TitleId = "6881E"
        };
        PlayFabClientAPI.SendAccountRecoveryEmail(req, OnPasswordReset, OnError);
    }
    void OnPasswordReset(SendAccountRecoveryEmailResult r)
    {
        Msg.text = "Password reset email sent.";
    }

    public void ClientGetTitleData() { //Slide 33: MOTD
        PlayFabClientAPI.GetTitleData(new GetTitleDataRequest(),
            result => {
                if(result.Data == null || !result.Data.ContainsKey("MOTD")) UpdateMsg("No MOTD");
                else UpdateMsg("MOTD: "+result.Data["MOTD"]);
            },
            error => {
                UpdateMsg("Got error getting titleData:");
                UpdateMsg(error.GenerateErrorReport());
            }
        );
    }
    public void OnButtonSetUserData() { //Player Data
        PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest() {
            Data = new Dictionary<string, string>() {
                {"Greetings", greetings.text.ToString()},
            }
        },
        result => UpdateMsg("Successfully updated user data"),
        error => {
            UpdateMsg("Error setting user data");
            UpdateMsg(error.GenerateErrorReport());
        });
    }
    public void GetUserData() { //Player Data
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() 
        , result => {
            UpdateMsg("Got user data:");
            if (result.Data == null || !result.Data.ContainsKey("Greetings")) UpdateMsg("No Greetings");
            else { 
                UpdateMsg(result.Data["Greetings"].Value); 
            }
        }, (error) => {
            UpdateMsg("Got error retrieving user data:");
            UpdateMsg(error.GenerateErrorReport());
        });
    }
    public void GotoScene(string scenename){
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenename);
    }

    public void ExeCloudScript()
    {
        var req = new ExecuteCloudScriptRequest()
        {
            FunctionName = "cldfn1",
            FunctionParameter = new
            {
                name = "someone"
            }
        };
        PlayFabClientAPI.ExecuteCloudScript(req, OnExecSucc, OnError);
    }

    void OnExecSucc(ExecuteCloudScriptResult r)
    {
        Debug.Log(r.FunctionResult.ToString());
        UpdateMsg(r.FunctionResult.ToString());
    }
}

