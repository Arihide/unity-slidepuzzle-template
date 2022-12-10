using System.Collections;
using UnityEngine;
using TMPro;

[DisallowMultipleComponent]
public class TileObject : MonoBehaviour
{
    public int number;
    public TextMeshPro numberText;

    public void SetNumber(int num)
    {
        number = num;
        numberText.SetText("{0}", num);
    }
}