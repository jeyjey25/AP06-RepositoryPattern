using System;
using System.IO;
using System.Text.Json;
public class ArquivoDigital : IEntidade
{
    public Guid Id { get; set; }
    public string NomeArquivo { get; set; }
    public string TipoArquivo { get; set; }
    public long TamanhoBytes { get; set; }
    public DateTime DataUpload { get; set; }

    public ArquivoDigital()
    {
        Id = Guid.NewGuid();
    }
}

// Interface IArquivoDigitalRepository
public interface IArquivoDigitalRepository : IRepository<ArquivoDigital>
{
}
public class ArquivoDigitalJsonRepository : GenericJsonRepository<ArquivoDigital>, IArquivoDigitalRepository
{
    
}
public class Program
{
    public static void Main(string[] args)
    {
        IArquivoDigitalRepository arquivoRepository = new ArquivoDigitalJsonRepository();

        ArquivoDigital arquivo1 = new ArquivoDigital
        {
            NomeArquivo = "documento.pdf",
            TipoArquivo = "PDF",
            TamanhoBytes = 1024 * 1024, 
            DataUpload = DateTime.Now
        };
        arquivoRepository.Adicionar(arquivo1);

        var arquivos = arquivoRepository.ObterTodos();
        foreach (var arquivo in arquivos)
        {
            Console.WriteLine($"Arquivo: {arquivo.NomeArquivo}, Tipo: {arquivo.TipoArquivo}, Tamanho: {arquivo.TamanhoBytes} bytes");
        }
    }
}