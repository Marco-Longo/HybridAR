using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void LoadARCore()
    {
        SceneManager.LoadScene(1);
        //ShowAndroidToastMessage("Load ARCoreApproach Scene");
    }
    
    public void LoadAccelArray()
    {
        SceneManager.LoadScene(2);
        //ShowAndroidToastMessage("Load AccelArray Scene");
    }
    
    public void LoadCumAccel()
    {
        SceneManager.LoadScene(3);
        //ShowAndroidToastMessage("Load CumAccel Scene");
    }

    public void QuitApplication()
    {
        DataWriter.SaveFile(PlayerPrefs.GetString("FILE"));
        Application.Quit();
    }

    private void ShowAndroidToastMessage(string message)
    {
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

        if (unityActivity != null)
        {
            AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
            unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 0);
                toastObject.Call("show");
            }));
        }
    }
}
