using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TradeManager : MonoBehaviour
{
    [SerializeField] TMP_InputField traderid, tradeid, itemoffered, itemwanted, tradedisplay;
    [SerializeField] TextMeshProUGUI Msg;

    string itemid;

    

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text = msg + '\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        Debug.Log("Error" + e.GenerateErrorReport());
        //UpdateMsg("Error" + e.GenerateErrorReport());
        tradedisplay.text = "Error: " + e.GenerateErrorReport();
    }

    void OnGetPlayerTradesSuccess(GetPlayerTradesResponse r)
    {
        UpdateMsg("get player trades success!");
    }

    void OnSuccessGetCatalogItem(string _itemid)
    {
        itemid = _itemid;
    }

    public void GetInventorySendTrade()
    {
        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
        {
            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), result1 =>
            {
                OnSendTrade(result, result1);
            }, OnError);
        }, OnError);
    }

    void OnSendTrade(GetUserInventoryResult result, GetCatalogItemsResult result1)
    {
        foreach (var item in result.Inventory)
        {
            foreach (var item1 in result1.Catalog)
            {
                if (item.DisplayName == itemoffered.text && item1.DisplayName == itemwanted.text)
                {
                    GetCatalogItem();
                    GiveItemTo(item.ItemInstanceId, item1.ItemId);
                }
                
            }
        }

    }

    public void GiveItemTo(string offeredItemInstanceId, string wantedItemInstanceId)
    {
        Debug.Log(offeredItemInstanceId);
        Debug.Log(wantedItemInstanceId);
        PlayFabClientAPI.OpenTrade(new OpenTradeRequest
        {
            AllowedPlayerIds = null,
            OfferedInventoryInstanceIds = new List<string> { offeredItemInstanceId },
            RequestedCatalogItemIds = new List<string> { wantedItemInstanceId }
        }, OnGiveItem, OnError);
    }

    void OnGiveItem(OpenTradeResponse r)
    {
        var accInfoReq = new GetAccountInfoRequest();

        PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
        {
            Debug.Log(result.AccountInfo.PlayFabId);
            PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
            {
               
                FunctionName = "SendTradeData",
                FunctionParameter = new
                {
                    tradeid = r.Trade.TradeId,
                    traderid = result.AccountInfo.PlayFabId
                },
                GeneratePlayStreamEvent = true
            }, OnExecSucc, OnError);;
        }, OnError);
    }
    void OnExecSucc(ExecuteCloudScriptResult r)
    {
        Debug.Log(r.FunctionResult.ToString());
        UpdateMsg(r.FunctionResult.ToString());
    }

    public void GetAvailableTrades()
    {
        var request = new GetTitleDataRequest { Keys = new List<string> { "Trades" } };

        PlayFabClientAPI.GetTitleData(request, OnTitleDataReceived, OnError);

    }

    [SerializeField] Tradebox[] TradeBoxes;

    void OnTitleDataReceived(GetTitleDataResult r)
    {
        tradedisplay.text = "";
        if (r.Data.ContainsKey("Trades"))
        {
            var dataDict = PlayFab.PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer).DeserializeObject<Dictionary<string, string[]>>(r.Data["Trades"]) ;
            foreach (var kvp in dataDict)
            {
                var accInfoReq = new GetAccountInfoRequest
                {
                    PlayFabId = kvp.Key
                };

                PlayFabClientAPI.GetAccountInfo(accInfoReq, result =>
                {
                    tradedisplay.text += $"Trader Name: {result.AccountInfo.TitleInfo.DisplayName}:" + "\n";
                    tradedisplay.text += $"Trader ID: {kvp.Key}:" + "\n" + "Trade IDs: ";
                    foreach (var item in kvp.Value)
                    {
                        tradedisplay.text += item + "\n";
                    }
                }, OnError);
            }
        }
        else
        {
            tradedisplay.text += "Failed to retrieve title data.";
        }
    }

    void GetCatalogItem()
    {
        var primaryCatalogName = "Shop"; // In your game, this should just be a constant matching your primary catalog
        var request = new GetCatalogItemsRequest
        {
            CatalogVersion = primaryCatalogName,
        };
        PlayFabClientAPI.GetCatalogItems(request,
            result =>
            {
                List<CatalogItem> items = result.Catalog;
                foreach (CatalogItem i in items)
                {
                    if (i.DisplayName == itemwanted.text)
                    {
                        OnSuccessGetCatalogItem(i.ItemId);
                        //Debug.Log(i.ItemId);
                    }
                }

            }, OnError);
    }

    public void ExamineTrade()
    {
        PlayFabClientAPI.GetTradeStatus(new GetTradeStatusRequest
        {
            OfferingPlayerId = traderid.text,
            TradeId = tradeid.text
        }, OnExamineTrade, OnError);
    }
    void OnExamineTrade(GetTradeStatusResponse r)
    {
        var primaryCatalogName = "Shop";
        tradedisplay.text = "";
        var request = new GetCatalogItemsRequest
        {
            CatalogVersion = primaryCatalogName,
        };
        PlayFabClientAPI.GetCatalogItems(request,
           result =>
           {
               List<CatalogItem> items = result.Catalog;
               foreach (CatalogItem i in items)
               {
                   if (i.ItemId == r.Trade.OfferedCatalogItemIds[0])
                   {

                       tradedisplay.text += "Item Offered: " + i.DisplayName + "\n";
                   }
                   if (i.ItemId == r.Trade.RequestedCatalogItemIds[0])
                   {
                       tradedisplay.text += "Item Wanted: " + i.DisplayName + "\n";
                   }
               }

           }, OnError);
    }

    public void GetPlayerTrades()
    {
        PlayFabClientAPI.GetPlayerTrades(new GetPlayerTradesRequest
        {

        }, OnGetPlayerTradesSuccess, OnError);
    }
    public void AcceptTrade()
    {
        System.Collections.Generic.List<string> iteminstanceid = new System.Collections.Generic.List<string> {"test"};

        PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), result =>
        {
            foreach (var item in result.Inventory)
            {
                if (item.DisplayName == itemoffered.text)
                {
                    iteminstanceid[0] = (item.ItemInstanceId);
                    PlayFabClientAPI.AcceptTrade(new AcceptTradeRequest
                    {
                        OfferingPlayerId = traderid.text,
                        TradeId = tradeid.text,
                        AcceptedInventoryInstanceIds = iteminstanceid
                    }, OnAcceptTrade, OnError);
                    tradedisplay.text = "";
                    tradedisplay.text += "Successfully traded item";
                }
            }
        }, OnError);
    }

    void OnAcceptTrade(AcceptTradeResponse r)
    {
        PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
        {
            FunctionName = "RemoveTradeData",
            FunctionParameter = new
            {
                tradeid = r.Trade.TradeId,
                traderid = r.Trade.OfferingPlayerId
            },
            GeneratePlayStreamEvent = true
        }, OnExecSucc, OnError);
    }

    public void GetPlayerInventory()
    {
        tradedisplay.text = "";
        var UserInv = new GetUserInventoryRequest();
        PlayFabClientAPI.GetUserInventory(UserInv,
            result =>
            {
                List<ItemInstance> ii = result.Inventory;
                UpdateMsg("Player inventory");
                foreach (ItemInstance i in ii)
                {
                    tradedisplay.text += i.DisplayName + "\n";
                }
            }, OnError);
    }

    public void GotoScene(string scenename)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scenename);
    }
}
