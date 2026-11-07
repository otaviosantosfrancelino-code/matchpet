using crudcomdb.Services;

namespace crudcomdb.Tests;

// RN01: um cadastro por CPF/CNPJ, com validação dos dígitos verificadores
public class DocumentoUtilTestes
{
    [Theory]
    [InlineData("529.982.247-25")]
    [InlineData("52998224725")]
    [InlineData("111.444.777-35")]
    public void Cpf_valido_e_aceito(string cpf) => Assert.True(DocumentoUtil.CpfValido(cpf));

    [Theory]
    [InlineData("529.982.247-24")]   // dígito verificador errado
    [InlineData("111.111.111-11")]   // todos os dígitos iguais
    [InlineData("123.456.789")]      // curto demais
    [InlineData("")]
    [InlineData(null)]
    public void Cpf_invalido_e_recusado(string? cpf) => Assert.False(DocumentoUtil.CpfValido(cpf));

    [Theory]
    [InlineData("11.222.333/0001-81")]
    [InlineData("11222333000181")]
    public void Cnpj_valido_e_aceito(string cnpj) => Assert.True(DocumentoUtil.CnpjValido(cnpj));

    [Theory]
    [InlineData("11.222.333/0001-80")]
    [InlineData("00.000.000/0000-00")]
    public void Cnpj_invalido_e_recusado(string cnpj) => Assert.False(DocumentoUtil.CnpjValido(cnpj));

    [Fact]
    public void Formata_cpf_e_cnpj()
    {
        Assert.Equal("529.982.247-25", DocumentoUtil.Formatar("52998224725"));
        Assert.Equal("11.222.333/0001-81", DocumentoUtil.Formatar("11222333000181"));
    }

    [Fact]
    public void Documento_com_e_sem_pontuacao_sao_o_mesmo()
    {
        Assert.Equal(DocumentoUtil.SomenteDigitos("529.982.247-25"), DocumentoUtil.SomenteDigitos("529 982 247 25"));
        Assert.True(DocumentoUtil.EhCpf("529.982.247-25"));
        Assert.True(DocumentoUtil.EhCnpj("11.222.333/0001-81"));
    }

    [Theory]
    [InlineData("Maria Aparecida da Silva", "MARIA APARECIDA SILVA", 1.0)]
    [InlineData("José Antônio", "Jose Antonio de Souza", 1.0)]   // ignora acentos e o sobrenome a mais
    [InlineData("Ana Paula Lima", "Carlos Henrique Souza", 0.0)]
    public void Compara_nome_do_documento_com_o_cadastro(string a, string b, double esperado) =>
        Assert.Equal(esperado, DocumentoUtil.SimilaridadeNomes(a, b), 2);
}
