const API_BASE_URL = 'https://localhost:64138/api';

class PessoasService {
  constructor() {
    this.baseURL = API_BASE_URL;
  }

  // Obtém o token do localStorage
  getAuthHeaders() {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      'Authorization': token ? `Bearer ${token}` : '',
    };
  }

  // Trata erros da API
  async handleResponse(response) {
    if (!response.ok) {
      const error = await response.json().catch(() => ({ message: 'Erro desconhecido' }));
      throw new Error(error.message || `Erro ${response.status}`);
    }
    return response.json();
  }

  // Lista pessoas com paginação e filtro
  async listarPessoas(pagina = 1, tamanhoPagina = 10, filtro = '') {
    try {
      const params = new URLSearchParams({
        pagina: pagina.toString(),
        tamanhoPagina: tamanhoPagina.toString(),
      });

      if (filtro) {
        params.append('filtro', filtro);
      }

      const response = await fetch(`${this.baseURL}/pessoas?${params}`, {
        headers: this.getAuthHeaders(),
      });

      return await this.handleResponse(response);
    } catch (error) {
      throw new Error(`Erro ao listar pessoas: ${error.message}`);
    }
  }

  // Obtém uma pessoa por ID
  async obterPessoa(id) {
    try {
      const response = await fetch(`${this.baseURL}/v1/pessoas/${id}`, {
        headers: this.getAuthHeaders(),
      });

      return await this.handleResponse(response);
    } catch (error) {
      throw new Error(`Erro ao obter pessoa: ${error.message}`);
    }
  }

  // Obtém uma pessoa por CPF
  async obterPessoaPorCpf(cpf) {
    try {
      const response = await fetch(`${this.baseURL}/pessoas/cpf/${cpf}`, {
        headers: this.getAuthHeaders(),
      });

      return await this.handleResponse(response);
    } catch (error) {
      throw new Error(`Erro ao obter pessoa por CPF: ${error.message}`);
    }
  }

  // Cria uma nova pessoa
  async criarPessoa(dadosPessoa) {
    try {
      const response = await fetch(`${this.baseURL}/pessoas`, {
        method: 'POST',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(dadosPessoa),
      });

      return await this.handleResponse(response);
    } catch (error) {
      throw new Error(`Erro ao criar pessoa: ${error.message}`);
    }
  }

  // Atualiza uma pessoa existente
  async atualizarPessoa(id, dadosPessoa) {
    try {
      const response = await fetch(`${this.baseURL}/pessoas/${id}`, {
        method: 'PUT',
        headers: this.getAuthHeaders(),
        body: JSON.stringify(dadosPessoa),
      });

      return await this.handleResponse(response);
    } catch (error) {
      throw new Error(`Erro ao atualizar pessoa: ${error.message}`);
    }
  }

  // Remove uma pessoa
  async removerPessoa(id) {
    try {
      const response = await fetch(`${this.baseURL}/pessoas/${id}`, {
        method: 'DELETE',
        headers: this.getAuthHeaders(),
      });

      if (!response.ok) {
        const error = await response.json().catch(() => ({ message: 'Erro desconhecido' }));
        throw new Error(error.message || `Erro ${response.status}`);
      }

      return true;
    } catch (error) {
      throw new Error(`Erro ao remover pessoa: ${error.message}`);
    }
  }

  // Verifica se um CPF já existe
  async verificarCpf(cpf, idExcluir = null) {
    try {
      const params = new URLSearchParams();
      if (idExcluir) {
        params.append('idExcluir', idExcluir.toString());
      }

      const url = `${this.baseURL}/pessoas/verificar-cpf/${cpf}${params.toString() ? `?${params}` : ''}`;
      
      const response = await fetch(url, {
        headers: this.getAuthHeaders(),
      });

      const data = await this.handleResponse(response);
      return data.existe;
    } catch (error) {
      throw new Error(`Erro ao verificar CPF: ${error.message}`);
    }
  }

  // Valida CPF (algoritmo cliente)
  validarCpf(cpf) {
    // Remove formatação
    cpf = cpf.replace(/[^\d]/g, '');

    // Verifica se tem 11 dígitos
    if (cpf.length !== 11) return false;

    // Verifica se todos os dígitos são iguais
    if (/^(\d)\1{10}$/.test(cpf)) return false;

    // Calcula os dígitos verificadores
    let soma = 0;
    for (let i = 0; i < 9; i++) {
      soma += parseInt(cpf[i]) * (10 - i);
    }
    let resto = soma % 11;
    let dv1 = resto < 2 ? 0 : 11 - resto;

    if (parseInt(cpf[9]) !== dv1) return false;

    soma = 0;
    for (let i = 0; i < 10; i++) {
      soma += parseInt(cpf[i]) * (11 - i);
    }
    resto = soma % 11;
    let dv2 = resto < 2 ? 0 : 11 - resto;

    return parseInt(cpf[10]) === dv2;
  }

  // Formata CPF
  formatarCpf(cpf) {
    cpf = cpf.replace(/[^\d]/g, '');
    if (cpf.length === 11) {
      return cpf.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
    }
    return cpf;
  }

  // Remove formatação do CPF
  limparCpf(cpf) {
    return cpf.replace(/[^\d]/g, '');
  }

  // Valida email
  validarEmail(email) {
    const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return regex.test(email);
  }

  // Calcula idade
  calcularIdade(dataNascimento) {
    const hoje = new Date();
    const nascimento = new Date(dataNascimento);
    let idade = hoje.getFullYear() - nascimento.getFullYear();
    const mes = hoje.getMonth() - nascimento.getMonth();
    
    if (mes < 0 || (mes === 0 && hoje.getDate() < nascimento.getDate())) {
      idade--;
    }
    
    return idade;
  }

  // Formata data para exibição
  formatarData(data) {
    return new Date(data).toLocaleDateString('pt-BR');
  }

  // Formata data para input
  formatarDataInput(data) {
    return new Date(data).toISOString().split('T')[0];
  }
}

export default new PessoasService();
