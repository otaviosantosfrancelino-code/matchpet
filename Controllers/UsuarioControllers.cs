using crudcomdb.Interfaces;
using crudcomdb.Models;
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
        public async Task<ActionResult<Usuario>> GetUsuario(int id)
        {
            var usuario = await _repository.GetByIdAsync(id);
            if (usuario == null) return NotFound("Usuário não encontrado.");
            return Ok(usuario);
        }

        // POST: api/usuarios (O cadastro propriamente dito)
        [HttpPost]
        public async Task<ActionResult<Usuario>> PostUsuario(Usuario usuario)
        {
            // 1. VALIDAÇÃO DE DUPLICIDADE (Segurança de Dados)
            var usuarioExistente = await _repository.GetByEmailAsync(usuario.Email);
            if (usuarioExistente != null) 
                return BadRequest("Este e-mail já está vinculado a uma conta ativa.");

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
                return CreatedAtAction(nameof(GetUsuario), new { id = usuario.Id }, usuario);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao processar identificação civil: {ex.Message}");
            }
        }
    }
}