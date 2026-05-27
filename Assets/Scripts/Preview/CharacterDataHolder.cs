using UnityEngine;

public class CharacterDataHolder : MonoBehaviour
{
    public static CharacterDataHolder Instance { get; private set; }
    public CharacterData DatosActuales { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetData(CharacterData data) => DatosActuales = data;
}