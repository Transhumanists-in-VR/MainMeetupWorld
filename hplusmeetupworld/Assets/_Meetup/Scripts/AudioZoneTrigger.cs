using System.Collections.Generic;
using System.Linq;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

public class AudioZoneTrigger : UdonSharpBehaviour
{
	public float nearDistance=0;
	public float farDistance=6f;
	float nearAngle = 2; // used for view based audio boosting
	float farAngle = 15;
	VRCPlayerApi[] playersInRegion;
	VRCPlayerApi localPlayer;
	public PresenterVoiceBoost PresenterBoost; // if the presenter boost is available,
											   // their voice ranges won't get clipped while we are in this zone
	PlayerAudioTracker AudioTracker;
	int playerLayer;
	int collisionMask;

	private void Start()
	{
		GameObject vrcRoot = GameObject.Find("VRCWorld");
		AudioTracker = vrcRoot.GetComponent<PlayerAudioTracker>();
		playersInRegion = new VRCPlayerApi[64];
		playerLayer = LayerMask.NameToLayer("Player");
		collisionMask = 1 >> playerLayer;
		collisionMask = ~collisionMask;

		localPlayer = LocalPlayer();
	}

	VRCPlayerApi LocalPlayer()
	{
		VRCPlayerApi[] allPlayers = new VRCPlayerApi[64];
		allPlayers = VRCPlayerApi.GetPlayers(allPlayers);
		foreach (VRCPlayerApi player in allPlayers)
		{
			if (player != null && player.IsValid() && player.isLocal) return player;
		}

		return null;
	}

	public override void OnPlayerLeft(VRCPlayerApi leavingPlayer)
	{
		// the log message works
		// but there's an error in the disconnect logic somewhere
		/*Debug.Log("Player " + leavingPlayer.displayName + " disconnected: " + gameObject.name);
		Debug.Log("Disconnecting player is valid:" + leavingPlayer.IsValid());*/

		for (int i = 0; i < playersInRegion.Length; i++)
		{
			if (playersInRegion[i] != null && playersInRegion[i].IsValid())
			{
				if (playersInRegion[i] == leavingPlayer) playersInRegion[i] = null;
			}
			else if (playersInRegion[i] != null && !playersInRegion[i].IsValid()) playersInRegion[i] = null;
		}
	}

	public override void OnPlayerTriggerEnter(VRCPlayerApi player)
	{
		int playerIndex = AudioTracker.FindPlayerSlot(player);
		/*Debug.Log("Player " + player.displayName + " ID: " + playerIndex +
			" entered zone: " + gameObject.name);*/
		for (int i=0; i < playersInRegion.Length; i++)
		{
			bool slotIsNull = playersInRegion[i] == null;
			bool slotIsValid = false;
			if (!slotIsNull) slotIsValid = playersInRegion[i].IsValid();

			if (slotIsNull || !slotIsValid)
			{
				playersInRegion[i] = player;
				break; // the lack of this was causing the whole array to get filled by the first player
			}
		}

		PrintPlayersInRegion();

		SetAudioValues();
	}

	public override void OnPlayerTriggerExit(VRCPlayerApi player)
	{
		int playerIndex = AudioTracker.FindPlayerSlot(player);
		/*Debug.Log("Player " + player.displayName + " ID: " + playerIndex + 
			" exited zone: " + gameObject.name);*/

		for (int i = 0; i < playersInRegion.Length; i++)
		{
			if (playersInRegion[i] != null && playersInRegion[i] == player) playersInRegion[i] = null;
		}

		PrintPlayersInRegion();

		if (!player.isLocal) SetAudioValues();
		else // undo-everything
		{
			ResetAudioValues();
		}
	}

	void PrintPlayersInRegion()
	{
		for (int i = 0; i < playersInRegion.Length; i++)
		{
			VRCPlayerApi player = playersInRegion[i];
			int playerIndex = AudioTracker.FindPlayerSlot(player);
			if (player != null && player.IsValid())
			{
				Debug.Log("Player " + player.displayName + "ID: " + playerIndex);
			}
		}
	}

	void ResetAudioValues()
	{
		VRCPlayerApi[] allPlayers = new VRCPlayerApi[64];
		allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

		foreach (VRCPlayerApi player in allPlayers)
		{
			if (player == null || !player.IsValid()) continue;
			AudioTracker.SetValuesForPlayer(player, 0, 25);
		}
	}

	bool IsLocalPlayerPresent()
	{
		foreach (VRCPlayerApi player in playersInRegion)
		{
			if (player != null && player.IsValid() && player.isLocal) return true;
		}

		return false;
	}

	bool PlayerIsInList(VRCPlayerApi[] players, VRCPlayerApi player)
	{
		foreach(VRCPlayerApi playerCandidate in players)
		{
			if (playerCandidate == player) return true;
		}

		return false;
	}

	private void Update()
	{
		SetAudioValues();
	}

	#region Linear Interpolation Functions
	/// <summary>
	/// Lerp with a sine tvalue
	/// </summary>
	float Sinerp(float from, float to, float t)
	{
		return Mathf.Lerp(from, to, Mathf.Sin(t * Mathf.PI * 0.5f));
	}

	/// <summary>
	/// Lerp with a cosine tvalue
	/// </summary>
	float Coserp(float from, float to, float t)
	{
		return Mathf.Lerp(from, to, (1f - Mathf.Cos(t * Mathf.PI * 0.5f)));
	}

	/// <summary>
	/// lerp with an exponential tvalue
	/// </summary>
	float Exerp(float from, float to, float t)
	{
		return Mathf.Lerp(from, to, t * t);
	}
	#endregion

	void SetAudioValues()
	{
		if (IsLocalPlayerPresent())
		{
			VRCPlayerApi[] allPlayers = new VRCPlayerApi[32];
			allPlayers = VRCPlayerApi.GetPlayers(allPlayers);

			foreach (VRCPlayerApi player in allPlayers)
			{
				if (player == null) continue;
				if (!player.IsValid())
				{
					Debug.Log("Skipping invalid player");
					continue;
				}

				VRCPlayerApi.TrackingData otherPlayerHeadTracking = player.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
				VRCPlayerApi.TrackingData localPlayerHeadTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

				Vector3 directionToOtherPlayer = (otherPlayerHeadTracking.position - localPlayerHeadTracking.position);
				float distanceToOtherPlayer = directionToOtherPlayer.magnitude;
				if (distanceToOtherPlayer > Mathf.Epsilon) directionToOtherPlayer /= distanceToOtherPlayer;
				else directionToOtherPlayer = directionToOtherPlayer.normalized; // avoiding any division by 0

				float angle = Vector3.Angle(localPlayerHeadTracking.rotation * Vector3.forward,
					directionToOtherPlayer);
				float additionalPower = 1 - Mathf.InverseLerp(nearAngle, farAngle, angle);

				if (PlayerIsInList(playersInRegion, player))
				{
					float finalFarValue = farDistance;

					if (!player.isLocal)
					{
						finalFarValue = Sinerp(farDistance, AudioTracker.DefaultFarDistance, additionalPower);
					}

					AudioTracker.SetValuesForPlayer(player, nearDistance, finalFarValue);
				}
				/*else if (PresenterBoost != null && // enable this for presenter boosting
					PlayerIsInList(PresenterBoost.Presenters, player))
				{
					AudioTracker.ResetValuesForPlayer(player);
				}*/
				else
				{
					// do a punch-through check
					bool hit = Physics.Raycast(new Ray(localPlayerHeadTracking.position, directionToOtherPlayer),
						distanceToOtherPlayer, collisionMask);

					if(hit && angle < farAngle)
					{
						float finalFarValue = farDistance;
						finalFarValue = Sinerp(farDistance, AudioTracker.DefaultFarDistance, additionalPower);
						AudioTracker.SetValuesForPlayer(player, nearDistance, finalFarValue);
					}
					else AudioTracker.SetValuesForPlayer(player, 0, 0);
				}
			}
		}
	}
}
