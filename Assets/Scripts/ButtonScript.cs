using UnityEngine;
using UnityEngine.UIElements;

public class ButtonScript : MonoBehaviour
{
    public int spellVariable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button1 = root.Q<Button>("Button1");
        Button button2 = root.Q<Button>("Button2");
        Button button3 = root.Q<Button>("Button3");

        button1.clicked += () => Debug.Log("Button 1 clicked");
        button2.clicked += () => Debug.Log("Button 2 clicked");
        button3.clicked += () => Debug.Log("Button 3 clicked");

        button1.clicked += () => spellVariable = 1;
        button2.clicked += () => spellVariable = 2;
        button3.clicked += () => spellVariable = 3;
    }
}
