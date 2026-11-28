
using UnityEngine;

public class MenuButtonRelay : MonoBehaviour
{
    public void Jogar()
    {
        if (Menu.Instance != null)
            Menu.Instance.Jogar();
    }

    public void AbrirOpcoes()
    {
        if (Menu.Instance != null)
            Menu.Instance.AbrirOpcoes();
    }

    public void Sair()
    {
        if (Menu.Instance != null)
            Menu.Instance.Sair();
    }
}
