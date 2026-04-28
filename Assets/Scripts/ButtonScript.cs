using UnityEngine;
using UnityEngine.UIElements;

public class ButtonScript : MonoBehaviour
{
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
