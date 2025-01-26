using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    public void EndGame() {
        SceneManager.LoadScene("MainMenu");
    }
}
