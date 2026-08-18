using TMPro;
using UnityEngine;


public class TroopCountUIHandler : MonoBehaviour
{
    // populate UI elements
    public GameObject buttonCountMode;
    public GameObject buttonCountUp1;
    public GameObject buttonCountUp2;
    public GameObject buttonCountMax;
    public GameObject buttonCountDown1;
    public GameObject buttonCountDown2;
    public GameObject buttonCountMin;

    // public GameObject textfieldTroopCount;

    // sandbox fields
    public int adjustPercentSmall   = 10;
    public int adjustPercentBig     = 25;
    public int adjustExactSmall     = 5;
    public int adjustExactBig       = 25;

    // getter setter
    public int SendValue
    {
        get
        {
            return sendExact_active ? sendExact_value : sendPercent_value;
        }
    }

    public bool SendExactActive
    {
        get
        {
            return sendExact_active;
        }
    }

    public bool SendExactAll
    {
        get
        {
            return sendExact_all;
        }
    }

    // vars
    int sendPercent_value = 50, sendExact_value = 10;
    bool sendExact_all = false;
    bool sendExact_active = false;

    TextMeshProUGUI text_countMode;

    void Start()
    {
        text_countMode = buttonCountMode.GetComponentInChildren<TextMeshProUGUI>();
        UpdateCountMode();
    }

    // Update is called once per frame
    void Update()
    {
        // 

        // update UI
        
    }

    void UpdateCountMode()
    {
        text_countMode.text = (sendExact_active ? $"{sendPercent_value}% > " : $"[{sendPercent_value}%] < ");
        if (sendExact_all)
        {
            text_countMode.text += (sendExact_active ? $"[ALL]" : $"all");
        }
        else
        {
            text_countMode.text += (sendExact_active ? $"[{sendExact_value}x]" : $"{sendExact_value}x");
        }
    }

    void AdjustSendCount(bool decrease = false, bool bigChange = false)
    {
        int sign = (decrease? -1 : 1);
        int delta;
        if (sendExact_active)
        {
            if (sendExact_all)
            {
                sendExact_all = false;
                return;
            }

            delta = (bigChange? adjustExactBig : adjustExactSmall);
            sendExact_value = ((sendExact_value / delta) + sign) * delta;

            if (sendExact_value < 1)
            {
                sendExact_value = 1;
            }
        }
        else
        {
            delta = (bigChange ? adjustPercentBig : adjustPercentSmall);
            sendPercent_value = ((sendPercent_value / delta) + sign) * delta;

            sendPercent_value = Mathf.Clamp(sendPercent_value, 1, 100);
        }


    }

    // For binding with buttons
    public void Button_CountMode()
    {
        sendExact_active = !sendExact_active;
        UpdateCountMode();
    }

    public void Button_CountUp1()
    {
        AdjustSendCount();
        UpdateCountMode();
    }

    public void Button_CountUp2()
    {
        AdjustSendCount(bigChange: true);
        UpdateCountMode();
    }

    public void Button_CountDown1()
    {
        AdjustSendCount(true, false);
        UpdateCountMode();
    }

    public void Button_CountDown2()
    {
        AdjustSendCount(true, true);
        UpdateCountMode();
    }

    public void Button_CountMin()
    {
        if (sendExact_active)
        {
            sendExact_value = 1;
            sendExact_all = false;
        }
        else
        {
            sendPercent_value = 1;
        }
        UpdateCountMode();
    }

    public void Button_CountMax()
    {
        if (sendExact_active)
        {
            sendExact_all = true;
        }
        else
        {
            sendPercent_value = 100;
        }
        UpdateCountMode();
    }
}
