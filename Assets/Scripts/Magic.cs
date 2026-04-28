using UnityEngine;
using UnityEngine.InputSystem;

public class Magic : MonoBehaviour
{
    public int spell;
    public InputActionProperty spellButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void castSpell()
    {
        if (spell == 0)
        {
                Debug.Log("First Spell");
        }
    }
}
