
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.StringLoading;
using VRC.SDK3.Data;
using VRC.Udon.Common.Interfaces;

public class SlideshowSystem : UdonSharpBehaviour
{
    [SerializeField] GameObject container;
    [SerializeField] GameObject presentationInterface;
    [SerializeField] GameObject loadingInterface;
    [SerializeField] Color defaultLabelColor;
    [SerializeField] UnityEngine.UI.Text loadButtonLabel;
    [SerializeField] GameObject startPresentationButton;

    [SerializeField] VRCUrl[] slides;
    [SerializeField] VRCUrl slideInfoUrl;
    DataDictionary slideInfoDictionary;

    bool hasAttemptedLoad = false;
    int numberOfSlides;
    bool presentationIsAvailable;

    [SerializeField] TMPro.TextMeshProUGUI mainPageStatusLabel;
    [SerializeField] TMPro.TextMeshProUGUI loadpageStatusLabel;

    const string isAvailableKey = "IsAvailable";
    const string slideCountKey = "SlideCount";

    string downloadedString;
    
    void Start()
    {
        
    }

    public void DownloadSlideInfo()
	{
        VRCStringDownloader.LoadUrl(slideInfoUrl, (IUdonEventReceiver)this);
        Debug.Log("String downloading started");
	}

	public override void OnStringLoadSuccess(IVRCStringDownload result)
	{
		base.OnStringLoadSuccess(result);
        Debug.Log("String download success");

        downloadedString = result.Result;
        DataToken resultToken;
        bool success = VRCJson.TryDeserializeFromJson(downloadedString, out resultToken);

        if(success && resultToken.TokenType == TokenType.DataDictionary)
		{
            slideInfoDictionary = (DataDictionary)resultToken;

            SetupSlides();
        }
        else
		{
            SetError("Error parsing slide information");
        }
	}

    public void HideSlideInterface()
	{
        container.SetActive(false);
	}

    public void ShowSlideInterface()
	{
        if(!hasAttemptedLoad || numberOfSlides == 0 ||
            !presentationIsAvailable)
		{
            ShowLoadingInterface();
		}
        else
		{
            StartPresentation();
		}

        container.SetActive(true);
	}

	void SetError(string error)
	{
        loadpageStatusLabel.text = error;
        loadpageStatusLabel.color = Color.red;
    }

    void DoneLoadingSlideInfo()
	{
        hasAttemptedLoad = true;
        loadpageStatusLabel.color = defaultLabelColor;

        if (presentationIsAvailable)
		{
            loadpageStatusLabel.text = string.Format("{0} slides loaded", numberOfSlides);
            startPresentationButton.SetActive(true);
		}
        else
		{
            loadpageStatusLabel.text = "no slides available.";
		}

        loadButtonLabel.text = "Reload presentation";
    }

    public void ShowLoadingInterface()
	{
        loadingInterface.SetActive(true);
        presentationInterface.SetActive(false);
    }

    public void StartPresentation()
	{
        loadingInterface.SetActive(false);
        presentationInterface.SetActive(true);
    }

    void SetupSlides()
    {
        if(slideInfoDictionary.ContainsKey(isAvailableKey))
		{
            // try parsing our isactive flag
            DataToken isAvailableToken;
            bool availableTokenFetched = slideInfoDictionary.TryGetValue(isAvailableKey, out isAvailableToken);
            if(availableTokenFetched)
			{
                if(isAvailableToken.TokenType == TokenType.Boolean)
				{
                    presentationIsAvailable = isAvailableToken.Boolean;

                    // try getting slide  count

                    DataToken slideCountToken;
                    bool slideCountTokenFetched = slideInfoDictionary.TryGetValue(slideCountKey,
                        out slideCountToken);

                    if(slideCountTokenFetched)
					{
                        if(slideCountToken.TokenType == TokenType.Double)
						{
                            double slideCountDouble = slideCountToken.Double;
                            numberOfSlides = (int)slideCountDouble;
                        }
                        else
						{
                            SetError("Slide count token was not type double");
						}
					}
                    else
					{
                        SetError("Slide count token not found in dictionary");
					}

                    DoneLoadingSlideInfo();
				}
                else
				{
                    SetError("IsAvailable token type was not boolean. Ya dun goofed");
				}
			}
            else
			{
                SetError("IsAvailable token could not be fetched from json");
			}
		}
        else
		{
            SetError("Invalid slide presentation");
        }
    }

    public override void OnStringLoadError(IVRCStringDownload result)
	{
        Debug.Log("String download error");
		base.OnStringLoadError(result);
        SetError(result.Error);
	}
}
