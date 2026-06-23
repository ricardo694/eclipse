using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class Checpoint : MonoBehaviour
{
    public bool checkpointActivado = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D coliision)
    {
        if (coliision.CompareTag("Player"))
        {
            ActivarCheckpoint();
        }
    }

    void ActivarCheckpoint()
    {
        if (checkpointActivado) return; 

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ActualizarCheckpoint(transform.position, this);

            checkpointActivado = true;

            if (animator != null)
            {
                animator.SetTrigger("Activar");
            }
        }
    }

    
    
}