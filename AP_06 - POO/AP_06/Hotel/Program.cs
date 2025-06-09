using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

enum StatusReserva
{
    Pendente = 1,
    Confirmada,
    Cancelada,
    Finalizada
}

class ReservaHotel
{
    public Guid Id { get; set; }
    public string Cliente { get; set; }
    public DateTime DataEntrada { get; set; }
    public DateTime DataSaida { get; set; }
    public StatusReserva Status { get; set; }

    public ReservaHotel()
    {
        Id = Guid.NewGuid();
    }
}

interface IRepositorioGenerico<T>
{
    void Inserir(T entidade);
    IEnumerable<T> ObterTodos();
    void Salvar();
}

interface IReservaHotelRepository : IRepositorioGenerico<ReservaHotel>
{
    IEnumerable<ReservaHotel> ObterPorStatus(StatusReserva status);
}

class ReservaHotelJsonRepository : IReservaHotelRepository
{
    private readonly string _caminhoArquivo = "reservas.json";
    private List<ReservaHotel> _reservas;

    public ReservaHotelJsonRepository()
    {
        if (File.Exists(_caminhoArquivo))
        {
            string json = File.ReadAllText(_caminhoArquivo);
            _reservas = JsonSerializer.Deserialize<List<ReservaHotel>>(json)
                        ?? new List<ReservaHotel>();
        }
        else
        {
            _reservas = new List<ReservaHotel>();
        }
    }

    public void Inserir(ReservaHotel reserva)
    {
        _reservas.Add(reserva);
        Salvar();
    }

    public IEnumerable<ReservaHotel> ObterTodos() => _reservas;

    public IEnumerable<ReservaHotel> ObterPorStatus(StatusReserva status) =>
        _reservas.Where(r => r.Status == status);

    public void Salvar()
    {
        var json = JsonSerializer.Serialize(_reservas, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_caminhoArquivo, json);
    }
}

class Program
{
    static void Main()
    {
        IReservaHotelRepository repositorio = new ReservaHotelJsonRepository();

        while (true)
        {
            Console.WriteLine("\n=== SISTEMA DE RESERVAS ===");
            Console.WriteLine("1. Inserir nova reserva");
            Console.WriteLine("2. Consultar reservas por status");
            Console.WriteLine("3. Sair");
            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            if (opcao == "1")
            {
                InserirReserva(repositorio);
            }
            else if (opcao == "2")
            {
                ConsultarPorStatus(repositorio);
            }
            else if (opcao == "3")
            {
                break;
            }
            else
            {
                Console.WriteLine("Opção inválida.");
            }
        }
    }

    static void InserirReserva(IReservaHotelRepository repo)
    {
        Console.Write("Nome do cliente: ");
        string cliente = Console.ReadLine();

        Console.Write("Data de entrada (yyyy-mm-dd): ");
        DateTime entrada = DateTime.Parse(Console.ReadLine());

        Console.Write("Data de saída (yyyy-mm-dd): ");
        DateTime saida = DateTime.Parse(Console.ReadLine());

        Console.WriteLine("Status:");
        foreach (var valor in Enum.GetValues<StatusReserva>())
        {
            Console.WriteLine($"{(int)valor}. {valor}");
        }
        Console.Write("Escolha o status: ");
        StatusReserva status = (StatusReserva)int.Parse(Console.ReadLine());

        var reserva = new ReservaHotel
        {
            Cliente = cliente,
            DataEntrada = entrada,
            DataSaida = saida,
            Status = status
        };

        repo.Inserir(reserva);
        Console.WriteLine("Reserva inserida com sucesso!");
    }

    static void ConsultarPorStatus(IReservaHotelRepository repo)
    {
        Console.WriteLine("Status disponíveis:");
        foreach (var valor in Enum.GetValues<StatusReserva>())
        {
            Console.WriteLine($"{(int)valor}. {valor}");
        }
        Console.Write("Escolha o status: ");
        StatusReserva status = (StatusReserva)int.Parse(Console.ReadLine());

        var reservas = repo.ObterPorStatus(status);
        Console.WriteLine($"\nReservas com status '{status}':");

        foreach (var r in reservas)
        {
            Console.WriteLine($"- Cliente: {r.Cliente}, Entrada: {r.DataEntrada:dd/MM/yyyy}, Saída: {r.DataSaida:dd/MM/yyyy}");
        }

        if (!reservas.Any())
        {
            Console.WriteLine("Nenhuma reserva encontrada.");
        }
    }
}
