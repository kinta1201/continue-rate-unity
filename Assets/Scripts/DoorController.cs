using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    public void GoResult()
    {
        SceneManager.LoadScene("Result");
    }
}
