﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Radial Menu
///  - Pass in array of string
///  - Dynamically create buttons
///     - layout buttons
///     - name buttons
///  - Returns index
/// </summary>
/// 
[RequireComponent(typeof(Button))]
public class RadialMenu : MonoBehaviour
{
    public enum State
    {
        Deactivated,
        Activating,
        Active,
        Deactivating,
    }

    public State m_State = State.Deactivated;

    RadialLayout m_RadLayout;
    public Button m_ButtonPrefab;

    public string m_MenuName = "Menu";
    TMPro.TMP_Text m_MenuText;

    Button MainButton;

    // List of buttons
    public List<Button> m_Buttons = new List<Button>();

    public float m_DeadZone = 30;

    public string[] m_ButtonsNames;

    public float m_Radius = 100;
    public float m_ButtonSize = 60;

    bool m_Active = false;
    public bool m_DisplaySelectedName = false;

    float m_TargetDistance = 0;

    public float m_Smoothing = 8;

    public int m_SelectedIndex = 0;
    bool m_OptionSelected = false;

    public float m_StartAngle = 0;
    public float m_AngleRange = 360;

    [System.Serializable]
    public class SelectionEvent : UnityEvent<int> { }
    public SelectionEvent OnSelected;

    // int m_MaxNumberOfButtons
    bool initialized = false;
    public bool closeOnDeactivate = true;
	void Start () 
    {
        closeOnDeactivate = true;

	}

    void Update()
    {
      

        if( m_State == State.Activating )
        {
            m_RadLayout.UpdateFDistance(Mathf.Lerp(m_RadLayout.fDistance, m_TargetDistance, Time.deltaTime * m_Smoothing));

             float norm = m_RadLayout.fDistance / m_Radius;
             SetButtonFade( norm * norm );

             if (Mathf.Abs(m_RadLayout.fDistance - m_TargetDistance) < 2)
             {
                 m_State = State.Active;
             }

           /*  if (Input.GetMouseButtonUp(0))
                 DeactivateMenu();*/
        }
        else if (m_State == State.Active )
        {
            if (Vector3.Distance(Input.mousePosition, transform.position) < m_DeadZone)
            {
                m_MenuText.text = m_MenuName;
                m_OptionSelected = false;
            }
            else
            {
               // m_SelectedIndex = FindClosestButtonIndex();
                m_MenuText.text = m_ButtonsNames[m_SelectedIndex];
                m_OptionSelected = true;
            }

        }
        else if (m_State == State.Deactivating )
        {
            float norm = ( m_Radius - m_RadLayout.fDistance ) / m_Radius;
            norm = 1 - norm;
            SetButtonFade(norm * norm);

            m_RadLayout.UpdateFDistance(Mathf.Lerp(m_RadLayout.fDistance, m_TargetDistance, Time.deltaTime * m_Smoothing));

            if (Mathf.Abs(m_RadLayout.fDistance - m_TargetDistance) < 2)
            {
                m_State = State.Deactivated;
                for (int i = 0; i < m_ButtonsNames.Length; i++)
                {
                    m_Buttons[i].gameObject.SetActive(false);
                }
            }
        } else if(m_State == State.Deactivated && closeOnDeactivate)
        {
            gameObject.SetActive(false);
        }


    void SetButtonFade( float fade )
    {
        for (int i = 0; i < m_ButtonsNames.Length; i++)
        {
            ColorBlock colBlock = m_Buttons[i].colors;
           // colBlock.normalColor.a = norm;

            Color currentCol = colBlock.normalColor;
            currentCol.a = fade;
            colBlock.normalColor = currentCol;

            m_Buttons[i].colors = colBlock;

            Color textCol = m_Buttons[i].GetComponentInChildren<TMPro.TMP_Text>().color;
            textCol.a = fade;
            m_Buttons[i].GetComponentInChildren<TMPro.TMP_Text>().color = textCol;
        }
    }
	
	// Update is called once per frame
	public void GenerateMenu ( string menuName, string[] buttonsNames ) 
    {

        if (!initialized)
        {
            init();
        }
        m_MenuName = menuName;
        //m_MenuText.text = menuName;
        m_ButtonsNames = buttonsNames;
        if( m_ButtonsNames.Length > m_Buttons.Count )
        {
            int newBtnCount = m_ButtonsNames.Length - m_Buttons.Count;
            for (int i = 0; i < newBtnCount; i++)
            {
                m_Buttons.Add(CreateNewButton());
            }
        }

        // Set buttons names and set to active
        for (int i = 0; i < m_ButtonsNames.Length; i++)
        {
            Button b = m_Buttons[i];
            b.gameObject.SetActive(true);
            m_Buttons[i].GetComponentInChildren<TMPro.TMP_Text>().text = m_ButtonsNames[i];
            b.gameObject.SetActive(false);
        }
	}

    void init()
    {
        MainButton = GetComponent<Button>();
        m_MenuText = GetComponentInChildren<TMPro.TMP_Text>();
        m_MenuText.text = m_MenuName;

        m_RadLayout = new GameObject("Radial Layout").AddComponent<RadialLayout>();
        m_RadLayout.transform.SetParent(transform);
        m_RadLayout.transform.localPosition = Vector3.zero;
        m_RadLayout.MaxAngle = m_AngleRange;
        m_RadLayout.StartAngle = m_StartAngle;
        m_RadLayout.transform.localScale = new Vector3(1, 1, 1);

        int index = m_RadLayout.transform.GetSiblingIndex();
        m_RadLayout.transform.SetSiblingIndex(index - 1);
        initialized = true;
    }


    public void ActivateMenu()
    {
        print("Menu activated");

        for (int i = 0; i < m_ButtonsNames.Length; i++)
        {
            m_Buttons[i].gameObject.SetActive(true);
        }

        m_State = State.Activating;

        
        m_TargetDistance = 250;
        m_Active = true;

    }

   public void DeactivateMenu()
    {
        if (m_OptionSelected)
            OnSelected.Invoke(m_SelectedIndex);

        m_State = State.Deactivating;

     /*   if ( !m_DisplaySelectedName )
            m_MenuText.text = m_MenuName;*/

        print("Deactivating,    Name set to : " + m_MenuText.text);

        m_TargetDistance = 0;
    }

    Button CreateNewButton()
    {
        Button newBtn = Instantiate(m_ButtonPrefab) as Button;
        newBtn.transform.SetParent(m_RadLayout.transform);
        newBtn.gameObject.SetActive(false);
        newBtn.transform.localScale = new Vector3(2f, 2f, 2f);
        newBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, m_ButtonSize);
        newBtn.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, m_ButtonSize);

        newBtn.name = "Rad Btn " + m_Buttons.Count;

        return newBtn;
    }
}
