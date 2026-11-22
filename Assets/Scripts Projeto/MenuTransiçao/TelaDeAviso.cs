using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TelaAviso : MonoBehaviour
{
    public void CloseButton()
    {
        SceneManager.LoadScene("MenuInicial");
    }
}