using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NumberCounter : MonoBehaviour
{
    public TMP_Text numberText;
    public int number = 0;

    public UnityEvent<int> OnNumberChanged;

    void Start()
    {
        SetNumber(0);
    }

    public void ChangeBy(int delta)
    {
        number += delta;
        numberText.text = number.ToString();
        OnNumberChanged?.Invoke(number);
    }
    
    public void SetNumber(int newNumber)
    {
        number = newNumber;
        numberText.text = number.ToString();
        OnNumberChanged?.Invoke(number);
    }
}
