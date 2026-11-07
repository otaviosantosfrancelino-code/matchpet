using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.AspNetCore.DataProtection;

namespace crudcomdb.Tests;

public class SenhaServiceTestes
{
    [Fact]
    public void Senha_e_guardada_com_hash_e_nunca_em_texto()
    {
        var hash = SenhaService.GerarHash("MinhaSenha@2026");
        Assert.NotEqual("MinhaSenha@2026", hash);
        Assert.True(SenhaService.EhHash(hash));
    }

    [Fact]
    public void Senha_certa_entra_e_senha_errada_nao()
    {
        var usuario = new Usuario { Senha = SenhaService.GerarHash("MinhaSenha@2026") };
        Assert.True(SenhaService.Verificar(usuario, "MinhaSenha@2026"));
        Assert.False(SenhaService.Verificar(usuario, "minhasenha@2026"));
        Assert.False(SenhaService.Verificar(usuario, ""));
    }

    [Fact]
    public void Mesma_senha_gera_hashes_diferentes_por_causa_do_sal() =>
        Assert.NotEqual(SenhaService.GerarHash("abc12345"), SenhaService.GerarHash("abc12345"));
}

public class LimiteTentativasTestes
{
    [Fact]
    public void Bloqueia_depois_de_5_erros_e_libera_ao_zerar()
    {
        var limite = new LimiteTentativasService();
        var janela = TimeSpan.FromMinutes(15);
        for (var i = 0; i < 4; i++) limite.Registrar("login:conta:x");
        Assert.Null(limite.Bloqueio("login:conta:x", 5, janela));

        limite.Registrar("login:conta:x");
        Assert.NotNull(limite.Bloqueio("login:conta:x", 5, janela));

        limite.Zerar("login:conta:x");
        Assert.Null(limite.Bloqueio("login:conta:x", 5, janela));
    }

    [Fact]
    public void Contas_diferentes_tem_limites_separados()
    {
        var limite = new LimiteTentativasService();
        for (var i = 0; i < 5; i++) limite.Registrar("login:conta:a");
        Assert.Null(limite.Bloqueio("login:conta:b", 5, TimeSpan.FromMinutes(15)));
    }
}

// LGPD: a foto do documento fica criptografada no banco
public class CofreDocumentosTestes
{
    private readonly CofreDocumentos _cofre = new(new EphemeralDataProtectionProvider());

    [Fact]
    public void Foto_criptografada_nao_contem_a_imagem_e_abre_de_volta()
    {
        var imagem = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var guardado = _cofre.Fechar(imagem, "image/jpeg");

        Assert.True(CofreDocumentos.EstaCriptografado(guardado));
        Assert.DoesNotContain(Convert.ToBase64String(imagem), guardado);
        Assert.Equal($"data:image/jpeg;base64,{Convert.ToBase64String(imagem)}", _cofre.Abrir(guardado));
    }

    [Fact]
    public void Dado_adulterado_nao_abre()
    {
        var guardado = _cofre.Fechar(new byte[] { 9, 9, 9 }, "image/png");
        var adulterado = guardado[..^6] + "AAAAAA";
        Assert.Null(_cofre.Abrir(adulterado));
    }

    [Fact]
    public void Chave_de_outro_servidor_nao_abre_o_documento()
    {
        var guardado = _cofre.Fechar(new byte[] { 1, 2, 3 }, "image/png");
        var outroServidor = new CofreDocumentos(new EphemeralDataProtectionProvider());
        Assert.Null(outroServidor.Abrir(guardado));
    }
}

// Termo de adoção: autenticidade e privacidade na verificação pública
public class TermoTestes
{
    private static TermoAdocao Termo() => new()
    {
        Codigo = "MP-1A2B-3C4D", Texto = "Termo de adoção do Bolinha", DoadorId = 1, AdotanteId = 2, PetId = 3,
        DataEnvio = new DateTime(2026, 11, 5, 14, 30, 0, DateTimeKind.Utc), IpDoador = "200.1.2.3",
        DataAssinatura = new DateTime(2026, 11, 5, 15, 0, 0, DateTimeKind.Utc), IpAdotante = "200.4.5.6",
    };

    [Fact]
    public void Impressao_digital_e_sempre_a_mesma_para_o_mesmo_termo() =>
        Assert.Equal(ChatService.ImpressaoDigital(Termo()), ChatService.ImpressaoDigital(Termo()));

    [Fact]
    public void Qualquer_alteracao_no_texto_muda_a_impressao_digital()
    {
        var alterado = Termo();
        alterado.Texto += ".";
        Assert.NotEqual(ChatService.ImpressaoDigital(Termo()), ChatService.ImpressaoDigital(alterado));
    }

    [Theory]
    [InlineData("Maria Aparecida da Silva", "Maria A. da S.")]
    [InlineData("Pedro Henrique Palamoni Grace", "Pedro H. P. G.")]
    [InlineData("Ana", "Ana")]
    public void Nome_aparece_abreviado_na_verificacao_publica(string nome, string esperado) =>
        Assert.Equal(esperado, ChatService.AbreviarNome(nome));
}
