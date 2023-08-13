using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BookManager : MonoBehaviour
{
    public void LoadBook()
    {
        AuthManager.Instance.Destroy();
        //이건 혼자만 사용하는 것이므로
        SceneManager.LoadScene("AuthScene");
    }

    public void Paper() 
    {
        AuthManager.Instance.audioManager.PlaySfx(AudioManager.Sfx.Paper, true);
    }
}
