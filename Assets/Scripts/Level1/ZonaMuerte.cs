using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class ZonaMuerte : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
                pc.RecibeDano(Vector2.zero, 99);

        }
}
}