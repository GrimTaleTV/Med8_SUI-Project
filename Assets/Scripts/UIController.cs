using System;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public UIDocument uiDocumentOne;
    public UIDocument uiDocumentTwo;
    public Texture2D[] spellIcons; // Array to hold spell icons for UI display

    // References to the ButtonScript and TapScript instances
    public ButtonScript buttonScriptOne;
    public ButtonScript buttonScriptTwo;
    public TapScript tapScript;

    public int spell;
    public int pageNumber;

    // cached UI Toolkit elements
    private Button docOneButton1;
    private Button docOneButton2;
    private Button docOneButton3;

    private Button docTwoButton1;
    private Button docTwoButton2;
    private Button docTwoButton3;

    // cache last applied page to avoid redundant work
    private int lastPageNumber = -1;

    // stored delegates so we can unsubscribe cleanly
    private Action decrementPageHandler;
    private Action incrementPageHandler;

    void Start()
    {
        pageNumber = 0;

        // cache visual elements for both documents
        CacheDocumentButtons();

        if (buttonScriptOne != null)
        {
            // These are for choosing spells.
            buttonScriptOne.button1Value = 1;
            buttonScriptOne.button2Value = 2;
            buttonScriptOne.button3Value = 3;

            // Subscribe page controls (store delegates so we can unsubscribe later)
            decrementPageHandler = () => pageNumber = Mathf.Clamp(pageNumber - 1, 0, 3);
            buttonScriptOne.Button4Clicked += decrementPageHandler;

            // Subscribe spell changes from script one
            buttonScriptOne.Button1Clicked += OnButtonScriptOneClicked;
            buttonScriptOne.Button2Clicked += OnButtonScriptOneClicked;
            buttonScriptOne.Button3Clicked += OnButtonScriptOneClicked;
        }

        if (buttonScriptTwo != null)
        {
            buttonScriptTwo.button1Value = 4;
            buttonScriptTwo.button2Value = 5;
            buttonScriptTwo.button3Value = 6;

            // Subscribe page controls
            incrementPageHandler = () => pageNumber = Mathf.Clamp(pageNumber + 1, 0, 3);
            buttonScriptTwo.Button4Clicked += incrementPageHandler;

            // Subscribe spell changes from script two
            buttonScriptTwo.Button1Clicked += OnButtonScriptTwoClicked;
            buttonScriptTwo.Button2Clicked += OnButtonScriptTwoClicked;
            buttonScriptTwo.Button3Clicked += OnButtonScriptTwoClicked;
        }

        if (tapScript != null)
        {
            // These are for choosing what pages can be shown.
            tapScript.button1Value = 1; // All pages
            tapScript.button2Value = 2; // Only page 1&2
            tapScript.button3Value = 3; // Only page 3&4
        }

        // initial icon sync
        UpdateButtonIcons();
        lastPageNumber = pageNumber;
    }

    void Update()
    {
        if (tapScript != null)
        {
            if (tapScript.TapVariable == 1)
            {
                pageNumber = 0;
                pageNumber = Mathf.Clamp(pageNumber, 0, 3);
            }
            else if (tapScript.TapVariable == 2)
            {
                pageNumber = 0;
                // pages 0..1
                pageNumber = Mathf.Clamp(pageNumber, 0, 1);
            }
            else if (tapScript.TapVariable == 3)
            {
                pageNumber = 2;
                // pages 2..3
                pageNumber = Mathf.Clamp(pageNumber, 2, 3);
            }
        }
        else
        {
            pageNumber = Mathf.Clamp(pageNumber, 0, 3);
        }

        // update page-specific button values (guard nulls)
        if (buttonScriptOne != null)
        {
            if (pageNumber == 0)
            {
                buttonScriptOne.button1Value = 1;
                buttonScriptOne.button2Value = 2;
                buttonScriptOne.button3Value = 3;
            }
            else if (pageNumber == 1)
            {
                buttonScriptOne.button1Value = 7;
                buttonScriptOne.button2Value = 8;
                buttonScriptOne.button3Value = 9;
            }
            else if (pageNumber == 2)
            {
                buttonScriptOne.button1Value = 13;
                buttonScriptOne.button2Value = 14;
                buttonScriptOne.button3Value = 15;
            }
            else if (pageNumber == 3)
            {
                buttonScriptOne.button1Value = 19;
                buttonScriptOne.button2Value = 20;
                buttonScriptOne.button3Value = 21;
            }
        }

        if (buttonScriptTwo != null)
        {
            if (pageNumber == 0)
            {
                buttonScriptTwo.button1Value = 4;
                buttonScriptTwo.button2Value = 5;
                buttonScriptTwo.button3Value = 6;
            }
            else if (pageNumber == 1)
            {
                buttonScriptTwo.button1Value = 10;
                buttonScriptTwo.button2Value = 11;
                buttonScriptTwo.button3Value = 12;
            }
            else if (pageNumber == 2)
            {
                buttonScriptTwo.button1Value = 16;
                buttonScriptTwo.button2Value = 17;
                buttonScriptTwo.button3Value = 18;
            }
            else if (pageNumber == 3)
            {
                buttonScriptTwo.button1Value = 22;
                buttonScriptTwo.button2Value = 23;
                buttonScriptTwo.button3Value = 24;
            }
        }

        // update visual icons only when page changed
        if (pageNumber != lastPageNumber)
        {
            UpdateButtonIcons();
            lastPageNumber = pageNumber;
        }
    }

    // Called when any of buttonScriptOne's 1/2/3 buttons are clicked
    private void OnButtonScriptOneClicked()
    {
        if (buttonScriptOne != null)
            spell = buttonScriptOne.spellVariable;
    }

    // Called when any of buttonScriptTwo's 1/2/3 buttons are clicked
    private void OnButtonScriptTwoClicked()
    {
        if (buttonScriptTwo != null)
            spell = buttonScriptTwo.spellVariable;
    }

    // Cache button elements from UIDocuments to avoid repeated Q() calls.
    private void CacheDocumentButtons()
    {
        if (uiDocumentOne != null)
        {
            var root = uiDocumentOne.rootVisualElement;
            if (root != null)
            {
                docOneButton1 = root.Q<Button>("Button1");
                docOneButton2 = root.Q<Button>("Button2");
                docOneButton3 = root.Q<Button>("Button3");
            }
        }

        if (uiDocumentTwo != null)
        {
            var root = uiDocumentTwo.rootVisualElement;
            if (root != null)
            {
                docTwoButton1 = root.Q<Button>("Button1");
                docTwoButton2 = root.Q<Button>("Button2");
                docTwoButton3 = root.Q<Button>("Button3");
            }
        }
    }

    // Update both documents' three buttons based on current pageNumber.
    // Layout: 6 textures per page. Page N -> indices N*6 .. N*6 + 5.
    // uiDocumentOne receives the first three textures (base + 0..2),
    // uiDocumentTwo receives the next three (base + 3..5).
    private void UpdateButtonIcons()
    {
        int baseIndex = pageNumber * 6;

        ApplyTextureToElement(docOneButton1, baseIndex + 0);
        ApplyTextureToElement(docOneButton2, baseIndex + 1);
        ApplyTextureToElement(docOneButton3, baseIndex + 2);

        ApplyTextureToElement(docTwoButton1, baseIndex + 3);
        ApplyTextureToElement(docTwoButton2, baseIndex + 4);
        ApplyTextureToElement(docTwoButton3, baseIndex + 5);
    }

    private void ApplyTextureToElement(VisualElement ve, int textureIndex)
    {
        if (ve == null) return;

        if (spellIcons != null &&
            textureIndex >= 0 &&
            textureIndex < spellIcons.Length &&
            spellIcons[textureIndex] != null)
        {
            ve.style.backgroundImage = new StyleBackground(spellIcons[textureIndex]);
        }
        else
        {
            // clear background
            ve.style.backgroundImage = new StyleBackground();
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks / dangling delegates
        if (buttonScriptOne != null)
        {
            buttonScriptOne.Button1Clicked -= OnButtonScriptOneClicked;
            buttonScriptOne.Button2Clicked -= OnButtonScriptOneClicked;
            buttonScriptOne.Button3Clicked -= OnButtonScriptOneClicked;
            if (decrementPageHandler != null)
                buttonScriptOne.Button4Clicked -= decrementPageHandler;
        }

        if (buttonScriptTwo != null)
        {
            buttonScriptTwo.Button1Clicked -= OnButtonScriptTwoClicked;
            buttonScriptTwo.Button2Clicked -= OnButtonScriptTwoClicked;
            buttonScriptTwo.Button3Clicked -= OnButtonScriptTwoClicked;
            if (incrementPageHandler != null)
                buttonScriptTwo.Button4Clicked -= incrementPageHandler;
        }
    }
}
