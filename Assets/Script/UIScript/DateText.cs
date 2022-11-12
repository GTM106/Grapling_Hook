using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class DateText : MonoBehaviour
{
    public TextMeshProUGUI Qtext;
    float a_color;
    // Use this for initialization
    void Start()
    {
        //Qtext = GetComponentInChildren<Text>();

        a_color = 0.8f;
    }

    // Update is called once per frame
    void Update()
    {
        DateTime dt = DateTime.Now;
        string AMPM = "a";
        string Mon = "a";

        string TimeString = dt.ToString();
        if (dt.ToString("tt") == "�ߑO")
        {
            AMPM = "AM";
        }
        else
        {
            AMPM = "PM";
        }

        switch (Convert.ToString(dt.Month))
        {
            case "1":
                Mon = "Jan";
                break;
            case "2":
                Mon = "Feb";
                break;
            case "3":
                Mon = "Mar";
                break;
            case "4":
                Mon = "Apr";
                break;
            case "5":
                Mon = "May";
                break;
            case "6":
                Mon = "Jun";
                break;
            case "7":
                Mon = "Jul";
                break;
            case "8":
                Mon = "Aug";
                break;
            case "9":
                Mon = "Sep";
                break;
            case "10":
                Mon = "Oct";
                break;
            case "11":
                Mon = "Nob";
                break;
            case "12":
                Mon = "Dec";
                break;
        }

        string AmPmString = AMPM + "  " + dt.ToString("hh:mm") + Environment.NewLine + Mon + "." + dt.Day + " " + dt.Year; //12���ԕ\����string�^�֕ϊ�

        Qtext.text = AmPmString;

        //�e�L�X�g�̓����x��ύX����
        Qtext.color = new Color(0, 1, 0, a_color);
    }
}
