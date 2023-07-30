
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Image;
using VRC.SDKBase;
using VRC.Udon;

public class SmartPictureFrame : UdonSharpBehaviour
{
    [SerializeField] VRCUrl[] imageURLs;
    [SerializeField] MeshRenderer meshRenderer;
    VRCImageDownloader imageDownloader;
    TextureInfo textureInfo;
    Texture2D loadedImage;
    int previousIndex = -1;
    float largeDimension;

    const float pictureDuratiuon = 5;
    float pictureTimer = 0;

    void Start()
    {
        largeDimension = meshRenderer.transform.localScale.x;
        CycleImage();
    }

    void CycleImage()
	{
        int newIndex = Random.Range(0, imageURLs.Length);

        if (newIndex == previousIndex) newIndex = Random.Range(0, imageURLs.Length);

        VRCUrl selectedURL = imageURLs[newIndex];

        if (imageDownloader != null)
        {
            imageDownloader.Dispose();
            imageDownloader = null;
        }

        imageDownloader = new VRCImageDownloader();

        imageDownloader.DownloadImage(selectedURL, meshRenderer.material,
            (VRC.Udon.Common.Interfaces.IUdonEventReceiver)this/*, textureInfo*/);
        pictureTimer = pictureDuratiuon;
	}

	private void Update()
	{
        pictureTimer -= Time.deltaTime;
        if(pictureTimer < 0)
		{
            CycleImage();
		}
	}

	public override void OnImageLoadSuccess(IVRCImageDownload download)
	{
        if (loadedImage) loadedImage = null; // allow garbage collection

        Debug.Log("On image load success");
        loadedImage = download.Result;
        float aspect = (float)loadedImage.width / (float)loadedImage.height;

        Debug.Log(string.Format("width {0}, height {1}, aspect {2}",
            loadedImage.width, loadedImage.height, aspect));

        float width = largeDimension;
        float height = largeDimension;

        if(loadedImage.width > loadedImage.height)
		{
            height = (aspect > 1) ? largeDimension / aspect : largeDimension * aspect;
            width = largeDimension;
        }
        else
		{
            height = largeDimension;
            width = (aspect > 1) ? largeDimension / aspect : largeDimension * aspect;
        }

        meshRenderer.transform.localScale = new Vector3(width, height, 1);
        //meshRenderer.material.mainTexture = download.Result;
	}

    public override void OnImageLoadError(IVRCImageDownload download)
	{
        Debug.LogError(download.ErrorMessage);
	}
}
