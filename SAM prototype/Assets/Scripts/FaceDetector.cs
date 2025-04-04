using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FaceDetector : MonoBehaviour
{
    DiceRoll dice;

    private void Awake()
    {
        dice = FindObjectOfType<DiceRoll>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (dice != null)
        {
            if (dice.GetComponent<Rigidbody>().velocity== Vector3.zero)
            {
                dice.diceFaceNum = int.Parse(other.name);
                Debug.Log(int.Parse(other.name));

            }
        }
    }

}
