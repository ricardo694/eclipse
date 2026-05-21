using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxMovement : MonoBehaviour
{
    Transform cam; // Main Camera
    Vector3 camStartPos;
    
    // Ahora guardamos la distancia en ambos ejes (X e Y)
    Vector2 distance; 

    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;

    [Range(0.01f, 1f)]
    public float parallaxSpeed;
    
    // OPCIONAL: Por si quieres que el fondo vertical se mueva más lento o más rápido que el horizontal
    [Range(0.01f, 1f)]
    public float verticalParallaxModifier = 0.5f; 

    void Start()
    {
        cam = Camera.main.transform;
        camStartPos = cam.position;

        int backCount = transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = transform.GetChild(i).gameObject;
            mat[i] = backgrounds[i].GetComponent<Renderer>().material;
        }

        BackSpeedCalculate(backCount);
    }

    void BackSpeedCalculate(int backCount)
    {
        for (int i = 0; i < backCount; i++) 
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) 
        {
            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void LateUpdate()
    {
        // 1. Calculamos la distancia que se ha movido la cámara en ambos ejes
        distance.x = cam.position.x - camStartPos.x;
        distance.y = cam.position.y - camStartPos.y;

        // 2. CORRECCIÓN CLAVE: El fondo ahora sigue a la cámara en X y en Y
        // Eliminé el "- 1" para que el fondo se mantenga perfectamente centrado con tu cámara
        transform.position = new Vector3(cam.position.x, cam.position.y, transform.position.z);

        // 3. Aplicamos el movimiento a las texturas
        for (int i = 0; i < backgrounds.Length; i++)
        {
            float speed = backSpeed[i] * parallaxSpeed;
            
            // Calculamos el desfase para X y para Y
            float offsetX = distance.x * speed;
            float offsetY = distance.y * speed * verticalParallaxModifier;

            mat[i].SetTextureOffset("_MainTex", new Vector2(offsetX, offsetY));
        }
    }
}