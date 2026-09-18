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
        public async Task<ActionResult<IEnumerable<Pet>>> GetAll()
        {
            var pets = await _repository.GetAllAsync();
            return Ok(pets);
        }

        // --- FILTRO ATUALIZADO: Incluindo o parâmetro Cidade para o Match Inteligente ---
        [HttpGet("match")]
        public async Task<ActionResult<IEnumerable<Pet>>> GetPetsMatch(
            [FromQuery] bool temCriancas, 
            [FromQuery] bool moraEmApartamento,
            [FromQuery] string? especie = null,
            [FromQuery] string? porte = null,
            [FromQuery] string? sexo = null,
            [FromQuery] string? cidade = null) // NOVO: Parâmetro de cidade adicionado
        {
            // Chamada agora com os 6 argumentos exigidos pela Interface atualizada
            var pets = await _repository.GetPetsParaMatch(
                temCriancas, 
                moraEmApartamento, 
                especie, 
                porte, 
                sexo, 
                cidade);

            return Ok(pets);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Pet>> GetById(int id)
        {
            var pet = await _repository.GetByIdAsync(id);
            if (pet == null) return NotFound();
            return Ok(pet);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }
    }
}