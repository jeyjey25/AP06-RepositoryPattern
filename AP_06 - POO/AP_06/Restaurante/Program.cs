using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

public interface IEntidade
{
    Guid Id { get; set; }
}

public interface IRepository<T> where T : IEntidade
{
    void Adicionar(T entity);
    T? ObterPorId(Guid id);
    List<T> ObterTodos();
    void Atualizar(T entity);
    bool Remover(Guid id);
}

public class GenericJsonRepository<T> : IRepository<T> where T : class, IEntidade, new()
{
    private readonly string _filePath;

    public GenericJsonRepository()
    {
        string entityName = typeof(T).Name.ToLower();
        _filePath = $"{entityName}s.json";
    }

    public void Adicionar(T entity)
    {
        List<T> entities = ObterTodos();
        entities.Add(entity);
        SalvarEntidades(entities);
    }

    public T? ObterPorId(Guid id)
    {
        List<T> entities = ObterTodos();
        return entities.FirstOrDefault(e => e.Id == id);
    }

    public List<T> ObterTodos()
    {
        if (!File.Exists(_filePath))
        {
            return new List<T>();
        }

        string jsonString = File.ReadAllText(_filePath);
        if (string.IsNullOrEmpty(jsonString))
        {
            return new List<T>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<T>>(jsonString);
        }
        catch (JsonException)
        {
            Console.WriteLine($"Error deserializing entities of type {typeof(T).Name} from JSON. Returning an empty list.");
            return new List<T>();
        }
    }

    public void Atualizar(T entity)
    {
        List<T> entities = ObterTodos();
        int index = entities.FindIndex(e => e.Id == entity.Id);
        if (index != -1)
        {
            entities[index] = entity;
            SalvarEntidades(entities);
        }
    }

    public bool Remover(Guid id)
    {
        List<T> entities = ObterTodos();
        int initialCount = entities.Count;
        entities.RemoveAll(e => e.Id == id);
        if (entities.Count < initialCount)
        {
            SalvarEntidades(entities);
            return true;
        }
        return false;
    }

    private void SalvarEntidades(List<T> entities)
    {
        string jsonString = JsonSerializer.Serialize(entities);
        File.WriteAllText(_filePath, jsonString);
    }
}

[JsonDerivedType(typeof(Prato), typeDiscriminator: "prato")]
[JsonDerivedType(typeof(Bebida), typeDiscriminator: "bebida")]
public abstract class ItemCardapio : IEntidade
{
    public Guid Id { get; set; }
    public string NomeItem { get; set; }
    public decimal Preco { get; set; }

    public ItemCardapio()
    {
        Id = Guid.NewGuid();
    }
}
public class Prato : ItemCardapio
{
    public string DescricaoDetalhada { get; set; }
    public bool Vegetariano { get; set; }
}
public class Bebida : ItemCardapio
{
    public int VolumeMl { get; set; }
    public bool Alcoolica { get; set; }
}
public class Program
{
    public static void Main(string[] args)
    {
        IRepository<ItemCardapio> cardapioRepository = new GenericJsonRepository<ItemCardapio>();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Cardápio de Restaurante ---");
            Console.WriteLine("1. Adicionar Prato");
            Console.WriteLine("2. Adicionar Bebida");
            Console.WriteLine("3. Listar Itens do Cardápio");
            Console.WriteLine("4. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarPrato(cardapioRepository);
                    break;
                case "2":
                    AdicionarBebida(cardapioRepository);
                    break;
                case "3":
                    ListarItensCardapio(cardapioRepository);
                    break;
                case "4":
                    Console.WriteLine("Saindo do sistema...");
                    return;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    static void AdicionarPrato(IRepository<ItemCardapio> cardapioRepository)
    {
        Console.Write("Nome do Prato: ");
        string nomeItem = Console.ReadLine();

        Console.Write("Preço: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal preco))
        {
            Console.WriteLine("Preço inválido.");
            return;
        }

        Console.Write("Descrição Detalhada: ");
        string descricaoDetalhada = Console.ReadLine();

        Console.Write("Vegetariano (S/N): ");
        bool vegetariano = Console.ReadLine().ToUpper() == "S";

        Prato prato = new Prato
        {
            NomeItem = nomeItem,
            Preco = preco,
            DescricaoDetalhada = descricaoDetalhada,
            Vegetariano = vegetariano
        };

        cardapioRepository.Adicionar(prato);
        Console.WriteLine("Prato adicionado com sucesso!");
    }

    static void AdicionarBebida(IRepository<ItemCardapio> cardapioRepository)
    {
        Console.Write("Nome da Bebida: ");
        string nomeItem = Console.ReadLine();

        Console.Write("Preço: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal preco))
        {
            Console.WriteLine("Preço inválido.");
            return;
        }

        Console.Write("Volume (ml): ");
        if (!int.TryParse(Console.ReadLine(), out int volumeMl))
        {
            Console.WriteLine("Volume inválido.");
            return;
        }

        Console.Write("Alcoólica (S/N): ");
        bool alcoolica = Console.ReadLine().ToUpper() == "S";

        Bebida bebida = new Bebida
        {
            NomeItem = nomeItem,
            Preco = preco,
            VolumeMl = volumeMl,
            Alcoolica = alcoolica
        };

        cardapioRepository.Adicionar(bebida);
        Console.WriteLine("Bebida adicionada com sucesso!");
    }

    static void ListarItensCardapio(IRepository<ItemCardapio> cardapioRepository)
    {
        List<ItemCardapio> itensCardapio = cardapioRepository.ObterTodos();
        Console.WriteLine("Itens do Cardápio:");
        foreach (var item in itensCardapio)
        {
            Console.WriteLine($"Nome: {item.NomeItem}, Preço: {item.Preco}, Tipo: {item.GetType().Name}");
            if (item is Prato prato)
            {
                Console.WriteLine($"  Descrição: {prato.DescricaoDetalhada}, Vegetariano: {prato.Vegetariano}");
            }
            else if (item is Bebida bebida)
            {
                Console.WriteLine($"  Volume: {bebida.VolumeMl}ml, Alcoolica: {bebida.Alcoolica}");
            }
        }
    }
}