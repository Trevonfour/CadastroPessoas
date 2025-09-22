using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StefaniniCadastroPessoas.DTOs;
using StefaniniCadastroPessoas.Services;

namespace StefaniniCadastroPessoas.Controllers
{
    /// <summary>
    /// Controller para gerenciamento de pessoas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class PessoasController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;
        private readonly ILogger<PessoasController> _logger;

        public PessoasController(IPessoaService pessoaService, ILogger<PessoasController> logger)
        {
            _pessoaService = pessoaService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todas as pessoas com paginação e filtro
        /// </summary>
        /// <param name="pagina">Número da página (padrão: 1)</param>
        /// <param name="tamanhoPagina">Tamanho da página (padrão: 10, máximo: 100)</param>
        /// <param name="filtro">Filtro por nome, CPF ou email</param>
        /// <returns>Lista paginada de pessoas</returns>
        /// <response code="200">Lista de pessoas retornada com sucesso</response>
        /// <response code="400">Parâmetros inválidos</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet]
        [ProducesResponseType(typeof(PessoaListResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ObterTodas(
            [FromQuery] int pagina = 1,
            [FromQuery] int tamanhoPagina = 10,
            [FromQuery] string? filtro = null)
        {
            try
            {
                if (pagina < 1)
                {
                    return BadRequest(new { message = "Página deve ser maior que zero" });
                }

                if (tamanhoPagina < 1 || tamanhoPagina > 100)
                {
                    return BadRequest(new { message = "Tamanho da página deve estar entre 1 e 100" });
                }

                var resultado = await _pessoaService.ObterTodasAsync(pagina, tamanhoPagina, filtro);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de pessoas");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém uma pessoa por ID
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Dados da pessoa</returns>
        /// <response code="200">Pessoa encontrada</response>
        /// <response code="404">Pessoa não encontrada</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet("v1/{id:int}")]
        [ProducesResponseType(typeof(PessoaResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ObterPorId(int id)
        {
            try
            {
                var pessoa = await _pessoaService.ObterPorIdAsync(id);

                if (pessoa == null)
                {
                    return NotFound(new { message = "Pessoa não encontrada" });
                }

                return Ok(pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pessoa por ID: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Obtém uma pessoa por CPF
        /// </summary>
        /// <param name="cpf">CPF da pessoa (com ou sem formatação)</param>
        /// <returns>Dados da pessoa</returns>
        /// <response code="200">Pessoa encontrada</response>
        /// <response code="404">Pessoa não encontrada</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet("cpf/{cpf}")]
        [ProducesResponseType(typeof(PessoaResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ObterPorCpf(string cpf)
        {
            try
            {
                var pessoa = await _pessoaService.ObterPorCpfAsync(cpf);

                if (pessoa == null)
                {
                    return NotFound(new { message = "Pessoa não encontrada" });
                }

                return Ok(pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pessoa por CPF: {CPF}", cpf);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Cria uma nova pessoa
        /// </summary>
        /// <param name="criarPessoaDto">Dados da pessoa a ser criada</param>
        /// <returns>Dados da pessoa criada</returns>
        /// <response code="201">Pessoa criada com sucesso</response>
        /// <response code="400">Dados inválidos ou CPF já existe</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPost]
        [ProducesResponseType(typeof(PessoaResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Criar([FromBody] CriarPessoaDto criarPessoaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pessoa = await _pessoaService.CriarAsync(criarPessoaDto);

                if (pessoa == null)
                {
                    return BadRequest(new { message = "CPF inválido ou já existe no sistema" });
                }

                return CreatedAtAction(nameof(ObterPorId), new { id = pessoa.Id }, pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pessoa");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Atualiza uma pessoa existente
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <param name="atualizarPessoaDto">Dados a serem atualizados</param>
        /// <returns>Dados da pessoa atualizada</returns>
        /// <response code="200">Pessoa atualizada com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="404">Pessoa não encontrada</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PessoaResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarPessoaDto atualizarPessoaDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var pessoa = await _pessoaService.AtualizarAsync(id, atualizarPessoaDto);

                if (pessoa == null)
                {
                    return NotFound(new { message = "Pessoa não encontrada" });
                }

                return Ok(pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pessoa: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Remove uma pessoa (soft delete)
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Confirmação da remoção</returns>
        /// <response code="204">Pessoa removida com sucesso</response>
        /// <response code="404">Pessoa não encontrada</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> Remover(int id)
        {
            try
            {
                var sucesso = await _pessoaService.RemoverAsync(id);

                if (!sucesso)
                {
                    return NotFound(new { message = "Pessoa não encontrada" });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover pessoa: {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Verifica se um CPF já existe no sistema
        /// </summary>
        /// <param name="cpf">CPF a ser verificado</param>
        /// <param name="idExcluir">ID da pessoa a ser excluída da verificação (opcional)</param>
        /// <returns>Status da verificação</returns>
        /// <response code="200">Resultado da verificação</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet("verificar-cpf/{cpf}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> VerificarCpf(string cpf, [FromQuery] int? idExcluir = null)
        {
            try
            {
                var existe = await _pessoaService.ExisteCpfAsync(cpf, idExcluir);
                return Ok(new { existe = existe });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar CPF: {CPF}", cpf);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Endpoint específico da versão 2 da API - Inclui dados adicionais
        /// </summary>
        /// <param name="id">ID da pessoa</param>
        /// <returns>Dados da pessoa com informações adicionais</returns>
        /// <response code="200">Pessoa encontrada com dados estendidos</response>
        /// <response code="404">Pessoa não encontrada</response>
        /// <response code="401">Usuário não autenticado</response>
        [HttpGet("v2/{id:int}")] 
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> ObterPorIdV2(int id)
        {
            try
            {
                var pessoa = await _pessoaService.ObterPorIdAsync(id);

                if (pessoa == null)
                {
                    return NotFound(new { message = "Pessoa não encontrada" });
                }

                var pessoaV2 = new
                {
                    pessoa.Id,
                    pessoa.Nome,
                    pessoa.Sexo,
                    pessoa.Email,
                    pessoa.DataNascimento,
                    pessoa.Idade,
                    pessoa.Naturalidade,
                    pessoa.Nacionalidade,
                    pessoa.CPF,
                    pessoa.DataCadastro,
                    pessoa.DataAtualizacao,
                };

                return Ok(pessoaV2);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pessoa por ID (v2): {Id}", id);
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }
    }
}
