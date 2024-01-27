using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.GroupsModels;
using PlayFab.ProfilesModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuildManager : MonoBehaviour
{
	[SerializeField] TMP_InputField displayname, guildname;
	[SerializeField] TextMeshProUGUI Msg;

	public readonly HashSet<KeyValuePair<string, string>> EntityGroupPairs = new HashSet<KeyValuePair<string, string>>();
	public readonly Dictionary<string, string> GroupNameById = new Dictionary<string, string>();

    void UpdateMsg(string msg) //to display in console and messagebox
    {
        Debug.Log(msg);
        Msg.text = msg + '\n';
    }
    void OnError(PlayFabError e) //report any errors here!
    {
        UpdateMsg("Error" + e.GenerateErrorReport());
    }

    void OnExecSucc(ExecuteCloudScriptResult r)
    {
        Debug.Log(r.FunctionResult.ToString());
        UpdateMsg(r.FunctionResult.ToString());
    }

    public static PlayFab.GroupsModels.EntityKey EntityKeyMaker(string entityId)
	{
		return new PlayFab.GroupsModels.EntityKey { Id = entityId };
	}

	public static PlayFab.ProfilesModels.EntityKey ProfileEntityMaker(PlayFab.GroupsModels.EntityKey entityKeyid)
	{
        return new PlayFab.ProfilesModels.EntityKey
        {
            Id = entityKeyid.Id,
			Type = entityKeyid.Type
        };
    }

	public static PlayFab.GroupsModels.EntityKey EntityMaker(PlayFab.ClientModels.EntityKey entityKey)
	{
		return new PlayFab.GroupsModels.EntityKey
		{
			Id = entityKey.Id,
			Type = entityKey.Type
        };
	}

    public static PlayFab.GroupsModels.EntityKey EntityMaker(PlayFab.GroupsModels.EntityKey entityKey)
    {
        return new PlayFab.GroupsModels.EntityKey
        {
            Id = entityKey.Id,
            Type = entityKey.Type
        };
    }

    private void OnSharedError(PlayFab.PlayFabError error)
	{
		Debug.LogError(error.GenerateErrorReport());
	}

	public void ListGroups()
	{
		var request = new ListMembershipRequest {};
		PlayFabGroupsAPI.ListMembership(request, OnListGroups, OnSharedError);
	}
	private void OnListGroups(ListMembershipResponse response)
	{
        Msg.text = "";
        var prevRequest = (ListMembershipRequest)response.Request;
		var request = new GetEntityProfileRequest();

        PlayFabProfilesAPI.GetProfile(request, result =>
        {
			foreach (var pair in response.Groups)
			{
				GroupNameById[pair.Group.Id] = pair.GroupName;
				EntityGroupPairs.Add(new KeyValuePair<string, string>(result.Profile.Lineage.TitlePlayerAccountId, pair.Group.Id));
				Msg.text += pair.GroupName + "\n";
			}
        }, OnError);
    }

	public void CreateGroup()
	{
		// A player-controlled entity creates a new group
		var request = new CreateGroupRequest { GroupName = guildname.text};
		PlayFabGroupsAPI.CreateGroup(request, OnCreateGroup, OnSharedError);
	}
	private void OnCreateGroup(CreateGroupResponse response)
	{
		Debug.Log("Group Created: " + response.GroupName + " - " + response.Group.Id);
		UpdateMsg("Group Created: " + response.GroupName + " - " + response.Group.Id);

		var accInfoReq = new GetAccountInfoRequest();
        //var prevRequest = (CreateGroupRequest)response.Request;

		PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
		{
			EntityGroupPairs.Add(new KeyValuePair<string, string>(result1.AccountInfo.TitleInfo.TitlePlayerAccount.Id, response.Group.Id));
        }, OnError);
		GroupNameById[response.Group.Id] = response.GroupName;
	}

	public void DeleteGroup()
	{
        var request = new GetGroupRequest
        {
            GroupName = guildname.text
        };

		PlayFabGroupsAPI.GetGroup(request, result =>
		{
			Debug.Log(result.Group.Id.ToString());
            Debug.Log(EntityKeyMaker(result.Group.Id.ToString()));
            // A title, or player-controlled entity with authority to do so, decides to destroy an existing group
            var request = new DeleteGroupRequest { Group = EntityKeyMaker(result.Group.Id.ToString()) };
			PlayFabGroupsAPI.DeleteGroup(request, OnDeleteGroup, OnSharedError);
		}, OnError);
	}

	private void OnDeleteGroup(PlayFab.GroupsModels.EmptyResponse response)
	{
		var prevRequest = (DeleteGroupRequest)response.Request;
		Debug.Log("Group Deleted: " + prevRequest.Group.Id);
		UpdateMsg("Group Deleted: " + prevRequest.Group.Id);
		var temp = new HashSet<KeyValuePair<string, string>>();
		foreach (var each in EntityGroupPairs)
			if (each.Value != prevRequest.Group.Id)
				temp.Add(each);
		EntityGroupPairs.IntersectWith(temp);
		GroupNameById.Remove(prevRequest.Group.Id);
	}

    public void InviteToGroup()
	{
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = displayname.text
        };

        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

		PlayFabGroupsAPI.GetGroup(request1, result =>
		{
			PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
			{
				var request = new InviteToGroupRequest
				{
					Group = EntityMaker(result.Group),
					Entity = EntityMaker(result1.AccountInfo.TitleInfo.TitlePlayerAccount)
				};
				PlayFabGroupsAPI.InviteToGroup(request, OnSuccessInvite, OnSharedError);
			}, OnError);
		}, OnError);
	}

	public void OnSuccessInvite(InviteToGroupResponse r)
	{
		UpdateMsg("Successfully sent invite");
	}

	public void OnInvite()
	{
        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

        // Presumably, this would be part of a separate process where the recipient reviews and accepts the request
        PlayFabGroupsAPI.GetGroup(request1, result =>
		{
			var request = new AcceptGroupInvitationRequest { Group = EntityMaker(result.Group) };
            PlayFabGroupsAPI.AcceptGroupInvitation(request, OnAcceptInvite, OnSharedError);
        },OnError);
	}

    public void OnAcceptInvite(PlayFab.GroupsModels.EmptyResponse response)
    {
		var accInfoReq = new GetAccountInfoRequest();

        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

        PlayFabGroupsAPI.GetGroup(request1, result =>
		{
			PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
			{
				Debug.Log("Entity Added to Group: " + result1.AccountInfo.TitleInfo.TitlePlayerAccount.Id + " to " + result.Group.Id);
				EntityGroupPairs.Add(new KeyValuePair<string, string>(result1.AccountInfo.TitleInfo.TitlePlayerAccount.Id, result.Group.Id));
			}, OnError);
		}, OnError);
    }

	//void OnCloudScriptSendInvite(InviteToGroupResponse response)
	//{
	//	var prevRequest = (InviteToGroupRequest)response.Request;

	//	var request = new GetGroupRequest
	//	{
	//		Group = prevRequest.Group
	//	};

	//	var request1 = new GetAccountInfoRequest();

	//	var request2 = new GetEntityProfileRequest
	//	{
	//		Entity = ProfileEntityMaker(prevRequest.Entity)
	//	};

	//	PlayFabProfilesAPI.GetProfile(request2, result2 =>
	//	{
 //           PlayFabClientAPI.GetAccountInfo(request1, result1 =>
	//		{
	//			PlayFabGroupsAPI.GetGroup(request, result =>
	//			{
	//				//Debug.Log(result2.Profile.Lineage.MasterPlayerAccountId.ToString());
 //                   //Debug.Log(result.GroupName.ToString());
 //                   //Debug.Log(result1.AccountInfo.PlayFabId.ToString());
 //                   PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest()
	//				{
						
	//					FunctionName = "SendGuildInvite",
	//					FunctionParameter = new
	//					{
	//						inviteeid = result2.Profile.Lineage.MasterPlayerAccountId.ToString(),
	//						guildname = result.GroupName.ToString(),
	//						inviterid = result1.AccountInfo.PlayFabId.ToString()
	//					},
	//					GeneratePlayStreamEvent = true
	//				}, OnExecSucc, OnError);

	//			}, OnError);
	//		}, OnError);
	//	}, OnError);
 //   }

	public void GetInApp()
	{
        var request = new GetGroupRequest
        {
            GroupName = guildname.text
        };

		PlayFabGroupsAPI.GetGroup(request, result =>
		{
			var request1 = new ListGroupApplicationsRequest
			{
				Group = result.Group
			};
			PlayFabGroupsAPI.ListGroupApplications(request1, OnGetInApp, OnError);
		}, OnError);

		
	}

	void OnGetInApp(ListGroupApplicationsResponse r)
	{
        Msg.text = "";

        if (r.Applications.Count == 0)
        {
            Msg.text = "No applications received";
        }

        foreach (var item in r.Applications)
		{
            var request = new GetEntityProfileRequest
            {
                Entity = ProfileEntityMaker(item.Entity.Key)
            };

            var request1 = new GetGroupRequest
            {
                GroupName = guildname.text
            };


			PlayFabGroupsAPI.GetGroup(request1, result2 =>
			{
				PlayFabProfilesAPI.GetProfile(request, result =>
				{
					var accInfoReq = new GetAccountInfoRequest
					{
						PlayFabId = result.Profile.Lineage.MasterPlayerAccountId
					};
					PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
					{
						Msg.text = "Incoming Applications: " + "\n";
						Msg.text += result1.AccountInfo.TitleInfo.DisplayName + " | " + result2.GroupName;
					}, OnError);
				}, OnError);
			}, OnError);
        }
    }

	public void GetApp()
	{
		var request = new ListMembershipOpportunitiesRequest();

		PlayFabGroupsAPI.ListMembershipOpportunities(request, OnGetApp, OnError);
	}

    public void GetInv()
    {
        var request = new ListMembershipOpportunitiesRequest();

        PlayFabGroupsAPI.ListMembershipOpportunities(request, OnGetInv, OnError);
    }

    void OnGetApp(ListMembershipOpportunitiesResponse r)
	{
        Msg.text = "";

		if (r.Applications.Count == 0)
		{
            Msg.text = "No applications sent";
        }

        foreach (var item in r.Applications)
		{
            var request = new GetEntityProfileRequest
            {
                Entity = ProfileEntityMaker(item.Entity.Key)
            };

			PlayFabProfilesAPI.GetProfile(request, result =>
			{
				var accInfoReq = new GetAccountInfoRequest
				{
					PlayFabId = result.Profile.Lineage.MasterPlayerAccountId
                };
                PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
                {

                    Msg.text = "Sent Applications: " + "\n";
                    Msg.text += result1.AccountInfo.TitleInfo.DisplayName+ "\n";
                }, OnError);
            }, OnError);

           
		}
	}

    void OnGetInv(ListMembershipOpportunitiesResponse r)
    {
        Msg.text = "";

		if (r.Invitations.Count == 0)
		{
            Msg.text = "No invitations received";
        }

        foreach (var item in r.Invitations)
        {
            var request = new GetGroupRequest
            {
                Group = item.Group
            };

			PlayFabGroupsAPI.GetGroup(request, result =>
			{
                Msg.text = "Invitations: " + "\n";
                Msg.text += result.GroupName + "\n";
            }, OnError);

                
        }
    }

   

	public void ApplyToGroup()
	{
        var request = new GetGroupRequest
        {
            GroupName = guildname.text
        };
        PlayFabGroupsAPI.GetGroup(request, result => 
		{ 
			var request1 = new ApplyToGroupRequest { 
				Group = EntityKeyMaker(result.Group.Id.ToString()) 
			}; 
			PlayFabGroupsAPI.ApplyToGroup(request1, OnApplySuccess, OnSharedError);
        }, OnError);
        // A player-controlled entity applies to join an existing group (of which they are not already a member)
	}

	void OnApplySuccess(ApplyToGroupResponse response)
	{
		UpdateMsg("Successfully applied to a guild");
	}

    public void OnApply()
	{
        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = displayname.text
        };


		PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
		{
			PlayFabGroupsAPI.GetGroup(request1, result =>
			{
				// Presumably, this would be part of a separate process where the recipient reviews and accepts the request
				var request = new AcceptGroupApplicationRequest 
				{ 
					Group = EntityMaker(result.Group),
                    Entity = EntityMaker(result1.AccountInfo.TitleInfo.TitlePlayerAccount)
                };
				PlayFabGroupsAPI.AcceptGroupApplication(request, OnAcceptApplication, OnSharedError);
			}, OnError);
		}, OnError);
	}

	public void OnAcceptApplication(PlayFab.GroupsModels.EmptyResponse response)
	{
        var prevRequest = (AcceptGroupApplicationRequest)response.Request;

        var request = new GetEntityProfileRequest
        {
            Entity = ProfileEntityMaker(prevRequest.Entity)
        };

        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

		PlayFabProfilesAPI.GetProfile(request, result =>
		{
			PlayFabGroupsAPI.GetGroup(request1, result1 =>
			{
				UpdateMsg("Entity Added to Group: " + result.Profile.DisplayName + " to " + result1.GroupName);
			}, OnError);
		}, OnError);
	}

	public void KickMember()
	{
        var accInfoReq = new GetAccountInfoRequest
        {
            TitleDisplayName = displayname.text
        };

        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

		PlayFabGroupsAPI.GetGroup(request1, result =>
		{
			PlayFabClientAPI.GetAccountInfo(accInfoReq, result1 =>
			{
				var request = new RemoveMembersRequest
				{
					Group = EntityMaker(result.Group),
					Members = new List<PlayFab.GroupsModels.EntityKey> { EntityMaker(result1.AccountInfo.TitleInfo.TitlePlayerAccount) }
				};
				PlayFabGroupsAPI.RemoveMembers(request, OnKickMembers, OnSharedError);
			}, OnError);
		}, OnError);
           
		
	}

	private void OnKickMembers(PlayFab.GroupsModels.EmptyResponse response)
	{
		var prevRequest = (RemoveMembersRequest)response.Request;

        var request = new GetEntityProfileRequest
        {
            Entity = ProfileEntityMaker(prevRequest.Members[0])
        };

        var request1 = new GetGroupRequest
        {
            GroupName = guildname.text
        };

        Debug.Log("Entity kicked from Group: " + prevRequest.Members[0].Id + " to " + prevRequest.Group.Id);
        PlayFabProfilesAPI.GetProfile(request, result =>
        {
			PlayFabGroupsAPI.GetGroup(request1, result1 =>
			{
				UpdateMsg("Entity kicked from Group: " + result.Profile.DisplayName + " to " + result1.GroupName);
			}, OnError);
        }, OnError);
        EntityGroupPairs.Remove(new KeyValuePair<string, string>(prevRequest.Members[0].Id, prevRequest.Group.Id));
	}

	public void GotoScene(string scenename)
	{
		UnityEngine.SceneManagement.SceneManager.LoadScene(scenename);
	}

    [System.Serializable]
    public class JSListWrapper<T>
    {
        public List<T> list;
        public JSListWrapper(List<T> list) => this.list = list;
    }
}

