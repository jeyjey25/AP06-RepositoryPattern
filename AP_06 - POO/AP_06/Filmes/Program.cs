using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
public class Filme : IEntidade
{
    public Guid Id { get; set; }
    public string Titulo { get; set; }
    public string Diretor { get; set; }
    public int AnoLancamento { get; set; }
    public string Genero { get; set; }

    public Filme()
    {
        Id = Guid.NewGuid();
    }
}

public interface IEntidade
{
}
public interface IFilmeRepository : IRepository<Filme>
{
    IEnumerable<Filme> ObterPorGenero(string genero);
    void Adicionar(Filme filme);
    List<Filme> ObterTodos();
}

public interface IRepository<T>
{
}
public class FilmeJsonRepository : GenericJsonRepository<Filme>, IFilmeRepository
{
    public void Adicionar(Filme filme)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Filme> ObterPorGenero(string genero)
    {
        List<Filme> filmes = ObterTodos();
        return filmes.Where(f => f.Genero.Equals(genero, StringComparison.OrdinalIgnoreCase));
    }

    private List<Filme> ObterTodos()
    {
        throw new NotImplementedException();
    }

    List<Filme> IFilmeRepository.ObterTodos()
    {
        return ObterTodos();
    }
}

public class GenericJsonRepository<T>
{
}
public class Program
{
    public static void Main(string[] args)
    {
        IFilmeRepository filmeRepository = new FilmeJsonRepository();

        while (true)
        {
            Console.WriteLine("\n--- Catálogo de Filmes ---");
            Console.WriteLine("1. Adicionar Filme");
            Console.WriteLine("2. Listar Filmes por Gênero");
            Console.WriteLine("3. Listar Todos os Filmes"); 
            Console.WriteLine("4. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarFilme(filmeRepository);
                    break;
                case "2":
                    ListarFilmesPorGenero(filmeRepository);
                    break;
                case "3":
                    ListarTodosFilmes(filmeRepository); 
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

    static void AdicionarFilme(IFilmeRepository filmeRepository)
    {
        Console.Write("Título: ");
        string titulo = Console.ReadLine();

        Console.Write("Diretor: ");
        string diretor = Console.ReadLine();

        Console.Write("Ano de Lançamento: ");
        if (!int.TryParse(Console.ReadLine(), out int anoLancamento))
        {
            Console.WriteLine("Ano inválido.");
            return;
        }

        Console.Write("Gênero: ");
        string genero = Console.ReadLine();

        Filme filme = new Filme
        {
            Titulo = titulo,
            Diretor = diretor,
            AnoLancamento = anoLancamento,
            Genero = genero
        };

        filmeRepository.Adicionar(filme);
        Console.WriteLine("Filme adicionado com sucesso!");
    }

    static void ListarFilmesPorGenero(IFilmeRepository filmeRepository)
    {
        Console.Write("Gênero: ");
        string genero = Console.ReadLine();

        IEnumerable<Filme> filmesGenero = filmeRepository.ObterPorGenero(genero);
        Console.WriteLine($"Filmes de {genero}:");
        foreach (var filme in filmesGenero)
        {
            Console.WriteLine($"Título: {filme.Titulo}, Diretor: {filme.Diretor}, Ano: {filme.AnoLancamento}");
        }
    }

    static void ListarTodosFilmes(IFilmeRepository filmeRepository)
    {
        List<Filme> filmes = filmeRepository.ObterTodos();
        Console.WriteLine("Todos os Filmes:");
        foreach (var filme in filmes)
        {
            Console.WriteLine($"Título: {filme.Titulo}, Diretor: {filme.Diretor}, Ano: {filme.AnoLancamento}, Gênero: {filme.Genero}");
        }
    }
}