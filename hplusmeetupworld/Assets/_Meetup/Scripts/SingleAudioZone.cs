
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// When the player enters the trigger zone, 
/// </summary>
public class SingleAudioZone : UdonSharpBehaviour
{
	[SerializeField] AudioSource sourceThisRegion;
	[SerializeField] AudioSource[] otherSourceRegions;
	[SerializeField] AudioSource enableOnExit;

	public override void OnPlayerTriggerEnter(VRCPlayerApi player)
	{
		if (player.isLocal)
		{
			foreach (AudioSource item in otherSourceRegions) item.volume = 0;
			sourceThisRegion.volume = 1;
		}
		enableOnExit.volume = 0;
	}

	public override void OnPlayerTriggerExit(VRCPlayerApi player)
	{
		if(player.isLocal)
		{
			foreach (AudioSource item in otherSourceRegions) item.volume = 0;
			sourceThisRegion.volume = 0;
			enableOnExit.volume = 1;
		}
	}
}
