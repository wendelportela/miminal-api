using Microsoft.EntityFrameworkCore;
using MiminalApi.Infraestrutura.Db;
using MimimalApi.DTOs;
using MiminalApi.Dominio.interfaces;
using MiminalApi.Dominio.servicos;
using Microsoft.AspNetCore.Mvc;
using MiminalApi.Dominio.ModelViews;
using MiminalApi.Dominio.Entidades; 

#region builder

var builder = WebApplication.CreateBuilder(args);


  builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
    builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();


  builder.Services.AddDbContext<DbContexto>(options => {
  options.UseMySql(
       builder.Configuration.GetConnectionString("mysql"),
      ServerVersion. AutoDetect(builder.Configuration.GetConnectionString("mysql"))
  );
  });



var app = builder.Build();
#endregion


#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion


#region Administradores
app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administradorServico) => {
    if(administradorServico.Login(loginDTO) != null)
      return Results.Ok("login com sucesso");
    else 
      return Results.Unauthorized(); 
}).WithTags("Administradores");

#endregion

#region veiculo

ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacao = new ErrosDeValidacao
    {
      Mensagens = new List<string>()
    };

    if(string.IsNullOrEmpty(veiculoDTO.Nome))
        validacao.Mensagens.Add("O nome não pode ser vazio");

    if(string.IsNullOrEmpty(veiculoDTO.Marca))
        validacao.Mensagens.Add("A marca não pode ficar em branca");

    if(veiculoDTO.Ano < 1950)
        validacao.Mensagens.Add("Ano incorreto, menor que 1950");

    return validacao;
}



app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
      return Results.BadRequest(validacao);




    var veiculo = new Veiculo
    {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };

    veiculoServico.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).WithTags("Veiculos");


app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoServico veiculoServico) =>
{
    var veiculos = veiculoServico.Todos(pagina);

    return Results.Ok(veiculos);
}).WithTags("Veiculos");


app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();
    return Results.Ok(veiculo);
}).WithTags("Veiculos");


app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    var validacao = validaDTO(veiculoDTO);
    if(validacao.Mensagens.Count > 0)
      return Results.BadRequest(validacao);

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veiculos");



app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) =>
{
    var veiculo = veiculoServico.BuscaPorId(id);
    if(veiculo == null) return Results.NotFound();

    veiculoServico.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veiculos");


#endregion


#region app

app.UseSwagger();
app.UseSwaggerUI();
app.Run();

#endregion