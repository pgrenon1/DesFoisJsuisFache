using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
 
 
public class TakeScreenshot : MonoBehaviour
{
    public AudioSource captureSound;

    // Use this for initialization
    public void Screenshot()
    {
        StartCoroutine(UploadPNG());
        captureSound.Play();
        //Debug.log (encodedText);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            Screenshot();
    }

    IEnumerator UploadPNG()
    {
        PoemHUD.Instance.headerText.enabled = false;
        PoemHUD.Instance.poemText.enabled = false;
        PoemHUD.Instance.screenShotText.enabled = false;
        PoemHUD.Instance.controls.SetActive(false);

        // We should only read the screen after all rendering is complete
        yield return new WaitForEndOfFrame();

        // Create a texture the size of the screen, RGB24 format
        int width = Screen.width;
        int height = Screen.height;
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Read screen contents into the texture
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToPNG();
        Destroy(tex);

        //string ToBase64String byte[]
        string encodedText = System.Convert.ToBase64String(bytes);

        var image_url = "data:image/png;base64," + encodedText;

        Debug.Log(image_url);
#if !UNITY_EDITOR
        //SaveScreenshotWebGL("DesFoisJsuisFache", image_url);
        openWindow(image_url);
#endif

        PoemHUD.Instance.headerText.enabled = true;
        PoemHUD.Instance.poemText.enabled = true;
        PoemHUD.Instance.screenShotText.enabled = true;
        PoemHUD.Instance.controls.SetActive(true);
    }

    [DllImport("__Internal")]
    private static extern void openWindow(string url);

    //[DllImport("__Internal")]
    //private static extern void SaveScreenshotWebGL(string filename, string data);
}
