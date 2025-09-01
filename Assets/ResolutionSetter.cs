using UnityEngine;

public class ResolutionSetter : MonoBehaviour
{
    void Start()
    {
        Screen.SetResolution(640, 480, true); // false = ventana
    }
}
