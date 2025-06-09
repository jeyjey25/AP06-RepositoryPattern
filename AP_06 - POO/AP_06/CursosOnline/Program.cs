using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
public class CursoOnline : IEntidade
{
    public Guid Id { get; set; }
    public string NomeCurso { get; set; }
    public string Descricao { get; set; }
    public decimal Preco { get; set; }

    public CursoOnline()
    {
        Id = Guid.NewGuid();
    }
}
public interface ICursoOnlineRepository : IRepository<CursoOnline>
{
}
public class CursoOnlineJsonRepository : GenericJsonRepository<CursoOnline>, ICursoOnlineRepository
{
}
public class CatalogoCursosService
{
    private readonly ICursoOnlineRepository _cursoRepository;

    public CatalogoCursosService(ICursoOnlineRepository cursoRepository)
    {
        _cursoRepository = cursoRepository;
    }

    public void AdicionarCurso(CursoOnline curso)
    {
        List<CursoOnline> cursos = _cursoRepository.ObterTodos();
        if (cursos.Any(c => c.NomeCurso.Equals(curso.NomeCurso, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine("Já existe um curso com este nome.");
            return;
        }

        _cursoRepository.Adicionar(curso);
        Console.WriteLine("Curso adicionado com sucesso.");
    }

    public List<CursoOnline> ListarCursos()
    {
        return _cursoRepository.ObterTodos();
    }
}
public class Program
{
    public static void Main(string[] args)
    {
        ICursoOnlineRepository cursoRepository = new CursoOnlineJsonRepository();
        CatalogoCursosService catalogoService = new CatalogoCursosService(cursoRepository);

        while (true)
        {
            Console.WriteLine("\n--- Plataforma de Cursos Online ---");
            Console.WriteLine("1. Adicionar Curso");
            Console.WriteLine("2. Listar Cursos");
            Console.WriteLine("3. Sair");

            Console.Write("Escolha uma opção: ");
            string opcao = Console.ReadLine();

            switch (opcao)
            {
                case "1":
                    AdicionarCurso(catalogoService);
                    break;
                case "2":
                    ListarCursos(catalogoService);
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

    static void AdicionarCurso(CatalogoCursosService catalogoService)
    {
        Console.Write("Nome do Curso: ");
        string nomeCurso = Console.ReadLine();

        Console.Write("Descrição: ");
        string descricao = Console.ReadLine();

        Console.Write("Preço: ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal preco))
        {
            Console.WriteLine("Preço inválido.");
            return;
        }

        CursoOnline curso = new CursoOnline
        {
            NomeCurso = nomeCurso,
            Descricao = descricao,
            Preco = preco
        };

        catalogoService.AdicionarCurso(curso);
    }

    static void ListarCursos(CatalogoCursosService catalogoService)
    {
        List<CursoOnline> cursos = catalogoService.ListarCursos();
        Console.WriteLine("Cursos Online:");
        foreach (var curso in cursos)
        {
            Console.WriteLine($"Nome: {curso.NomeCurso}, Preço: {curso.Preco}");
        }
    }
}
