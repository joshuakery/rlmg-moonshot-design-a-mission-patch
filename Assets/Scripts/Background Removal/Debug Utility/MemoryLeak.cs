using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using TMPro;

public class EventRaiser
{
    private string stringValue;

    public string StringValue
    {
        get { return this.stringValue; }
        set
        {
            if (this.stringValue != value)
            {
                this.stringValue = value;
                if (this.StringValueChanged != null)
                {
                    this.StringValueChanged(this, new EventArgs());
                }
            }
        }
    }

    public event EventHandler StringValueChanged;
}

public class MemoryLeakObject
{
    private byte[] allocatedMemory;
    private Mat allocatedMat;
    private EventRaiser raiser;

    public MemoryLeakObject(EventRaiser raiser)
    {
        this.raiser = raiser;
        //allocate some memory to mimic a real life object
        this.allocatedMemory = new byte[10000000];
        /*        this.allocatedMat = OpenCVForUnity.CoreModule.Mat.zeros(1080, 920, CvType.CV_8UC1);*/
        raiser.StringValueChanged += new EventHandler(raiser_StringValueChanged);
    }

    private void raiser_StringValueChanged(object sender, EventArgs e)
    {

    }
}

public class MemoryLeak : MonoBehaviour
{
    [SerializeField]
    private TMP_Text countDisplay;
    private int count = 0;
    private bool doGenerateMemoryLeaks = false;

    private static EventRaiser raiser;

    // Start is called before the first frame update
    void Start()
    {
        count = 0;

        raiser = new EventRaiser();
    }

    // Update is called once per frame
    void Update()
    {
        if (doGenerateMemoryLeaks)
            GenerateMemoryLeaks();

        long memory = GC.GetTotalMemory(true);
        Debug.Log(String.Format("Memory being used: {0:0,0}", memory));
    }

    public void ToggleDoGenerateMemoryLeaks(bool value)
    {
        doGenerateMemoryLeaks = value;
    }

    private void GenerateMemoryLeaks()
    {
        /* Mat mask = OpenCVForUnity.CoreModule.Mat.zeros(1080, 920, CvType.CV_8UC1);*/
        CreateLeak();
        count++;
        if (countDisplay != null) countDisplay.text = count.ToString();
    }

    private static void CreateLeak()
    {
        MemoryLeakObject memoryLeak = new MemoryLeakObject(raiser);
        memoryLeak = null;
        GC.Collect();

    }
}
