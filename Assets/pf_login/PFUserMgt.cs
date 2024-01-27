using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using System.Collections.Generic;
using TMPro; //for text mesh pro UI elements

public class PFUserMgt : MonoBehaviour
{
    [SerializeField] TMP_InputField userEmail, userPassword, userName;
    [SerializeField] TextMeshProUGUI Msg;
    public void OnButtonRegUser(){
        var regReq=new RegisterPlayFabUserRequest();
        regReq.Email=userEmail.text;
        regReq.Password=userPassword.text;
        regReq.Username=userName.text;
        PlayFabClientAPI.RegisterPlayFabUser(regReq,OnRegSucc,OnError);
    }
    void OnRegSucc(RegisterPlayFabUserResult r){
        Msg.text="REgister success. PlayFabID allocated:"+r.PlayFabId;
    }
    void OnError(PlayFabError e){
        Msg.text="Error:"+e.GenerateErrorReport();
    }

    public void OnButtonLogin(){
        var loginReq=new LoginWithEmailAddressRequest{
            Email=userEmail.text,
            Password=userPassword.text
        };
        PlayFabClientAPI.LoginWithEmailAddress(loginReq,OnLoginSuccess,OnError);
    }

    void OnLoginSuccess(LoginResult r){
       Msg.text="Login Success! Welcome back, "+r.PlayFabId;
    }


}
