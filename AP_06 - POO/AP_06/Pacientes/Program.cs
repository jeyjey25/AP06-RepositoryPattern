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
public class Paciente : IEntidade
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; }
    public DateTime DataNascimento { get; set; }
    public string ContatoEmergencia { get; set; }

    public Paciente()
    {
        Id = Guid.NewGuid();
    }
}
public interface IPacienteRepository : IRepository<Paciente>
{
    IEnumerable<Paciente> ObterPorFaixaEtaria(int idadeMinima, int idadeMaxima);
}
public class PacienteJsonRepository : GenericJsonRepository<Paciente>, IPacienteRepository
{
    public IEnumerable<Paciente> ObterPorFaixaEtaria(int idadeMinima, int idadeMaxima)
    {
        List<Paciente> pacientes = ObterTodos();
        return pacientes.Where(p =>
        {
            int idade = DateTime.Now.Year - p.DataNascimento.Year;
            if (DateTime.Now.Month < p.DataNascimento.Month || (DateTime.Now.Month == p.DataNascimento.Month && DateTime.Now.Day < p.DataNascimento.Day))
            {
                idade--;
            }
            return idade >= idadeMinima && idade <= idadeMaxima;
        });
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        IPacienteRepository pacienteRepository = new PacienteJsonRepository();

        while (true)
        {
            Console.WriteLine("\n--- Sistema de Cadastro de Pacientes ---");
            Console.WriteLine("1. Adicionar Paciente");
            Console.WriteLine("2. Listar Pacientes por Faixa Etária");
            Console.WriteLine("3. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarPaciente(pacienteRepository);
                    break;
                case "2":
                    ListarPacientesPorFaixaEtaria(pacienteRepository);
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

    static void AdicionarPaciente(IPacienteRepository pacienteRepository)
    {
        Console.Write("Nome Completo: ");
        string nomeCompleto = Console.ReadLine();

        Console.Write("Data de Nascimento (AAAA-MM-DD): ");
        if (!DateTime.TryParse(Console.ReadLine(), out DateTime dataNascimento))
        {
            Console.WriteLine("Data inválida.");
            return;
        }

        Console.Write("Contato de Emergência: ");
        string contatoEmergencia = Console.ReadLine();

        Paciente paciente = new Paciente
        {
            NomeCompleto = nomeCompleto,
            DataNascimento = dataNascimento,
            ContatoEmergencia = contatoEmergencia
        };

        pacienteRepository.Adicionar(paciente);
        Console.WriteLine("Paciente adicionado com sucesso!");
    }

    static void ListarPacientesPorFaixaEtaria(IPacienteRepository pacienteRepository)
    {
        Console.Write("Idade Mínima: ");
        if (!int.TryParse(Console.ReadLine(), out int idadeMinima))
        {
            Console.WriteLine("Idade inválida.");
            return;
        }

        Console.Write("Idade Máxima: ");
        if (!int.TryParse(Console.ReadLine(), out int idadeMaxima))
        {
            Console.WriteLine("Idade inválida.");
            return;
        }

        IEnumerable<Paciente> pacientesFaixaEtaria = pacienteRepository.ObterPorFaixaEtaria(idadeMinima, idadeMaxima);
        Console.WriteLine("Pacientes na faixa etária:");
        foreach (var paciente in pacientesFaixaEtaria)
        {
            Console.WriteLine($"Nome: {paciente.NomeCompleto}, Data de Nascimento: {paciente.DataNascimento.ToShortDateString()}");
        }
    }
}