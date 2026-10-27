using crudcomdb.Interfaces;
using crudcomdb.Models;
using Microsoft.AspNetCore.Mvc;

namespace crudcomdb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PetsControllers : ControllerBase
    {
        private readonly IPetRepository _repository;

        public PetsControllers(IPetRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var pets = await _repository.GetAllAsync();
            return Ok(pets.Select(ParaResposta));
        }

        // --- FILTRO ATUALIZADO: Incluindo o parâmetro Cidade para o Match Inteligente ---
        [HttpGet("match")]
        public async Task<ActionResult> GetPetsMatch(
            [FromQuery] bool temCriancas, 
            [FromQuery] bool moraEmApartamento,
            [FromQuery] string? especie = null,
            [FromQuery] string? porte = null,
            [FromQuery] string? sexo = null,
            [FromQuery] string? cidade = null)
        {
            var pets = await _repository.GetPetsParaMatch(
                temCriancas, 
                moraEmApartamento, 
                especie, 
                porte, 
                sexo, 
                cidade);

            return Ok(pets.Select(ParaResposta));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var pet = await _repository.GetByIdAsync(id);
            if (pet == null) return NotFound();
            return Ok(ParaResposta(pet));
        }

        // OBS: a rota DELETE pública foi removida — qualquer pessoa podia apagar pets sem login.
        // A exclusão continua disponível para o dono, pela tela "Meus Anúncios".

        // Monta a resposta da API sem dados sensíveis do doador (senha, documento, IP...)
        private static object ParaResposta(Pet p) => new
        {
            p.Id, p.Nome, p.Especie, p.Raca, p.Sexo, p.Porte, p.Idade, p.Descricao,
            p.Castrado, p.PrecisaDeQuintal, p.RecomendadoParaCriancas, p.Status,
            p.UsuarioDoadorId,
            Doador = p.UsuarioDoador == null ? null : new
            {
                p.UsuarioDoador.Id, p.UsuarioDoador.Nome, p.UsuarioDoador.Cidade,
                p.UsuarioDoador.Estado, p.UsuarioDoador.EhOng
            },
            Imagens = p.Imagens.Select(i => new { i.Id, i.Base64Data })
        };
    }
}
