using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CadastroProdutos : MonoBehaviour
{
    [Header("Inputs")]
    public TMP_InputField inputNome, inputValidade, inputQuantidade;

    [Header("Lista de Textos")]
    public List<TMP_Text> listaDeTextos = new List<TMP_Text>();


    [Header("Lista de Classes")]
    public List<Produto> listaDeProdutos = new List<Produto>();

    

    public void CadastrarProduto()
    {
        if(!string.IsNullOrWhiteSpace(inputNome.text + inputValidade.text + inputQuantidade.text))
        {
            int qtd = int.Parse(inputQuantidade.text);
            Produto newProduto = new Produto(inputNome.text, inputValidade.text , qtd);

            listaDeProdutos.Add(newProduto);

            inputNome.text = "";
            inputValidade.text = "";
            inputQuantidade.text = "";
            
            return;
        }

        print("error");
        
    }

    public void MostrarListaDeProdutos()
    {
        foreach (TMP_Text item in listaDeTextos)
        {
            item.text = "";
            item.gameObject.SetActive(false);
        }

        for(int i = 0; i < listaDeProdutos.Count; i++)
        {
            listaDeTextos[i].text = "Produto: " + listaDeProdutos[i].name + "       Validade: " + listaDeProdutos[i].validade + "       Quantidade: " + listaDeProdutos[i].quantidade.ToString(); 
            listaDeTextos[i].gameObject.SetActive(true);
        }

    }
}

[System.Serializable]
public class Produto
{
    public string name, validade;
    public int quantidade;

    public Produto(string _name, string _validade, int _quantidade)
    {
        name = _name;
        validade = _validade;
        quantidade = _quantidade;
    }
}
