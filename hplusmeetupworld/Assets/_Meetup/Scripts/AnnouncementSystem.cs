
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnnouncementSystem : UdonSharpBehaviour
{
	string[] userWhiteList = new string[]
	{
		"HampVR",
		"JCorvinus",
		"AllisonDeere",
		"lauren0001"
	};

	[SerializeField] GameObject announcementButton;
	[SerializeField] TMPro.TextMeshProUGUI buttonText;
	[SerializeField] Transform notificationTransform;
	[SerializeField] TMPro.TextMeshProUGUI announcementText;
	AudioSource alertSound;
	const float announcementDuration = 5f;
	const float fadeDuration = 0.25f;
	float announcementPhaseTimer = 0;
	VRCPlayerApi localPlayer;
	const float cooldownDuration = 10;
	float cooldownTimer = 0;

	int phase=3; // 0 fade in, 1 sustain, 2 fade out

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

	void Start()
    {
		localPlayer = LocalPlayer();

		announcementButton.SetActive(false);
		CheckLocalPlayerPermissions();
		alertSound = notificationTransform.GetComponent<AudioSource>();
    }

	void CheckLocalPlayerPermissions()
	{
		if (localPlayer != null && localPlayer.IsValid())
		{
			foreach (string userName in userWhiteList)
			{
				if (localPlayer.displayName == userName)
				{
					announcementButton.gameObject.SetActive(true);
					break;
				}
			}
		}
		else
		{
			localPlayer = LocalPlayer();
		}
	}

	public void SendAnnouncement()
	{
		if(cooldownTimer > 0)
		{
			return;
		}

		int playerNameID = -1;
		for(int i=0; i < userWhiteList.Length; i++)
		{
			if(userWhiteList[i] == localPlayer.displayName)
			{
				playerNameID = i;
				break;
			}
		}

		switch (playerNameID)
		{
			case (0):
				SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
					"SendHampAnnouncement");
				break;

			case (1):
				SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
					"SendJCAnnouncement");
				break;

			case (2):
				SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
					"SendAllisonAnnouncement");
				break;

			case (3):
				SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
					"SendLaurenAnnouncement");
				break;

			default:
				break;
		}
	}

	public void SendHampAnnouncement()
	{
		StartAnnouncement(0);
	}

	public void SendJCAnnouncement()
	{
		StartAnnouncement(1);
	}

	public void SendAllisonAnnouncement()
	{
		StartAnnouncement(2);
	}

	public void SendLaurenAnnouncement()
	{
		StartAnnouncement(3);
	}

	void StartAnnouncement(int whiteListID)
	{
		announcementText.text = string.Format("{0} has an announcement!",
			userWhiteList[whiteListID]);
		announcementPhaseTimer = 0;
		phase = 0;
		alertSound.Play();
		cooldownTimer = cooldownDuration;
	}

	private void Update()
	{
#if !UNITY_EDITOR
		// check our local player validity
		CheckLocalPlayerPermissions();

		// do our cooldown
		if(cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer < 0) cooldownTimer = 0;

			buttonText.text = (cooldownTimer > 0) ? cooldownTimer.ToString() : "Make Announcement";
		}

		VRCPlayerApi.TrackingData headTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

		float slerpPower = 0.021f;
		Vector3 forwardDirection = headTracking.rotation * Vector3.forward;
		forwardDirection = Vector3.Scale(forwardDirection, new Vector3(1, 0, 1));

		Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
		Vector3 position = headTracking.position + (Vector3.down * 0.2f);
		Quaternion rotation = Quaternion.Slerp(notificationTransform.rotation, targetRotation, slerpPower);
		notificationTransform.SetPositionAndRotation(position, rotation);

		float tValue = 0;
		switch (phase)
		{
			case (0):
				announcementPhaseTimer += Time.deltaTime;
				tValue = Mathf.InverseLerp(0, fadeDuration, announcementPhaseTimer);
				announcementText.color = Color.Lerp(Color.clear, Color.white, tValue);

				if (announcementPhaseTimer > fadeDuration)
				{
					phase = 1;
					announcementPhaseTimer = 0;
				}
				break;

			case (1):
				announcementPhaseTimer += Time.deltaTime;
				announcementText.color = Color.white;

				if(announcementPhaseTimer > announcementDuration)
				{
					phase = 2;
					announcementPhaseTimer = 0;
				}
				break;

			case (2):
				announcementPhaseTimer += Time.deltaTime;
				tValue = Mathf.InverseLerp(0, fadeDuration, announcementPhaseTimer);
				announcementText.color = Color.Lerp(Color.white, Color.clear, tValue);

				if(announcementPhaseTimer > fadeDuration)
				{
					phase = 3;
				}
				break;

			case (3):
				announcementText.color = Color.clear;
				break;

			default:
				break;
		}
#endif
	}
}
