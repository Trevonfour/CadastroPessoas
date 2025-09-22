import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

// Páginas de autenticação
import Login from './pages/Login';
import Registro from './pages/Registro';
import RecuperarSenha from './pages/RecuperarSenha';
import RedefinirSenha from './pages/RedefinirSenha';

// Páginas principais
import Dashboard from './pages/Dashboard';
import Pessoas from './pages/Pessoas';
import PessoaForm from './pages/PessoaForm';

import './App.css';

function App() {
  return (
    <AuthProvider>
      <Router>
        <div className="App">
          <Routes>
            {/* Rotas públicas */}
            <Route path="/login" element={<Login />} />
            <Route path="/registro" element={<Registro />} />
            <Route path="/recuperar-senha" element={<RecuperarSenha />} />
            <Route path="/redefinir-senha" element={<RedefinirSenha />} />

            {/* Rotas protegidas */}
            <Route path="/dashboard" element={
              <ProtectedRoute>
                <Dashboard />
              </ProtectedRoute>
            } />
            
            <Route path="/pessoas" element={
              <ProtectedRoute>
                <Pessoas />
              </ProtectedRoute>
            } />
            
            <Route path="/pessoas/novo" element={
              <ProtectedRoute>
                <PessoaForm />
              </ProtectedRoute>
            } />
            
            <Route path="/pessoas/:id/editar" element={
              <ProtectedRoute>
                <PessoaForm />
              </ProtectedRoute>
            } />

            {/* Redirecionamentos */}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
            <Route path="*" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </div>
      </Router>
    </AuthProvider>
  );
}

export default App;
