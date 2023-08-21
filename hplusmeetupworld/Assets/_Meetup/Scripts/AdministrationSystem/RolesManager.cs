
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class RolesManager : UdonSharpBehaviour
{
    public string[] Admins;
    public string[] Mods;
    public string[] CommunityBuilders;

    bool hasChecked = false;
    bool playerIsAdmin = false;
    bool playerIsMod = false;
    bool playerIsCommunityBuilder;

    public bool CanDoAnnouncements()
	{
        return playerIsAdmin || playerIsMod || playerIsCommunityBuilder;
	}

    public bool HasChecked()
	{
        return hasChecked;
	}

    void Start()
    {
        
    }

    void CheckPermissions()
	{
        hasChecked = true;
        string playerName = Networking.LocalPlayer.displayName;

        for(int i=0; i < Admins.Length; i++)
		{
            if (Admins[i] == playerName)
            {
                playerIsAdmin = true;
                break;
            }
		}

        for(int i=0; i < Mods.Length; i++)
		{
            if(Mods[i] == playerName)
			{
                playerIsMod = true;
                break;
			}
		}

        for(int i=0; i < CommunityBuilders.Length; i++)
		{
            if(CommunityBuilders[i] == playerName)
			{
                playerIsCommunityBuilder = true;
                break;
			}
        }
	}

	private void Update()
	{
		if(!hasChecked)
		{
            CheckPermissions();
		}
	}
}