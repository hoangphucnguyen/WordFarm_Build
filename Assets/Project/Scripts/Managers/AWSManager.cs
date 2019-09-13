using System;
using System.Collections;
using System.IO;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using GameFramework.GameObjects.Components;
using UnityEngine;

public class AWSManager : Singleton <AWSManager>  {
    public string IdentityPoolId = "";
    public string CognitoIdentityRegion = RegionEndpoint.USEast1.SystemName;
	private RegionEndpoint _CognitoIdentityRegion
	{
		get { return RegionEndpoint.GetBySystemName(CognitoIdentityRegion); }
	}

    public string S3Region = RegionEndpoint.USEast1.SystemName;
    public string AccessKey = "";
    public string SecretKey = "";
	private RegionEndpoint _S3Region
	{
		get { return RegionEndpoint.GetBySystemName(S3Region); }
	}
	public string S3BucketName = null;

    protected override void GameSetup()
    {
        base.GameSetup();

        UnityInitializer.AttachToGameObject(this.gameObject);

		AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;

		AWSConfigs.LoggingConfig.LogTo = LoggingOptions.UnityLogger;
		AWSConfigs.LoggingConfig.LogResponses = ResponseLoggingOption.Always;
		AWSConfigs.LoggingConfig.LogMetrics = true;
		AWSConfigs.CorrectForClockSkew = true;
    }

	private IAmazonS3 _s3Client;
	private AWSCredentials _credentials;

	private AWSCredentials Credentials
	{
		get
		{
            if (_credentials == null)
                _credentials = new BasicAWSCredentials(AccessKey, SecretKey);

			return _credentials;
		}
	}

	private IAmazonS3 Client
	{
		get
		{
			if (_s3Client == null)
			{
				_s3Client = new AmazonS3Client(Credentials, _S3Region);
			}
			
			return _s3Client;
		}
	}

    public void UploadFile(string fileName, string filePath, Action <bool, string> action = null) {
		var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

		var request = new PostObjectRequest()
		{
			Region = _S3Region,
			Bucket = S3BucketName,
			Key = fileName,
			InputStream = stream,
            CannedACL = S3CannedACL.PublicRead
		};

		Client.PostObjectAsync(request, (responseObj) =>
		{
			if (action != null)
			{
                Debug.Log(responseObj.Exception);
				action(responseObj.Exception == null, fileName);
			}
		});
    }

    public void MakeScreenshot()
    {
        StartCoroutine(TakeScreenShot());
    }

    private string _shareImageUrl;
    private string _shareUrl;
    private string _shareImageName;
    private bool _uploadingScreenshot;

    public IEnumerator TakeScreenShot(string type = "user", string key = "123") // Note - key = user_id or something unique
    {
        yield return new WaitForEndOfFrame();

        Camera cam = Camera.main;

        RenderTexture rt = new RenderTexture(512, 512, 24);
        cam.targetTexture = rt;
        cam.Render();
        RenderTexture.active = rt;

        Texture2D imageOverview = new Texture2D(412, 256, TextureFormat.ARGB32, false);
        imageOverview.ReadPixels(new Rect(50, 256, 512, 512), 0, 0);
        imageOverview.Apply();

        cam.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);

        // Encode texture into PNG
        byte[] bytes = imageOverview.EncodeToPNG();

        // save in memory
        string filename = string.Format("{0}_{1}.png", type, key);
        string urlName = string.Format("{0}_{1}", type, key);

        string folder = Path.Combine(Application.persistentDataPath, "Screenshots");

        if (!Directory.Exists(folder)) { 
            Directory.CreateDirectory(folder); 
        }

        string path = Path.Combine(folder, filename);

        File.WriteAllBytes(path, bytes);

        _shareImageUrl = null; // reset
        _shareImageName = null;
        _shareUrl = null;

        _uploadingScreenshot = true;
        UploadFile(filename, path, (bool success, string fileName) =>
        {
            _uploadingScreenshot = false;

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            _shareImageUrl = string.Format("https://s3.amazonaws.com/minisar/{0}", fileName);

            string rand = ""; // TODO - some random key
            _shareUrl = string.Format("https://minisarpetgame.supersocial.games/2f1957/{0}/{1}", urlName, rand);
            _shareImageName = fileName;
        });
    }
}
