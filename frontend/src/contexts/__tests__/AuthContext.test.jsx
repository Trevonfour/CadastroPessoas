import { render, screen, waitFor, act } from '@testing-library/react';
import { AuthProvider, useAuth } from '../AuthContext';

// Mock do fetch global
global.fetch = jest.fn();

// Componente de teste para usar o contexto
const TestComponent = () => {
  const { user, token, login, logout, isAuthenticated } = useAuth();
  
  return (
    <div>
      <div data-testid="authenticated">{isAuthenticated ? 'true' : 'false'}</div>
      <div data-testid="user">{user ? user.nomeUsuario : 'null'}</div>
      <div data-testid="token">{token || 'null'}</div>
      <button onClick={() => login({ usuario: 'test', senha: 'test' })}>
        Login
      </button>
      <button onClick={logout}>Logout</button>
    </div>
  );
};

describe('AuthContext', () => {
  beforeEach(() => {
    fetch.mockClear();
    localStorage.clear();
  });

  test('deve inicializar com estado não autenticado', () => {
    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
    expect(screen.getByTestId('user')).toHaveTextContent('null');
    expect(screen.getByTestId('token')).toHaveTextContent('null');
  });

  test('deve carregar dados do localStorage na inicialização', () => {
    const mockUser = { id: 1, nomeUsuario: 'testuser' };
    const mockToken = 'fake-token';

    localStorage.setItem('token', mockToken);
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    expect(screen.getByTestId('authenticated')).toHaveTextContent('true');
    expect(screen.getByTestId('user')).toHaveTextContent('testuser');
    expect(screen.getByTestId('token')).toHaveTextContent('fake-token');
  });

  test('deve fazer login com sucesso', async () => {
    const mockResponse = {
      token: 'new-token',
      usuario: { id: 1, nomeUsuario: 'testuser', email: 'test@email.com' }
    };

    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse
    });

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    const loginButton = screen.getByText('Login');
    
    await act(async () => {
      loginButton.click();
    });

    await waitFor(() => {
      expect(screen.getByTestId('authenticated')).toHaveTextContent('true');
      expect(screen.getByTestId('user')).toHaveTextContent('testuser');
      expect(screen.getByTestId('token')).toHaveTextContent('new-token');
    });

    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/v1/auth/login',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ usuario: 'test', senha: 'test' })
      }
    );

    expect(localStorage.getItem('token')).toBe('new-token');
    expect(JSON.parse(localStorage.getItem('user'))).toEqual(mockResponse.usuario);
  });

  test('deve tratar erro no login', async () => {
    fetch.mockResolvedValueOnce({
      ok: false,
      status: 401,
      json: async () => ({ message: 'Credenciais inválidas' })
    });

    const { login } = useAuth();
    
    // Renderiza o provider para ter acesso ao contexto
    let result;
    const TestLoginComponent = () => {
      const auth = useAuth();
      result = auth;
      return null;
    };

    render(
      <AuthProvider>
        <TestLoginComponent />
      </AuthProvider>
    );

    const loginResult = await act(async () => {
      return await result.login({ usuario: 'wrong', senha: 'wrong' });
    });

    expect(loginResult.success).toBe(false);
    expect(loginResult.error).toBe('Credenciais inválidas');
  });

  test('deve fazer logout corretamente', async () => {
    const mockUser = { id: 1, nomeUsuario: 'testuser' };
    const mockToken = 'fake-token';

    localStorage.setItem('token', mockToken);
    localStorage.setItem('user', JSON.stringify(mockUser));

    render(
      <AuthProvider>
        <TestComponent />
      </AuthProvider>
    );

    // Verifica se está autenticado inicialmente
    expect(screen.getByTestId('authenticated')).toHaveTextContent('true');

    const logoutButton = screen.getByText('Logout');
    
    act(() => {
      logoutButton.click();
    });

    expect(screen.getByTestId('authenticated')).toHaveTextContent('false');
    expect(screen.getByTestId('user')).toHaveTextContent('null');
    expect(screen.getByTestId('token')).toHaveTextContent('null');

    expect(localStorage.getItem('token')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });

  test('deve registrar usuário com sucesso', async () => {
    const mockResponse = {
      token: 'new-token',
      usuario: { id: 1, nomeUsuario: 'newuser', email: 'new@email.com' }
    };

    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse
    });

    let result;
    const TestRegisterComponent = () => {
      const auth = useAuth();
      result = auth;
      return null;
    };

    render(
      <AuthProvider>
        <TestRegisterComponent />
      </AuthProvider>
    );

    const registerData = {
      nomeUsuario: 'newuser',
      email: 'new@email.com',
      nomeCompleto: 'New User',
      senha: 'password123',
      confirmarSenha: 'password123'
    };

    const registerResult = await act(async () => {
      return await result.register(registerData);
    });

    expect(registerResult.success).toBe(true);
    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/v1/auth/registro',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(registerData)
      }
    );
  });

  test('deve recuperar senha com sucesso', async () => {
    const mockResponse = {
      message: 'Email de recuperação enviado'
    };

    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => mockResponse
    });

    let result;
    const TestRecoverComponent = () => {
      const auth = useAuth();
      result = auth;
      return null;
    };

    render(
      <AuthProvider>
        <TestRecoverComponent />
      </AuthProvider>
    );

    const recoverResult = await act(async () => {
      return await result.recoverPassword('test@email.com');
    });

    expect(recoverResult.success).toBe(true);
    expect(recoverResult.message).toBe('Email de recuperação enviado');
    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/v1/auth/recuperar-senha',
      {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email: 'test@email.com' })
      }
    );
  });

  test('deve validar token com sucesso', async () => {
    fetch.mockResolvedValueOnce({
      ok: true,
      json: async () => ({ valido: true })
    });

    localStorage.setItem('token', 'valid-token');

    let result;
    const TestValidateComponent = () => {
      const auth = useAuth();
      result = auth;
      return null;
    };

    render(
      <AuthProvider>
        <TestValidateComponent />
      </AuthProvider>
    );

    const isValid = await act(async () => {
      return await result.validateToken();
    });

    expect(isValid).toBe(true);
    expect(fetch).toHaveBeenCalledWith(
      'http://localhost:5000/api/v1/auth/validar-token',
      {
        headers: {
          'Authorization': 'Bearer valid-token'
        }
      }
    );
  });

  test('deve fazer logout quando token é inválido', async () => {
    fetch.mockResolvedValueOnce({
      ok: false,
      status: 401
    });

    localStorage.setItem('token', 'invalid-token');
    localStorage.setItem('user', JSON.stringify({ id: 1, nomeUsuario: 'test' }));

    let result;
    const TestValidateComponent = () => {
      const auth = useAuth();
      result = auth;
      return null;
    };

    render(
      <AuthProvider>
        <TestValidateComponent />
      </AuthProvider>
    );

    const isValid = await act(async () => {
      return await result.validateToken();
    });

    expect(isValid).toBe(false);
    expect(localStorage.getItem('token')).toBeNull();
    expect(localStorage.getItem('user')).toBeNull();
  });
});
