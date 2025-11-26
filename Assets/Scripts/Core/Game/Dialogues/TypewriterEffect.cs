using TMPro;
using UnityEngine;
using System.Collections;

public class TypewriterEffect : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textUI;

    [SerializeField] private float typeSpeed = 0.05f;

    Coroutine routine;
    public bool IsTyping { get; private set; }
    public bool IsCompleted => !IsTyping;

    private string currentText;
    private int currentIndexCharacter;
    public void Play(string text)
    {
        currentText = text;
        textUI.text = currentText;
        currentIndexCharacter = 0;
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(TypeText());
    }

    public void Skip()
    {
        if (!IsTyping)
            return;

        // Hiện toàn bộ text ngay lập tức
        textUI.text = currentText;

        // Kết thúc tiến trình typewriter
        StopCoroutine(routine);
        IsTyping = false;
    }

    IEnumerator TypeText()
    {
        IsTyping = true;
        textUI.text = "";
        while(currentIndexCharacter < currentText.Length)
        {
            currentIndexCharacter++;
            string text = currentText.Substring(0, currentIndexCharacter);
            text += "<color=#00000000>" + currentText.Substring(currentIndexCharacter) + "</color>";
            textUI.text = text;
            yield return new WaitForSeconds(typeSpeed);
        }
       IsTyping = false;
    }
   
}
