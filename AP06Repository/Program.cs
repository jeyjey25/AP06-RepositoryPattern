// Testando repositório de produtos
var repo = new ProdutoJsonRepository();

repo.Adicionar(new Produto
{
    Nome = "Mouse",
    Descricao = "Mouse óptico USB",
    Preco = 59.90M,
    Estoque = 20
});

foreach (var produto in repo.ObterTodos())
{
    Console.WriteLine($"{produto.Nome} - R$ {produto.Preco}");
}