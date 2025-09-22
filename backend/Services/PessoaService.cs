using Microsoft.EntityFrameworkCore;
using StefaniniCadastroPessoas.Data;
using StefaniniCadastroPessoas.Models;
using StefaniniCadastroPessoas.DTOs;
using Microsoft.Extensions.Logging; // Garantir que o using do ILogger está presente

namespace StefaniniCadastroPessoas.Services
{
    /// <summary>
    /// Interface para o serviço de pessoas
    /// </summary>
    public interface IPessoaService
    {
        Task<PessoaListResponseDto> ObterTodasAsync(int pagina = 1, int tamanhoPagina = 10, string? filtro = null);
        Task<PessoaResponseDto?> ObterPorIdAsync(int id);
        Task<PessoaResponseDto?> ObterPorCpfAsync(string cpf);
        Task<PessoaResponseDto?> CriarAsync(CriarPessoaDto criarPessoaDto);
        Task<PessoaResponseDto?> AtualizarAsync(int id, AtualizarPessoaDto atualizarPessoaDto);
        Task<bool> RemoverAsync(int id);
        Task<bool> ExisteCpfAsync(string cpf, int? idExcluir = null);
    }

    /// <summary>
    /// Serviço para gerenciamento de pessoas
    /// </summary>
    public class PessoaService : IPessoaService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICpfValidationService _cpfValidationService;
        private readonly ILogger<PessoaService> _logger;

        public PessoaService(
            ApplicationDbContext context,
            ICpfValidationService cpfValidationService,
            ILogger<PessoaService> logger)
        {
            _context = context;
            _cpfValidationService = cpfValidationService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém todas as pessoas com paginação e filtro
        /// </summary>
        public async Task<PessoaListResponseDto> ObterTodasAsync(int pagina = 1, int tamanhoPagina = 10, string? filtro = null)
        {
            try
            {
                var query = _context.Pessoas.Where(p => p.Ativo);

                if (!string.IsNullOrWhiteSpace(filtro))
                {
                    var filtroLower = filtro.ToLower();
                    query = query.Where(p =>
                        p.Nome.ToLower().Contains(filtroLower) ||
                        p.CPF.Contains(filtro) ||
                        (p.Email != null && p.Email.ToLower().Contains(filtroLower)));
                }

                var total = await query.CountAsync();
                var totalPaginas = (int)Math.Ceiling((double)total / tamanhoPagina);

                var pessoas = await query
                    .OrderBy(p => p.Id)
                    .Skip((pagina - 1) * tamanhoPagina)
                    .Take(tamanhoPagina)
                    .Select(p => MapToDto(p)) 
                    .ToListAsync();

                return new PessoaListResponseDto
                {
                    Pessoas = pessoas,
                    Total = total,
                    Pagina = pagina,
                    TamanhoPagina = tamanhoPagina,
                    TotalPaginas = totalPaginas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter lista de pessoas");
                throw;
            }
        }

        /// <summary>
        /// Obtém uma pessoa por ID
        /// </summary>
        public async Task<PessoaResponseDto?> ObterPorIdAsync(int id)
        {
            try
            {
                var pessoa = await _context.Pessoas
                    .Where(p => p.Id == id && p.Ativo)
                    .FirstOrDefaultAsync();

                return pessoa != null ? MapToDto(pessoa) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pessoa por ID: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtém uma pessoa por CPF
        /// </summary>
        public async Task<PessoaResponseDto?> ObterPorCpfAsync(string cpf)
        {
            try
            {
                var cpfLimpo = _cpfValidationService.RemoverFormatacao(cpf);

                var pessoa = await _context.Pessoas
                    .Where(p => p.CPF == cpfLimpo && p.Ativo)
                    .FirstOrDefaultAsync();

                return pessoa != null ? MapToDto(pessoa) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter pessoa por CPF: {CPF}", cpf);
                throw;
            }
        }

        /// <summary>
        /// Cria uma nova pessoa
        /// </summary>
        public async Task<PessoaResponseDto?> CriarAsync(CriarPessoaDto criarPessoaDto)
        {
            try
            {
                if (!_cpfValidationService.ValidarCpf(criarPessoaDto.CPF))
                {
                    _logger.LogWarning("Tentativa de criar pessoa com CPF inválido: {CPF}", criarPessoaDto.CPF);
                    return null;
                }

                if (await ExisteCpfAsync(criarPessoaDto.CPF))
                {
                    _logger.LogWarning("Tentativa de criar pessoa com CPF já existente: {CPF}", criarPessoaDto.CPF);
                    return null;
                }

                var pessoa = new Pessoa
                {
                    Nome = criarPessoaDto.Nome,
                    Sexo = criarPessoaDto.Sexo,
                    Email = criarPessoaDto.Email,
                    DataNascimento = criarPessoaDto.DataNascimento,
                    Naturalidade = criarPessoaDto.Naturalidade,
                    Nacionalidade = criarPessoaDto.Nacionalidade,
                    CPF = _cpfValidationService.RemoverFormatacao(criarPessoaDto.CPF)
                };

                _context.Pessoas.Add(pessoa);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pessoa criada com sucesso: {Nome} - CPF: {CPF}", pessoa.Nome, pessoa.CPF);

                return MapToDto(pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar pessoa: {Nome}", criarPessoaDto.Nome);
                throw;
            }
        }

        /// <summary>
        /// Atualiza uma pessoa existente
        /// </summary>
        public async Task<PessoaResponseDto?> AtualizarAsync(int id, AtualizarPessoaDto atualizarPessoaDto)
        {
            try
            {
                var pessoa = await _context.Pessoas
                    .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

                if (pessoa == null)
                {
                    _logger.LogWarning("Tentativa de atualizar pessoa inexistente: {Id}", id);
                    return null;
                }

                // CORREÇÃO: Adicionada a lógica para atualizar todos os campos do DTO.
                if (!string.IsNullOrWhiteSpace(atualizarPessoaDto.Nome))
                    pessoa.Nome = atualizarPessoaDto.Nome;
                
                if (atualizarPessoaDto.Sexo != null)
                    pessoa.Sexo = atualizarPessoaDto.Sexo;

                if (atualizarPessoaDto.Email != null)
                    pessoa.Email = atualizarPessoaDto.Email;

                if (atualizarPessoaDto.DataNascimento.HasValue)
                    pessoa.DataNascimento = atualizarPessoaDto.DataNascimento.Value;

                if (atualizarPessoaDto.Naturalidade != null)
                    pessoa.Naturalidade = atualizarPessoaDto.Naturalidade;

                if (atualizarPessoaDto.Nacionalidade != null)
                    pessoa.Nacionalidade = atualizarPessoaDto.Nacionalidade;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Pessoa atualizada com sucesso: {Nome} - ID: {Id}", pessoa.Nome, pessoa.Id);

                return MapToDto(pessoa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar pessoa: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Remove uma pessoa (soft delete)
        /// </summary>
        public async Task<bool> RemoverAsync(int id)
        {
            try
            {
                var pessoa = await _context.Pessoas
                    .FirstOrDefaultAsync(p => p.Id == id && p.Ativo);

                if (pessoa == null)
                {
                    _logger.LogWarning("Tentativa de remover pessoa inexistente: {Id}", id);
                    return false;
                }

                pessoa.Ativo = false;
                
                await _context.SaveChangesAsync();

                _logger.LogInformation("Pessoa removida com sucesso: {Nome} - ID: {Id}", pessoa.Nome, pessoa.Id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover pessoa: {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se um CPF já existe no banco
        /// </summary>
        public async Task<bool> ExisteCpfAsync(string cpf, int? idExcluir = null)
        {
            try
            {
                var cpfLimpo = _cpfValidationService.RemoverFormatacao(cpf);

                var query = _context.Pessoas.Where(p => p.CPF == cpfLimpo && p.Ativo);

                if (idExcluir.HasValue)
                    query = query.Where(p => p.Id != idExcluir.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar existência de CPF: {CPF}", cpf);
                throw;
            }
        }
        
        /// <summary>
        /// Método privado para mapear a entidade Pessoa para o DTO de resposta.
        /// </summary>
        private PessoaResponseDto MapToDto(Pessoa pessoa)
        {
            return new PessoaResponseDto
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Sexo = pessoa.Sexo,
                Email = pessoa.Email,
                DataNascimento = pessoa.DataNascimento,
                Idade = pessoa.Idade,
                Naturalidade = pessoa.Naturalidade,
                Nacionalidade = pessoa.Nacionalidade,
                CPF = pessoa.CPFFormatado,
                DataCadastro = pessoa.DataCadastro,
                DataAtualizacao = pessoa.DataAtualizacao
            };
        }
    }
}

