
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PodiumInterface : UdonSharpBehaviour
{
    [SerializeField] GameObject mainInterface;
    [SerializeField] GameObject videoPlayerInterface;
    [SerializeField] SlideshowSystem slideshowSystem;

    void Start()
    {
        mainInterface.gameObject.SetActive(true);
    }

    public void ShowVideoPlayerInterface()
	{
        mainInterface.SetActive(false);
        videoPlayerInterface.SetActive(true);
	}

    public void ShowMainMenuInterface()
	{
        mainInterface.SetActive(true);
        videoPlayerInterface.SetActive(false);
        slideshowSystem.HideSlideInterface();
    }

    public void ShowSlideshowInterface()
	{
        mainInterface.SetActive(false);
        videoPlayerInterface.SetActive(false);
        slideshowSystem.ShowSlideInterface();
    }
}
