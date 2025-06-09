using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

public interface IEntidade
{
    Guid Id { get; set; }
}
public class Musica : IEntidade
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Artista { get; set; }
    public string Album { get; set; }
    public TimeSpan Duracao { get; set; }

    public Musica()
    {
        Id = Guid.NewGuid();
    }
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
public class Program
{
    public static void Main(string[] args)
    {
        IRepository<Musica> musicaRepository = new GenericJsonRepository<Musica>();

        while (true)
        {
            Console.WriteLine("\n--- Biblioteca de Músicas Pessoais ---");
            Console.WriteLine("1. Adicionar Música");
            Console.WriteLine("2. Listar Músicas");
            Console.WriteLine("3. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarMusica(musicaRepository);
                    break;
                case "2":
                    ListarMusicas(musicaRepository);
                    break;
                case "3":
                    Console.WriteLine("Saindo do sistema...");
                    return;
                default:
                    Console.WriteLine("Opção inválida.");
                    break;
            }
        }
    }

    static void AdicionarMusica(IRepository<Musica> musicaRepository)
    {
        Console.Write("Título: ");
        string titulo = Console.ReadLine();

        Console.Write("Artista: ");
        string artista = Console.ReadLine();

        Console.Write("Álbum: ");
        string album = Console.ReadLine();

        Console.Write("Duração (mm:ss): ");
        if (!TimeSpan.TryParseExact(Console.ReadLine(), "mm\\:ss", null, out TimeSpan duracao))
        {
            Console.WriteLine("Duração inválida. Use o formato mm:ss.");
            return;
        }

        Musica musica = new Musica
        {
            Titulo = titulo,
            Artista = artista,
            Album = album,
            Duracao = duracao
        };

        musicaRepository.Adicionar(musica);
        Console.WriteLine("Música adicionada com sucesso!");
    }

    static void ListarMusicas(IRepository<Musica> musicaRepository)
    {
        List<Musica> musicas = musicaRepository.ObterTodos();
        Console.WriteLine("Músicas:");
        foreach (var musica in musicas)
        {
            Console.WriteLine($"ID: {musica.Id}, Título: {musica.Titulo}, Artista: {musica.Artista}, Álbum: {musica.Album}, Duração: {musica.Duracao}");
        }
    }
}
