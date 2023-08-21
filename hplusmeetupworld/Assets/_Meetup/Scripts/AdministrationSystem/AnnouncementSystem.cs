
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AnnouncementSystem : UdonSharpBehaviour
{
	RolesManager rolesManager;
	[SerializeField] GameObject announcementButton;
	[SerializeField] TMPro.TextMeshProUGUI buttonText;
	[SerializeField] Transform notificationTransform;
	[SerializeField] TMPro.TextMeshProUGUI announcementText;
	AudioSource alertSound;
	const float announcementDuration = 5f;
	const float fadeDuration = 0.25f;
	float announcementPhaseTimer = 0;
	const float cooldownDuration = 10;
	float cooldownTimer = 0;

	[UdonSynced]
	[SerializeField] string announcementMaker = "";

	int phase=3; // 0 fade in, 1 sustain, 2 fade out

	const bool announcementsEnabled = true;

	void Start()
    {
		rolesManager = GetComponent<RolesManager>();

		announcementButton.SetActive(false);
		alertSound = notificationTransform.GetComponent<AudioSource>();
    }

	bool HasAnnouncementPermission()
	{
		return rolesManager.HasChecked() &&
			rolesManager.CanDoAnnouncements();
	}


	public void SendAnnouncement()
	{
		if (cooldownTimer > 0)
		{
			return;
		}		

		if (HasAnnouncementPermission())
		{
			announcementMaker = Networking.LocalPlayer.displayName;
			SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All,
				"NetAnnouncement");
		}
	}


	public void NetAnnouncement()
	{
		StartAnnouncement();
	}

	void StartAnnouncement()
	{
		announcementText.text = string.Format("{0} has an announcement!",
			announcementMaker);
		announcementPhaseTimer = 0;
		phase = 0;
		alertSound.Play();
		cooldownTimer = cooldownDuration;
	}

	private void Update()
	{
		// update our button active state
		announcementButton.gameObject.SetActive(HasAnnouncementPermission() &&
			announcementsEnabled);

		// do our cooldown
		if(cooldownTimer > 0)
		{
			cooldownTimer -= Time.deltaTime;
			if (cooldownTimer < 0) cooldownTimer = 0;

			buttonText.text = (cooldownTimer > 0) ? cooldownTimer.ToString() : "Make Announcement";
		}

		if (Networking.LocalPlayer == null) return;

		VRCPlayerApi.TrackingData headTracking = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);

		float slerpPower = 0.021f;
		Vector3 forwardDirection = headTracking.rotation * Vector3.forward;
		forwardDirection = Vector3.Scale(forwardDirection, new Vector3(1, 0, 1));

		Quaternion targetRotation = Quaternion.LookRotation(forwardDirection, Vector3.up);
		Vector3 position = headTracking.position + (Vector3.down * 0.2f);
		Quaternion rotation = Quaternion.Slerp(notificationTransform.rotation, targetRotation, slerpPower);
		notificationTransform.SetPositionAndRotation(position, rotation);

		announcementText.text = string.Format("{0} has an announcement!",
			announcementMaker);

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
//#endif
	}
}
