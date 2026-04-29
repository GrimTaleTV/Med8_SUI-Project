using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonScript : MonoBehaviour
{
    public int spellVariable;

    public int button1Value;
    public int button2Value;
    public int button3Value;
    public int button4Value;

    // Events other scripts can subscribe to when each button is clicked
    public event Action Button1Clicked;
    public event Action Button2Clicked;
    public event Action Button3Clicked;
    public event Action Button4Clicked;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button1 = root.Q<Button>("Button1");
        Button button2 = root.Q<Button>("Button2");
        Button button3 = root.Q<Button>("Button3");
        Button button4 = root.Q<Button>("Button4");

        button1.clicked += () =>
        {
            spellVariable = button1Value;
            Button1Clicked?.Invoke();
        };

        button2.clicked += () =>
        {
            spellVariable = button2Value;
            Button2Clicked?.Invoke();
        };

        button3.clicked += () =>
        {
            spellVariable = button3Value;
            Button3Clicked?.Invoke();
        };

        button4.clicked += () =>
        {
            spellVariable = button4Value;
            Button4Clicked?.Invoke();
        };
    }
}
