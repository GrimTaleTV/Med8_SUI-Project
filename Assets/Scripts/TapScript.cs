using UnityEngine;
using UnityEngine.UIElements;

public class TapScript : MonoBehaviour
{
    public int button1Value;
    public int button2Value;
    public int button3Value;

    public int TapVariable;

    void Start()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        Button button1 = root.Q<Button>("Button1");
        Button button2 = root.Q<Button>("Button2");
        Button button3 = root.Q<Button>("Button3");

        //button1.clicked += () => Debug.Log("Button 1 clicked");
        //button2.clicked += () => Debug.Log("Button 2 clicked");
        //button3.clicked += () => Debug.Log("Button 3 clicked");

        button1.clicked += () => TapVariable = button1Value;
        button2.clicked += () => TapVariable = button2Value;
        button3.clicked += () => TapVariable = button3Value;
    }
}
