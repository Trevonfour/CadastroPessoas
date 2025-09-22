import pessoasService from '../pessoasService';

// Mock do fetch global
global.fetch = jest.fn();

describe('PessoasService', () => {
  beforeEach(() => {
    fetch.mockClear();
    localStorage.clear();
  });

  describe('validarCpf', () => {
    test('deve validar CPF correto', () => {
      expect(pessoasService.validarCpf('11144477735')).toBe(true);
      expect(pessoasService.validarCpf('111.444.777-35')).toBe(true);
    });

    test('deve rejeitar CPF inválido', () => {
      expect(pessoasService.validarCpf('11111111111')).toBe(false);
      expect(pessoasService.validarCpf('12345678901')).toBe(false);
      expect(pessoasService.validarCpf('123')).toBe(false);
    });
  });

  describe('formatarCpf', () => {
    test('deve formatar CPF corretamente', () => {
      expect(pessoasService.formatarCpf('11144477735')).toBe('111.444.777-35');
    });

    test('deve retornar CPF sem formatação se incompleto', () => {
      expect(pessoasService.formatarCpf('111444')).toBe('111444');
    });
  });

  describe('validarEmail', () => {
    test('deve validar email correto', () => {
      expect(pessoasService.validarEmail('teste@email.com')).toBe(true);
      expect(pessoasService.validarEmail('usuario.teste@dominio.com.br')).toBe(true);
    });

    test('deve rejeitar email inválido', () => {
      expect(pessoasService.validarEmail('email-invalido')).toBe(false);
      expect(pessoasService.validarEmail('teste@')).toBe(false);
      expect(pessoasService.validarEmail('@dominio.com')).toBe(false);
    });
  });

  describe('calcularIdade', () => {
    test('deve calcular idade corretamente', () => {
      const dataHoje = new Date();
      const anoAtual = dataHoje.getFullYear();
      const dataNascimento = `${anoAtual - 25}-01-01`;
      
      const idade = pessoasService.calcularIdade(dataNascimento);
      expect(idade).toBeGreaterThanOrEqual(24);
      expect(idade).toBeLessThanOrEqual(25);
    });
  });

  describe('listarPessoas', () => {
    test('deve fazer requisição correta para listar pessoas', async () => {
      const mockResponse = {
        pessoas: [
          { id: 1, nome: 'João Silva', cpf: '111.444.777-35' }
        ],
        total: 1,
        pagina: 1,
        totalPaginas: 1
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.listarPessoas(1, 10, 'João');

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas?pagina=1&tamanhoPagina=10&filtro=Jo%C3%A3o',
        {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          }
        }
      );

      expect(resultado).toEqual(mockResponse);
    });

    test('deve tratar erro na requisição', async () => {
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 500,
        json: async () => ({ message: 'Erro interno' })
      });

      localStorage.setItem('token', 'fake-token');

      await expect(pessoasService.listarPessoas()).rejects.toThrow('Erro ao listar pessoas');
    });
  });

  describe('criarPessoa', () => {
    test('deve criar pessoa com sucesso', async () => {
      const novaPessoa = {
        nome: 'Maria Silva',
        cpf: '98765432109',
        sexo: 'F',
        dataNascimento: '1990-01-01'
      };

      const mockResponse = {
        id: 1,
        ...novaPessoa,
        cpf: '987.654.321-09'
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.criarPessoa(novaPessoa);

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas',
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          },
          body: JSON.stringify(novaPessoa)
        }
      );

      expect(resultado).toEqual(mockResponse);
    });
  });

  describe('atualizarPessoa', () => {
    test('deve atualizar pessoa com sucesso', async () => {
      const dadosAtualizacao = {
        nome: 'João Santos Silva',
        email: 'joao.santos@email.com'
      };

      const mockResponse = {
        id: 1,
        ...dadosAtualizacao,
        cpf: '111.444.777-35'
      };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.atualizarPessoa(1, dadosAtualizacao);

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas/1',
        {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          },
          body: JSON.stringify(dadosAtualizacao)
        }
      );

      expect(resultado).toEqual(mockResponse);
    });
  });

  describe('removerPessoa', () => {
    test('deve remover pessoa com sucesso', async () => {
      fetch.mockResolvedValueOnce({
        ok: true
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.removerPessoa(1);

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas/1',
        {
          method: 'DELETE',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          }
        }
      );

      expect(resultado).toBe(true);
    });

    test('deve tratar erro ao remover pessoa', async () => {
      fetch.mockResolvedValueOnce({
        ok: false,
        status: 404,
        json: async () => ({ message: 'Pessoa não encontrada' })
      });

      localStorage.setItem('token', 'fake-token');

      await expect(pessoasService.removerPessoa(999)).rejects.toThrow('Erro ao remover pessoa');
    });
  });

  describe('verificarCpf', () => {
    test('deve verificar se CPF existe', async () => {
      const mockResponse = { existe: true };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.verificarCpf('11144477735');

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas/verificar-cpf/11144477735',
        {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          }
        }
      );

      expect(resultado).toBe(true);
    });

    test('deve verificar CPF excluindo ID específico', async () => {
      const mockResponse = { existe: false };

      fetch.mockResolvedValueOnce({
        ok: true,
        json: async () => mockResponse
      });

      localStorage.setItem('token', 'fake-token');

      const resultado = await pessoasService.verificarCpf('11144477735', 1);

      expect(fetch).toHaveBeenCalledWith(
        'http://localhost:5000/api/v1/pessoas/verificar-cpf/11144477735?idExcluir=1',
        {
          headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer fake-token'
          }
        }
      );

      expect(resultado).toBe(false);
    });
  });

  describe('formatarData', () => {
    test('deve formatar data corretamente', () => {
      const data = '2024-01-15T10:30:00Z';
      const resultado = pessoasService.formatarData(data);
      
      // Verifica se está no formato brasileiro
      expect(resultado).toMatch(/^\d{2}\/\d{2}\/\d{4}$/);
    });
  });

  describe('formatarDataInput', () => {
    test('deve formatar data para input corretamente', () => {
      const data = '2024-01-15T10:30:00Z';
      const resultado = pessoasService.formatarDataInput(data);
      
      expect(resultado).toBe('2024-01-15');
    });
  });
});
