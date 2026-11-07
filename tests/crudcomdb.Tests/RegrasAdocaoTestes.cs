using crudcomdb.Models;
using crudcomdb.Services;

namespace crudcomdb.Tests;

// RN04: só animais domésticos (Portaria Ibama nº 93/1998)
public class EspeciesPermitidasTestes
{
    [Theory]
    [InlineData("Cão", "SRD")]
    [InlineData("Gato", "Siamês")]
    [InlineData("Coelho", "")]
    [InlineData("Pássaro", "Calopsita")]
    [InlineData("Pássaro", "periquito-australiano")]
    public void Animais_domesticos_sao_permitidos(string especie, string raca) =>
        Assert.Null(EspeciesPermitidas.Validar(especie, raca));

    [Theory]
    [InlineData("Pássaro", "Papagaio")]
    [InlineData("Pássaro", "Canário-da-terra")]
    [InlineData("Pássaro", "")]
    [InlineData("Jabuti", "")]
    [InlineData("Mico", "")]
    [InlineData("", "")]
    public void Animais_silvestres_ou_desconhecidos_sao_recusados(string especie, string raca) =>
        Assert.NotNull(EspeciesPermitidas.Validar(especie, raca));

    [Fact]
    public void So_aves_passam_por_moderacao()
    {
        Assert.True(EspeciesPermitidas.PrecisaModeracao("Pássaro"));
        Assert.False(EspeciesPermitidas.PrecisaModeracao("Cão"));
    }
}

public class CompatibilidadeTestes
{
    private static Pet Pet(bool quintal = false, bool criancas = true, string especie = "Cão") =>
        new() { Nome = "Bolinha", Especie = especie, PrecisaDeQuintal = quintal, RecomendadoParaCriancas = criancas };

    private static PerfilAdotante Perfil(string moradia = "Casa", bool quintal = true, bool criancas = false) =>
        new() { Moradia = moradia, TemQuintal = quintal, TemCriancas = criancas, MoradiaPropria = true, ProprietarioPermite = true, TodosConcordam = true, HorasSozinho = 4 };

    [Fact]
    public void Casa_com_quintal_e_pet_tranquilo_da_100_por_cento() =>
        Assert.Equal(100, Compatibilidade.Pontuacao(Compatibilidade.Avaliar(Pet(), Perfil())));

    [Fact]
    public void Pet_que_precisa_de_quintal_nao_combina_com_apartamento()
    {
        var itens = Compatibilidade.Avaliar(Pet(quintal: true), Perfil("Apartamento", quintal: false));
        Assert.Contains(itens, i => !i.Ok && i.Texto.Contains("quintal"));
        Assert.True(Compatibilidade.Pontuacao(itens) < 100);
    }

    [Fact]
    public void Pet_nao_recomendado_para_criancas_gera_alerta() =>
        Assert.Contains(Compatibilidade.Avaliar(Pet(criancas: false), Perfil(criancas: true)), i => !i.Ok && i.Texto.Contains("crianças"));

    [Fact]
    public void Gato_em_apartamento_sem_tela_gera_alerta()
    {
        var perfil = Perfil("Apartamento", quintal: false);
        perfil.TelasDeProtecao = false;
        Assert.Contains(Compatibilidade.Avaliar(Pet(especie: "Gato"), perfil), i => !i.Ok && i.Texto.Contains("telas"));
    }

    [Fact]
    public void Cao_sozinho_mais_de_8_horas_gera_alerta()
    {
        var perfil = Perfil();
        perfil.HorasSozinho = 10;
        Assert.Contains(Compatibilidade.Avaliar(Pet(), perfil), i => !i.Ok);
    }
}

// Avaliação de usabilidade com o SUS (Brooke, 1996)
public class SusTestes
{
    [Fact]
    public void Melhores_respostas_dao_100() =>
        Assert.Equal(100, Sus.Calcular(new[] { 5, 1, 5, 1, 5, 1, 5, 1, 5, 1 }));

    [Fact]
    public void Piores_respostas_dao_0() =>
        Assert.Equal(0, Sus.Calcular(new[] { 1, 5, 1, 5, 1, 5, 1, 5, 1, 5 }));

    [Fact]
    public void Tudo_neutro_da_50() =>
        Assert.Equal(50, Sus.Calcular(Enumerable.Repeat(3, 10).ToArray()));

    [Fact]
    public void Concordo_nos_positivos_e_discordo_nos_negativos_da_75() =>
        // ímpares: 4 - 1 = 3 (×5 = 15); pares: 5 - 2 = 3 (×5 = 15); (15 + 15) × 2,5 = 75
        Assert.Equal(75, Sus.Calcular(new[] { 4, 2, 4, 2, 4, 2, 4, 2, 4, 2 }));

    [Theory]
    [InlineData(new[] { 1, 2, 3 })]
    [InlineData(new[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
    [InlineData(new[] { 6, 1, 1, 1, 1, 1, 1, 1, 1, 1 })]
    public void Respostas_invalidas_sao_recusadas(int[] notas) =>
        Assert.Throws<ArgumentException>(() => Sus.Calcular(notas));

    [Theory]
    [InlineData(90, "Excelente")]
    [InlineData(75, "Bom")]
    [InlineData(60, "Razoável")]
    [InlineData(40, "Ruim")]
    [InlineData(20, "Inaceitável")]
    public void Classifica_a_pontuacao(double nota, string esperado) =>
        Assert.Equal(esperado, Sus.Classificar(nota));
}
