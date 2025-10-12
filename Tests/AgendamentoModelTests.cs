using SiteQuadra.Models;
using Xunit;

namespace SiteQuadra.Tests;

public class AgendamentoModelTests
{
    [Fact]
    public void Agendamento_DeveTerPropriedadesPadraoCorretas()
    {
        // Act
        var agendamento = new Agendamento();

        // Assert
        Assert.Equal(0, agendamento.Id); // Padrão int
        Assert.Equal(string.Empty, agendamento.NomeResponsavel);
        Assert.Equal(string.Empty, agendamento.Contato);
        Assert.Equal(string.Empty, agendamento.CidadeBairro);
        Assert.Equal("#3788d8", agendamento.Cor); // Cor padrão azul
        Assert.Equal(default(DateTime), agendamento.DataHoraInicio);
        Assert.Equal(default(DateTime), agendamento.DataHoraFim);
    }

    [Fact]
    public void Agendamento_DevePermitirDefinirTodasPropriedades()
    {
        // Arrange
        var dataInicio = DateTime.Today.AddDays(1).AddHours(10);
        var dataFim = DateTime.Today.AddDays(1).AddHours(11);

        // Act
        var agendamento = new Agendamento
        {
            Id = 123,
            NomeResponsavel = "João Silva",
            Contato = "(98) 99999-9999",
            CidadeBairro = "Monte Alegre",
            DataHoraInicio = dataInicio,
            DataHoraFim = dataFim,
            Cor = "#ff0000"
        };

        // Assert
        Assert.Equal(123, agendamento.Id);
        Assert.Equal("João Silva", agendamento.NomeResponsavel);
        Assert.Equal("(98) 99999-9999", agendamento.Contato);
        Assert.Equal("Monte Alegre", agendamento.CidadeBairro);
        Assert.Equal(dataInicio, agendamento.DataHoraInicio);
        Assert.Equal(dataFim, agendamento.DataHoraFim);
        Assert.Equal("#ff0000", agendamento.Cor);
    }

    [Theory]
    [InlineData("#ff0000")] // Vermelho
    [InlineData("#00ff00")] // Verde
    [InlineData("#0000ff")] // Azul
    [InlineData("#ffffff")] // Branco
    [InlineData("#000000")] // Preto
    [InlineData("#3788d8")] // Padrão
    public void Agendamento_DeveAceitarDiferentesCores(string cor)
    {
        // Act
        var agendamento = new Agendamento
        {
            Cor = cor
        };

        // Assert
        Assert.Equal(cor, agendamento.Cor);
    }

    [Fact]
    public void Agendamento_DataHoraFim_DeveSerIndependenteDeDataHoraInicio()
    {
        // Arrange
        var dataInicio = DateTime.Today.AddHours(10);
        var dataFim = DateTime.Today.AddHours(15); // 5 horas depois

        // Act
        var agendamento = new Agendamento
        {
            DataHoraInicio = dataInicio,
            DataHoraFim = dataFim
        };

        // Assert
        Assert.Equal(dataInicio, agendamento.DataHoraInicio);
        Assert.Equal(dataFim, agendamento.DataHoraFim);
        Assert.NotEqual(agendamento.DataHoraInicio, agendamento.DataHoraFim);
    }

    [Fact]
    public void Agendamento_DevePermitirStringVaziaEspacosEmBranco()
    {
        // Act
        var agendamento = new Agendamento
        {
            NomeResponsavel = "",
            Contato = "   ",
            CidadeBairro = "\t",
            Cor = ""
        };

        // Assert - O modelo deve aceitar (validação é feita em outros lugares)
        Assert.Equal("", agendamento.NomeResponsavel);
        Assert.Equal("   ", agendamento.Contato);
        Assert.Equal("\t", agendamento.CidadeBairro);
        Assert.Equal("", agendamento.Cor);
    }

    [Fact]
    public void Agendamento_DevePermitirStringsMuitoLongas()
    {
        // Arrange
        var nomeMuitoLongo = new string('A', 1000);
        var contatoMuitoLongo = new string('1', 100);
        var cidadeMuitoLonga = new string('B', 500);

        // Act
        var agendamento = new Agendamento
        {
            NomeResponsavel = nomeMuitoLongo,
            Contato = contatoMuitoLongo,
            CidadeBairro = cidadeMuitoLonga
        };

        // Assert
        Assert.Equal(nomeMuitoLongo, agendamento.NomeResponsavel);
        Assert.Equal(contatoMuitoLongo, agendamento.Contato);
        Assert.Equal(cidadeMuitoLonga, agendamento.CidadeBairro);
    }

    [Theory]
    [InlineData("José da Silva")]
    [InlineData("Maria José")]
    [InlineData("João Antônio")]
    [InlineData("Ana-Clara")]
    [InlineData("Pedro D'Angelo")]
    public void Agendamento_DeveAceitarNomesComCaracteresEspeciais(string nome)
    {
        // Act
        var agendamento = new Agendamento
        {
            NomeResponsavel = nome
        };

        // Assert
        Assert.Equal(nome, agendamento.NomeResponsavel);
    }

    [Theory]
    [InlineData("(98) 99999-9999")]
    [InlineData("98999999999")]
    [InlineData("+55 98 99999-9999")]
    [InlineData("(11) 1234-5678")]
    [InlineData("85987654321")]
    public void Agendamento_DeveAceitarDiferentesFormatosDeContato(string contato)
    {
        // Act
        var agendamento = new Agendamento
        {
            Contato = contato
        };

        // Assert
        Assert.Equal(contato, agendamento.Contato);
    }

    [Theory]
    [InlineData("São Luís")]
    [InlineData("Monte Alegre")]
    [InlineData("Centro")]
    [InlineData("Liberdade")]
    [InlineData("Cohama")]
    [InlineData("Vila Palmeira")]
    [InlineData("São Francisco")]
    public void Agendamento_DeveAceitarDiferentesCidades(string cidade)
    {
        // Act
        var agendamento = new Agendamento
        {
            CidadeBairro = cidade
        };

        // Assert
        Assert.Equal(cidade, agendamento.CidadeBairro);
    }

    [Fact]
    public void Agendamento_DevePermitirDataPassada()
    {
        // Arrange
        var dataPassada = DateTime.Today.AddDays(-10);

        // Act
        var agendamento = new Agendamento
        {
            DataHoraInicio = dataPassada
        };

        // Assert
        Assert.Equal(dataPassada, agendamento.DataHoraInicio);
        Assert.True(agendamento.DataHoraInicio < DateTime.Today);
    }

    [Fact]
    public void Agendamento_DevePermitirDataFutura()
    {
        // Arrange
        var dataFutura = DateTime.Today.AddDays(30);

        // Act
        var agendamento = new Agendamento
        {
            DataHoraInicio = dataFutura
        };

        // Assert
        Assert.Equal(dataFutura, agendamento.DataHoraInicio);
        Assert.True(agendamento.DataHoraInicio > DateTime.Today);
    }
}