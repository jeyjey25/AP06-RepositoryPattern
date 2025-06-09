using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
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
public class EquipamentoTI : IEntidade
{
    public Guid Id { get; set; }
    public string NomeEquipamento { get; set; }
    public string TipoEquipamento { get; set; }
    public string NumeroSerie { get; set; }
    public DateTime DataAquisicao { get; set; }

    public EquipamentoTI()
    {
        Id = Guid.NewGuid();
    }
}
public interface IEquipamentoTIRepository : IRepository<EquipamentoTI>
{
}
public class EquipamentoTIJsonRepository : GenericJsonRepository<EquipamentoTI>, IEquipamentoTIRepository
{
}
public class Program
{
    public static void Main(string[] args)
    {
        IEquipamentoTIRepository equipamentoRepository = new EquipamentoTIJsonRepository();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Inventário de Equipamentos de TI ---");
            Console.WriteLine("1. Adicionar Equipamento");
            Console.WriteLine("2. Listar Equipamentos");
            Console.WriteLine("3. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarEquipamento(equipamentoRepository);
                    break;
                case "2":
                    ListarEquipamentos(equipamentoRepository);
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

    static void AdicionarEquipamento(IEquipamentoTIRepository equipamentoRepository)
    {
        Console.Write("Nome do Equipamento: ");
        string nomeEquipamento = Console.ReadLine();

        Console.Write("Tipo do Equipamento: ");
        string tipoEquipamento = Console.ReadLine();

        Console.Write("Número de Série: ");
        string numeroSerie = Console.ReadLine();

        Console.Write("Data de Aquisição (AAAA-MM-DD): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataAquisicao))
        {
            Console.WriteLine("Data inválida.");
            return;
        }

        EquipamentoTI equipamento = new EquipamentoTI
        {
            NomeEquipamento = nomeEquipamento,
            TipoEquipamento = tipoEquipamento,
            NumeroSerie = numeroSerie,
            DataAquisicao = dataAquisicao
        };

        equipamentoRepository.Adicionar(equipamento);
        Console.WriteLine("Equipamento adicionado com sucesso!");
    }

    static void ListarEquipamentos(IEquipamentoTIRepository equipamentoRepository)
    {
        List<EquipamentoTI> equipamentos = equipamentoRepository.ObterTodos();
        Console.WriteLine("Equipamentos de TI:");
        foreach (var equipamento in equipamentos)
        {
            Console.WriteLine($"Nome: {equipamento.NomeEquipamento}, Tipo: {equipamento.TipoEquipamento}, Número de Série: {equipamento.NumeroSerie}, Data de Aquisição: {equipamento.DataAquisicao.ToShortDateString()}");
        }
    }
}