
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerAudioTracker : UdonSharpBehaviour
{
	[SerializeField] GameObject visualizerPrefab;
	GameObject[] visualizers;

	[SerializeField] float defaultFarDistance = 19;
	[SerializeField] float defaultNearDistance=0;
	float[] nearDistances;
	float[] farDistances;

	public float DefaultFarDistance
	{
		get { return defaultFarDistance; }
	}

	VRCPlayerApi[] playerObjects;

	public bool EnableAudioVisualization;

	private void Start()
	{
		playerObjects = new VRCPlayerApi[64];
		visualizers = new GameObject[64];
		nearDistances = new float[64];
		farDistances = new float[64];

		for(int i=0; i < visualizers.Length; i++)
		{
			visualizers[i] = VRCInstantiate(visualizerPrefab);
			visualizers[i].gameObject.SetActive(false);
			nearDistances[i] = 0;
			farDistances[i] = 25;
		}
	}

	public override void OnPlayerJoined(VRCPlayerApi player)
	{
		AddPlayerToList(player);
		ResetValuesForPlayer(player);
	}

	public override void OnPlayerLeft(VRCPlayerApi player)
	{
		int playerSlot = FindPlayerSlot(player);
		playerObjects[playerSlot] = null;
	}

	int FindFreePlayerSlot()
	{
		for (int i = 0; i < playerObjects.Length; i++)
		{
			bool slotIsNull = playerObjects[i] == null;
			bool slotIsValid = false;
			if (!slotIsNull) slotIsValid = playerObjects[i].IsValid();

			if (slotIsNull || !slotIsValid)
			{
				return i;
			}
		}

		return -1; // failure case
	}

	public int FindPlayerSlot(VRCPlayerApi player)
	{
		for (int i = 0; i < playerObjects.Length; i++)
		{
			if(player == playerObjects[i])
			{
				return i;
			}
		}

		return -1;
	}

	void AddPlayerToList(VRCPlayerApi player)
	{
		int freeSlot = FindFreePlayerSlot();
		playerObjects[freeSlot] = player;
	}

	public void ToggleAudioVis()
	{
		EnableAudioVisualization = !EnableAudioVisualization;
	}

	public void ResetValuesForPlayer(VRCPlayerApi player)
	{
		SetValuesForPlayer(player, defaultNearDistance, defaultFarDistance);
	}

	public void SetValuesForPlayer(VRCPlayerApi player, float near, float far)
	{
		int playerIndex = FindPlayerSlot(player);
		if (playerIndex > -1)
		{
			nearDistances[playerIndex] = near;
			farDistances[playerIndex] = far;
			playerObjects[playerIndex].SetVoiceDistanceNear(near);
			playerObjects[playerIndex].SetVoiceDistanceFar(far);
		}
		else
		{
			Debug.LogError("Couldn't find index for player: " + player.displayName);
		}
	}

	private void Update()
	{
		for (int i=0; i < visualizers.Length; i++)
		{
			if(i < VRCPlayerApi.GetPlayerCount())
			{
				VRCPlayerApi player = playerObjects[i];

				if (player != null && player.IsValid())
				{
					visualizers[i].transform.position = player.GetPosition();
					visualizers[i].transform.localScale = new Vector3(farDistances[i],
						visualizers[i].transform.localScale.y,
						farDistances[i]);
					visualizers[i].gameObject.SetActive(EnableAudioVisualization);
				}
				else
				{
					visualizers[i].gameObject.SetActive(false);
				}
			}
			else
			{
				visualizers[i].gameObject.SetActive(false);
			}
		}
	}
}
