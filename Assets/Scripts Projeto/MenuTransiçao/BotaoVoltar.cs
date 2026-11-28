
using UnityEngine;

public class BotaoVoltar : MonoBehaviour
{
    private void OnMouseDown()
    {
        Debug.Log("[BOTAO] Clique no botão de retorno!");

        if (Menu.Instance != null)
        {
            Menu.Instance.VoltarMenu();
        }
        else
        {
            Debug.LogError("[BOTAO] Nenhuma instância do Menu encontrada!");
        }
    }
}
