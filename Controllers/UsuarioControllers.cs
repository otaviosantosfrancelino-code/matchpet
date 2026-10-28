using crudcomdb.Interfaces;
using crudcomdb.Models;
using crudcomdb.Services;
using Microsoft.AspNetCore.Mvc;

namespace crudcomdb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioRepository _repository;

        public UsuariosController(IUsuarioRepository repository)
        {
            _repository = repository;
        }

        // GET: api/usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult> GetUsuario(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return NotFound("Usuário não encontrado.");
            return Ok(ParaResposta(usuario));
        }

        // POST: api/usuarios (O cadastro propriamente dito)
        [HttpPost]
        public async Task<ActionResult> PostUsuario(Usuario usuario)
        {
            // 1. VALIDAÇÃO DE DUPLICIDADE (Segurança de Dados)
            var usuarioExistente = await _repository.GetByEmailAsync(usuario.Email);
            if (usuarioExistente != null) 
                return BadRequest("Este e-mail já está vinculado a uma conta ativa.");

            if (!DocumentoUtil.DocumentoValido(usuario.Documento))
                return BadRequest("CPF ou CNPJ inválido.");

            if (await _repository.GetByDocumentoAsync(usuario.Documento) != null)
                return BadRequest("Este CPF ou CNPJ já possui uma conta cadastrada.");

            if (string.IsNullOrWhiteSpace(usuario.Senha) || usuario.Senha.Length < 8)
                return BadRequest("A senha precisa ter pelo menos 8 caracteres.");

            // Senha nunca é guardada em texto puro; o documento é salvo formatado
            usuario.Senha = SenhaService.GerarHash(usuario.Senha);
            usuario.Documento = DocumentoUtil.Formatar(usuario.Documento);
            usuario.StatusValidacaoDocumento = "Pendente";

            // 2. SEGURANÇA JURÍDICA E LGPD (Nexo Causal)
            // AJUSTE: O operador '??' elimina o aviso CS8601 de referência nula
            usuario.IpAceite = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP não identificado";
            
            // 3. FORMALIZAÇÃO DO ACEITE (Manifestação de Vontade)
            if (usuario.AceitouTermos)
            {
                // Registra o timestamp exato do "clique" para evidência digital
                usuario.DataAceiteTermos = DateTime.UtcNow;
            }
            else
            {
                // Impede o cadastro sem a concordância jurídica
                return BadRequest("O aceite dos termos de uso e política de privacidade é obrigatório para a identificação civil.");
            }

            // 4. PERSISTÊNCIA NO BANCO
            try 
            {
                await _repository.AddAsync(usuario);
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, ParaResposta(usuario));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao processar identificação civil: {ex.Message}");
            }
        }

        // Nunca devolve senha, documento, código de confirmação ou IP na API
        private static object ParaResposta(Usuario u) => new
        {
            u.Id, u.Nome, u.Email, u.Telefone, u.Cidade, u.Estado,
            u.EhOng, u.DescricaoOng, u.ChavePix, u.Classificacao,
            u.StatusValidacaoDocumento, u.DataCadastro
        };
    }
}