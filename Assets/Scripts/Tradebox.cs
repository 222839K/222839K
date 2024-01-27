using PlayFab.ClientModels;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TradeInfo
{
    public string tradername;
    public string[] tradeid;


    public TradeInfo(string _tradername, string[] _tradeid)
    {
        tradername = _tradername;
        tradeid = _tradeid;
    }
}

public class Tradebox : MonoBehaviour
{
    [SerializeField] TMP_InputField tradedisplay;

    //public TradeInfo ReturnClass(OpenTradeResponse r)
    //{
    //    return new TradeInfo((string)r.Trade.OfferingPlayerId, (string)r.Trade.TradeId);
    //}
    public void SetUI(TradeInfo ti)
    {

        tradedisplay.text = ti.tradername + ti.tradeid;

    }
}