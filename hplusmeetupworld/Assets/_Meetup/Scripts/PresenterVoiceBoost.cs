
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PresenterVoiceBoost : UdonSharpBehaviour
{
	VRCPlayerApi[] presenters;

	public VRCPlayerApi[] Presenters { get { return presenters; } }

    void Start()
    {
		presenters = new VRCPlayerApi[32];
    }

	public override void OnPlayerLeft(VRCPlayerApi leavingPlayer)
	{
		for (int i = 0; i < presenters.Length; i++)
		{
			if (presenters[i] != null && presenters[i] == leavingPlayer) presenters[i] = null;
		}
	}

	public override void OnPlayerTriggerEnter(VRCPlayerApi player)
	{
		for (int i = 0; i < presenters.Length; i++)
		{
			if (presenters[i] == null) presenters[i] = player;
		}
	}

	public override void OnPlayerTriggerExit(VRCPlayerApi player)
	{
		for (int i = 0; i < presenters.Length; i++)
		{
			if (presenters[i] != null && presenters[i] == player) presenters[i] = null;
		}
	}

	bool IsLocalPlayerPresent()
	{
		foreach (VRCPlayerApi player in presenters)
		{
			if (player.isLocal) return true;
		}

		return false;
	}
}
