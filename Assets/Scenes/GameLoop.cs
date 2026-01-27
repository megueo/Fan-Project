using UnityEngine;

public class GameLoop : MonoBehaviour
{
    void Update()
    {
        TotemSystem.Instance.Tick(Time.deltaTime);
    }
}
