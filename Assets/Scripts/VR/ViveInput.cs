using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViveInput : MonoBehaviour {

    private SteamVR_TrackedObject trackedObj = null;
    public SteamVR_Controller.Device mDevice;
    public Transform fps;
    public bool isTriggerPressed = false;
    void Awake () {
        trackedObj = GetComponent<SteamVR_TrackedObject>();	
	}
	
	// Update is called once per frame
	void Update () {
        mDevice = SteamVR_Controller.Input((int)trackedObj.index);

        //Trigger
        if (mDevice.GetTouchDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            fps.GetComponent<FPSInputController>().isTriggerPressed = true;
            isTriggerPressed = true;
        }

        if (mDevice.GetTouchUp(SteamVR_Controller.ButtonMask.Trigger))
        {
            fps.GetComponent<FPSInputController>().isTriggerPressed = false;
            isTriggerPressed = false;
        }

        Vector2 triggerValue = mDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger);
        
        //Touchpad
        if (mDevice.GetTouchDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            
        }
        Vector2 touchValue = mDevice.GetAxis(Valve.VR.EVRButtonId.k_EButton_SteamVR_Touchpad);

    }
}
