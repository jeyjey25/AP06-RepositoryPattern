using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
public class Departamento : IEntidade
{
    public Guid Id { get; set; }
    public string NomeDepartamento { get; set; }
    public string Sigla { get; set; }

    public Departamento()
    {
        Id = Guid.NewGuid();
    }
}
public class Funcionario : IEntidade
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; }
    public string Cargo { get; set; }
    public Guid DepartamentoId { get; set; } 

    public Funcionario()
    {
        Id = Guid.NewGuid();
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        IRepository<Departamento> departamentoRepository = new GenericJsonRepository<Departamento>();
        IRepository<Funcionario> funcionarioRepository = new GenericJsonRepository<Funcionario>();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Gerenciamento de Funcionários e Departamentos ---");
            Console.WriteLine("1. Adicionar Departamento");
            Console.WriteLine("2. Adicionar Funcionário");
            Console.WriteLine("3. Listar Funcionários");
            Console.WriteLine("4. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarDepartamento(departamentoRepository);
                    break;
                case "2":
                    AdicionarFuncionario(departamentoRepository, funcionarioRepository);
                    break;
                case "3":
                    ListarFuncionarios(departamentoRepository, funcionarioRepository);
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

    static void AdicionarDepartamento(IRepository<Departamento> departamentoRepository)
    {
        Console.Write("Nome do Departamento: ");
        string nomeDepartamento = Console.ReadLine();

        Console.Write("Sigla: ");
        string sigla = Console.ReadLine();

        Departamento departamento = new Departamento
        {
            NomeDepartamento = nomeDepartamento,
            Sigla = sigla
        };

        departamentoRepository.Adicionar(departamento);
        Console.WriteLine("Departamento adicionado com sucesso!");
    }

    static void AdicionarFuncionario(IRepository<Departamento> departamentoRepository, IRepository<Funcionario> funcionarioRepository)
    {
        Console.Write("Nome Completo: ");
        string nomeCompleto = Console.ReadLine();

        Console.Write("Cargo: ");
        string cargo = Console.ReadLine();

        Console.WriteLine("Departamentos disponíveis:");
        List<Departamento> departamentos = departamentoRepository.ObterTodos();
        foreach (var departamento in departamentos)
        {
            Console.WriteLine($"ID: {departamento.Id}, Nome: {departamento.NomeDepartamento}");
        }

        Console.Write("ID do Departamento: ");
        if (!Guid.TryParse(Console.ReadLine(), out Guid departamentoId))
        {
            Console.WriteLine("ID de departamento inválido.");
            return;
        }

        Funcionario funcionario = new Funcionario
        {
            NomeCompleto = nomeCompleto,
            Cargo = cargo,
            DepartamentoId = departamentoId
        };

        funcionarioRepository.Adicionar(funcionario);
        Console.WriteLine("Funcionário adicionado com sucesso!");
    }

    static void ListarFuncionarios(IRepository<Departamento> departamentoRepository, IRepository<Funcionario> funcionarioRepository)
    {
        List<Funcionario> funcionarios = funcionarioRepository.ObterTodos();
        Console.WriteLine("Funcionários:");
        foreach (var funcionario in funcionarios)
        {
            Departamento? departamento = departamentoRepository.ObterPorId(funcionario.DepartamentoId);
            Console.WriteLine($"Nome: {funcionario.NomeCompleto}, Cargo: {funcionario.Cargo}, Departamento: {(departamento != null ? departamento.NomeDepartamento : "N/A")}");
        }
    }
}