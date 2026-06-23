using UnityEngine;

namespace EclipseraGlitch
{

    public class AttackHitbox : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            ComboPatchBlock patchBlock = other.GetComponent<ComboPatchBlock>();
            if (patchBlock != null)
            {
                patchBlock.Patch();
            }

           
        }
    }
}