
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PodiumMainMenuPage : UdonSharpBehaviour
{
    [SerializeField] PodiumInterface podiumInterface;

    void Start()
    {
        
    }

    public void ShowVideoPlayerPage()
	{
        podiumInterface.ShowVideoPlayerInterface();
	}

    public void ShowSlideshowInterface()
	{
        podiumInterface.ShowSlideshowInterface();
	}
}
