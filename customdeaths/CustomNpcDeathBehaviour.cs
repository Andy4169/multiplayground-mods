using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public class CustomNpcDeathBehaviour : MonoBehaviour
{
    public static int chdMode = 0;
    public static bool lockVar = false;
    public static bool lockVar2 = false;
    public static bool lockVar3 = false;
    public static bool lockVar4 = false;
    public static bool debug = false;
    public static bool cancel = false;
    public static int page = 1;
    public static DialogButton[] ppm = new DialogButton[] { new DialogButton("<<", true, delegate { NextPage(); }) };
    public static DialogButton[] x = new DialogButton[] { new DialogButton("X", true)};
    public static DialogButton[] npm = new DialogButton[] { new DialogButton(">>", true, delegate { PreviousPage(); }) };
    public static DialogButton ph = new DialogButton("placeholder", true, null);
    void Start()
    {

    }
    static void PreviousPage()
    {
        page++;
        Dialog();
    }
    static void NextPage()
    {
        page--;
        Dialog();
    }
    static void schdmode(int mode)
    {
        chdMode = mode;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F7))
        {
            if (!lockVar2)
            {
                lockVar2 = true;
                debug = !debug;
                if (debug)
                {
                    ModAPI.Notify("Debug mode: <color=green>on</color>");
                }
                else
                {
                    ModAPI.Notify("Debug mode: <color=red>off</color>");
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            if (!lockVar4)
            {
                lockVar4 = true;
                if (DialogBox.IsAnyDialogboxOpen)
                {
                    ModAPI.Notify("Return F6");
                    return;
                }
                page = 1;
                Dialog();
            }
        }
        if (Input.GetKeyUp(KeyCode.F7))
        {
            lockVar2 = false;
        }
        if (Input.GetKeyUp(KeyCode.F6))
        {
            lockVar4 = false;
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            if (!lockVar)
            {
                chdMode++;
            }
        }
        if (Input.GetKeyUp(KeyCode.F5))
        {
            lockVar = false;
            if (!cancel)
            {
                if (chdMode >= 13)
                {
                    chdMode = 0;
                    ModAPI.Notify("disabled custom deaths");
                }
                if (chdMode == 1)
                {
                    ModAPI.Notify("Mode:slice");
                }
                if (chdMode == 2)
                {
                    ModAPI.Notify("Mode:crush");
                }
                if (chdMode == 3)
                {
                    ModAPI.Notify("Mode:break bone");
                }
                if (chdMode == 4)
                {
                    ModAPI.Notify("Mode:freeze");
                }
                if (chdMode == 5)
                {
                    ModAPI.Notify("Mode:spark");
                }
                if (chdMode == 6)
                {
                    ModAPI.Notify("Mode:light");
                }
                if (chdMode == 7)
                {
                    ModAPI.Notify("Mode:zombie");
                }
                if (chdMode == 8)
                {
                    ModAPI.Notify("Mode:explode");
                }
                if (chdMode == 9)
                {
                    ModAPI.Notify("Mode:connected");
                }
                if (chdMode == 10)
                {
                    ModAPI.Notify("Mode:weightless");
                }
                if (chdMode == 11)
                {
                    ModAPI.Notify("Mode:fire");
                }
                if (chdMode == 12)
                {
                    ModAPI.Notify("Mode:soft");
                }
            }
            cancel = false;
        }
    }
    public static void Dialog()
    {
        if (page >= 7)
        {
            page = 1;
        }
        if(page <= 0)
        {
            page = 6;
        }
        DialogButton[] display = new DialogButton[] { };

        display = display.Concat(x).ToArray();
        display = display.Concat(ppm).ToArray();
        if (page == 1)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Slice", true, delegate { schdmode(1); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Crush", true, delegate { schdmode(2); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        if (page == 2)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Break Bone", true, delegate { schdmode(3); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Freeze", true, delegate { schdmode(4); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        if (page == 3)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Spark", true, delegate { schdmode(5); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Light", true, delegate { schdmode(6); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        if (page == 4)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Zombie", true, delegate { schdmode(7); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Explode", true, delegate { schdmode(8); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        if (page == 5)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Connected", true, delegate { schdmode(9); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Weightless", true, delegate { schdmode(10); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        if (page == 6)
        {
            display = display.Concat(new DialogButton[] { new DialogButton("Fire", true, delegate { schdmode(11); }) }).ToArray();
            display = display.Concat(new DialogButton[] { new DialogButton("Soft", true, delegate { schdmode(12); }) }).ToArray();
            display = display.Concat(npm).ToArray();
        }
        DialogBoxManager.Dialog("Select custom death mode", display);
    }
}