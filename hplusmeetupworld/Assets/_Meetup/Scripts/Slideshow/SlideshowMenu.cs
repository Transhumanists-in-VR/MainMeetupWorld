
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

/// <summary>
/// This is for a UI canvas slideshow,
/// not the presentation room slideshow
/// </summary>
public class SlideshowMenu : UdonSharpBehaviour
{
	public GameObject[] slides;
	int slideIndex=0;

	public GameObject NextButton;
	public GameObject PreviousButton;

    void Start()
    {
		slides[slideIndex].gameObject.SetActive(true);
		SetButtons();
    }

	void SetButtons()
	{
		PreviousButton.SetActive(slideIndex > 0);
		NextButton.SetActive(slideIndex < slides.Length - 1);
	}

	void SetActiveSlide()
	{
		for(int i=0; i < slides.Length; i++)
		{
			slides[i].SetActive(i == slideIndex);
		}
	}

	public void NextSlide()
	{
		slideIndex = Mathf.Clamp(slideIndex + 1, 0, slides.Length);
		SetActiveSlide();
		SetButtons();
	}

	public void PreviousSlide()
	{
		slideIndex = Mathf.Clamp(slideIndex - 1, 0, slides.Length);
		SetActiveSlide();
		SetButtons();
	}
}
