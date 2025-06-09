using System.Text.Json;

public class ProdutoJsonRepository : IProdutoRepository
{
    private const string CaminhoArquivo = "produtos.json";
    private List<Produto> produtos = new();

    public ProdutoJsonRepository()
    {
        Carregar();
    }

    public void Adicionar(Produto produto)
    {
        produto.Id = Guid.NewGuid();
        produtos.Add(produto);
        Salvar();
    }

    public Produto? ObterPorId(Guid id)
    {
        return produtos.FirstOrDefault(p => p.Id == id);
    }

    public List<Produto> ObterTodos()
    {
        return produtos;
    }

    public void Atualizar(Produto produto)
    {
        var index = produtos.FindIndex(p => p.Id == produto.Id);
        if (index >= 0)
        {
            produtos[index] = produto;
            Salvar();
        }
    }

    public bool Remover(Guid id)
    {
        var produto = ObterPorId(id);
        if (produto != null)
        {
            produtos.Remove(produto);
            Salvar();
            return true;
        }
        return false;
    }

    private void Carregar()
    {
        if (File.Exists(CaminhoArquivo))
        {
            var json = File.ReadAllText(CaminhoArquivo);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            produtos = JsonSerializer.Deserialize<List<Produto>>(json, options) ?? new();
        }
    }

    private void Salvar()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(CaminhoArquivo, JsonSerializer.Serialize(produtos, options));
    }
}