using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
public class Produto
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }
    public int Estoque { get; set; }

    public Produto()
    {
        Id = Guid.NewGuid();
    }
}
public interface IProdutoRepository
{
    void Adicionar(Produto produto);
    Produto? ObterPorId(Guid id);
    List<Produto> ObterTodos();
    void Atualizar(Produto produto);
    bool Remover(Guid id);
}
public class ProdutoJsonRepository : IProdutoRepository
{
    private readonly string _filePath = "produtos.json";

    public void Adicionar(Produto produto)
    {
        List<Produto> produtos = ObterTodos();
        produtos.Add(produto);
        SalvarProdutos(produtos);
    }

    public Produto? ObterPorId(Guid id)
    {
        List<Produto> produtos = ObterTodos();
        return produtos.FirstOrDefault(p => p.Id == id);
    }

    public List<Produto> ObterTodos()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Produto>();
        }

        string jsonString = File.ReadAllText(_filePath);
        if (string.IsNullOrEmpty(jsonString))
        {
            return new List<Produto>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<Produto>>(jsonString);
        }
        catch (JsonException)
        {
            Console.WriteLine("Error deserializing products from JSON. Returning an empty list.");
            return new List<Produto>();
        }
    }

    public void Atualizar(Produto produto)
    {
        List<Produto> produtos = ObterTodos();
        int index = produtos.FindIndex(p => p.Id == produto.Id);
        if (index != -1)
        {
            produtos[index] = produto;
            SalvarProdutos(produtos);
        }
    }

    public bool Remover(Guid id)
    {
        List<Produto> produtos = ObterTodos();
        int initialCount = produtos.Count;
        produtos.RemoveAll(p => p.Id == id);
        if (produtos.Count < initialCount)
        {
            SalvarProdutos(produtos);
            return true;
        }
        return false;
    }

    private void SalvarProdutos(List<Produto> produtos)
    {
        string jsonString = JsonSerializer.Serialize(produtos);
        File.WriteAllText(_filePath, jsonString);
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        IProdutoRepository produtoRepository = new ProdutoJsonRepository();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Catálogo de Produtos ---");
            Console.WriteLine("1. Adicionar Produto");
            Console.WriteLine("2. Listar Produtos");
            Console.WriteLine("3. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarProduto(produtoRepository);
                    break;
                case "2":
                    ListarProdutos(produtoRepository);
                    break;
                case "3":
                    Console.WriteLine("Encerrando sistema...");
                    return;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    static void AdicionarProduto(IProdutoRepository produtoRepository)
    {
        Console.Write("Nome do Produto: ");
        string nome = Console.ReadLine();

        Console.Write("Descrição: ");
        string descricao = Console.ReadLine();

        Console.Write("Preço: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal preco))
        {
            Console.WriteLine("Preço inválido.");
            return;
        }

        Console.Write("Estoque: ");
        if (!int.TryParse(Console.ReadLine(), out int estoque))
        {
            Console.WriteLine("Estoque inválido.");
            return;
        }

        Produto produto = new Produto
        {
            Nome = nome,
            Descricao = descricao,
            Preco = preco,
            Estoque = estoque
        };

        produtoRepository.Adicionar(produto);
        Console.WriteLine("Produto adicionado com sucesso!");
    }

    static void ListarProdutos(IProdutoRepository produtoRepository)
    {
        List<Produto> produtos = produtoRepository.ObterTodos();
        Console.WriteLine("Produtos:");
        foreach (var produto in produtos)
        {
            Console.WriteLine($"ID: {produto.Id}, Nome: {produto.Nome}, Preço: {produto.Preco}, Estoque: {produto.Estoque}");
        }
    }
}