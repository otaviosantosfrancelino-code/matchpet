using System.ComponentModel.DataAnnotations;
using crudcomdb.Interfaces;
using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace crudcomdb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("api")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;
        private readonly ILogger<UsuariosController> _log;

        public UsuariosController(IUsuarioRepository repository, ILogger<UsuariosController> log)
        {
            _repository = repository;
            _log = log;
        }

        // Só os campos que uma pessoa pode informar no cadastro.
        // Verificação, classificação, tipo ONG etc. são definidos pelo servidor (evita contas "verificadas" falsas).
        public record NovoUsuario(
            [Required, StringLength(120)] string Nome,
            [Required, EmailAddress, StringLength(160)] string Email,
            [Required, StringLength(20)] string Documento,
            [Required, StringLength(20)] string Telefone,
            [Required, StringLength(100, MinimumLength = 8)] string Senha,
            [StringLength(80)] string? Cidade,
            [StringLength(2)] string? Estado,
            bool AceitouTermos);

        // GET: api/usuarios/5 (só dados públicos do perfil)
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return NotFound("Usuário não encontrado.");
            return Ok(ParaResposta(usuario));
        }

        // POST: api/usuarios
        [HttpPost]
        public async Task<ActionResult> PostUsuario(NovoUsuario dados)
        {
            if (!dados.AceitouTermos)
                return BadRequest("O aceite dos termos de uso e da política de privacidade é obrigatório.");
            if (!DocumentoUtil.CpfValido(dados.Documento))
                return BadRequest("CPF inválido. ONGs devem se cadastrar pelo site, onde o CNPJ é consultado na Receita Federal.");
            if (await _repository.GetByEmailAsync(dados.Email) != null)
                return BadRequest("Este e-mail já está vinculado a uma conta.");
            if (await _repository.GetByDocumentoAsync(dados.Documento) != null)
                return BadRequest("Este CPF já possui uma conta cadastrada.");

            var usuario = new Usuario
            {
                Nome = dados.Nome.Trim(),
                Email = dados.Email.Trim().ToLowerInvariant(),
                Documento = DocumentoUtil.Formatar(dados.Documento),
                Telefone = dados.Telefone.Trim(),
                Senha = SenhaService.GerarHash(dados.Senha),
                Cidade = string.IsNullOrWhiteSpace(dados.Cidade) ? "Franca" : dados.Cidade.Trim(),
                Estado = string.IsNullOrWhiteSpace(dados.Estado) ? "SP" : dados.Estado.Trim().ToUpperInvariant(),
                AceitouTermos = true,
                DataAceiteTermos = DateTime.UtcNow,
                IpAceite = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP não identificado",
                DataCadastro = DateTime.UtcNow,
                // Sempre começa sem verificação; o documento é conferido depois, no Perfil
                EhOng = false,
                StatusValidacaoDocumento = "Pendente",
                Classificacao = "Novo Usuário",
                EmailConfirmado = false,
            };

            try
            {
                await _repository.AddAsync(usuario);
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, ParaResposta(usuario));
            }
            catch (Exception ex)
            {
                // O detalhe do erro fica só no log do servidor
                _log.LogError(ex, "Erro ao cadastrar usuário pela API");
                return StatusCode(500, "Não foi possível concluir o cadastro agora. Tente novamente.");
            }
        }

        // Nunca devolve e-mail, telefone, senha, documento, código de confirmação ou IP
        private static object ParaResposta(Usuario u) => new
        {
            u.Id, u.Nome, u.Cidade, u.Estado,
            u.EhOng, u.DescricaoOng, u.ChavePix, u.Classificacao,
            u.StatusValidacaoDocumento, u.DataCadastro
        };
    }
}
