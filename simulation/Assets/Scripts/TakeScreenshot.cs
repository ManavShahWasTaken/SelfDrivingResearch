using UnityEngine;
using System.Collections;
using System.Globalization;
using System.Diagnostics;

public class TakeScreenshot : MonoBehaviour
{
    public int num_counter = 1;

    public string ScreenShotName()
    {
        string str = "Screenshots/scr" + num_counter + ".png";
        return str;
    }

    // void LateUpdate() {
    //     if (num_counter < 5) {
    //         ScreenCapture.CaptureScreenshot(ScreenShotName());
    //         num_counter += 1;
    //     }
    //    else {
    //        num_counter = 1;
    //        ScreenCapture.CaptureScreenshot(ScreenShotName());
    //     }
    // }
}