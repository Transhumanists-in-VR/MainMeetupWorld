using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UdonSharp.Video;

public class VideoButton : UdonSharpBehaviour
{
	public VRC.SDKBase.VRCUrl videoURL;
	//public UdonBehaviour playerSyncUdon;
	public USharpVideoPlayer targetVideoPlayer;
	//public VRC.SDK3.Components.VRCUrlInputField urlInputField;
	//public AudioSource HoverSound;
	public AudioSource ClickSource;

	public void PlayVideo()
	{
		Debug.Log("Trying to play video");
		//playerSyncUdon.SendCustomEvent("OnURLChanged");
		targetVideoPlayer.PlayVideo(videoURL);
	}
}