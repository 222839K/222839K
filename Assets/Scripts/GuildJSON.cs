using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Invite
{
    public string guildid;

    public Invite(string _guildid)
    {
        guildid = _guildid;
    }
}

public class GuildJSON : MonoBehaviour
{
    public Invite ReturnClass(string groupentid)
    {

        return new Invite(groupentid);
    }
}