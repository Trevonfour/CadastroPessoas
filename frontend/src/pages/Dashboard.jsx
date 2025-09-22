import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { Users, UserPlus, Search, LogOut, User, Settings, BarChart3, TrendingUp } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';
import pessoasService from '../services/pessoasService';

const Dashboard = () => {
  const [stats, setStats] = useState({
    totalPessoas: 0,
    pessoasRecentes: 0,
    loading: true
  });
  const [recentPeople, setRecentPeople] = useState([]);
  
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    loadDashboardData();
  }, []);

  const loadDashboardData = async () => {
    try {
      // Carrega estatísticas básicas
      const response = await pessoasService.listarPessoas(1, 5);
      
      setStats({
        totalPessoas: response.total,
        pessoasRecentes: response.pessoas.length,
        loading: false
      });
      
      setRecentPeople(response.pessoas);
    } catch (error) {
      console.error('Erro ao carregar dados do dashboard:', error);
      setStats(prev => ({ ...prev, loading: false }));
    }
  };

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  const menuItems = [
    {
      title: 'Listar Pessoas',
      description: 'Visualizar e gerenciar todas as pessoas cadastradas',
      icon: Users,
      action: () => navigate('/pessoas'),
      color: 'from-blue-500 to-blue-600'
    },
    {
      title: 'Cadastrar Pessoa',
      description: 'Adicionar uma nova pessoa ao sistema',
      icon: UserPlus,
      action: () => navigate('/pessoas/novo'),
      color: 'from-green-500 to-green-600'
    },
    {
      title: 'Buscar Pessoa',
      description: 'Encontrar pessoa por nome, CPF ou email',
      icon: Search,
      action: () => navigate('/pessoas?buscar=true'),
      color: 'from-purple-500 to-purple-600'
    }
  ];

  const statsCards = [
    {
      title: 'Total de Pessoas',
      value: stats.totalPessoas,
      icon: Users,
      color: 'text-blue-600',
      bgColor: 'bg-blue-50',
      change: '+12%',
      changeType: 'positive'
    },
    {
      title: 'Cadastros Recentes',
      value: stats.pessoasRecentes,
      icon: TrendingUp,
      color: 'text-green-600',
      bgColor: 'bg-green-50',
      change: '+5%',
      changeType: 'positive'
    },
    {
      title: 'Taxa de Crescimento',
      value: '8.2%',
      icon: BarChart3,
      color: 'text-purple-600',
      bgColor: 'bg-purple-50',
      change: '+2.1%',
      changeType: 'positive'
    }
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-4">
              <div className="w-8 h-8 bg-gradient-to-r from-blue-600 to-blue-700 rounded-lg flex items-center justify-center">
                <Users className="w-5 h-5 text-white" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-gray-900">Stefanini Cadastro</h1>
                <p className="text-sm text-gray-500">Sistema de Gerenciamento de Pessoas</p>
              </div>
            </div>
            
            <div className="flex items-center space-x-4">
              <div className="flex items-center space-x-2 text-sm text-gray-600">
                <User className="w-4 h-4" />
                <span>Olá, {user?.nomeCompleto || user?.nomeUsuario}</span>
              </div>
              
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/perfil')}
                className="text-gray-600 hover:text-gray-900"
              >
                <Settings className="w-4 h-4" />
              </Button>
              
              <Button
                variant="ghost"
                size="sm"
                onClick={handleLogout}
                className="text-gray-600 hover:text-red-600"
              >
                <LogOut className="w-4 h-4" />
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Welcome Section */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="mb-8"
        >
          <h2 className="text-3xl font-bold text-gray-900 mb-2">
            Bem-vindo ao Dashboard
          </h2>
          <p className="text-gray-600">
            Gerencie pessoas, visualize estatísticas e acesse todas as funcionalidades do sistema.
          </p>
        </motion.div>

        {/* Stats Cards */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
          className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8"
        >
          {statsCards.map((stat, index) => (
            <Card key={index} className="hover:shadow-lg transition-shadow duration-200">
              <CardContent className="p-6">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-gray-600 mb-1">{stat.title}</p>
                    <p className="text-2xl font-bold text-gray-900">
                      {stats.loading ? (
                        <div className="animate-pulse bg-gray-200 h-8 w-16 rounded"></div>
                      ) : (
                        stat.value
                      )}
                    </p>
                    <p className={`text-xs mt-1 ${
                      stat.changeType === 'positive' ? 'text-green-600' : 'text-red-600'
                    }`}>
                      {stat.change} vs mês anterior
                    </p>
                  </div>
                  <div className={`w-12 h-12 ${stat.bgColor} rounded-lg flex items-center justify-center`}>
                    <stat.icon className={`w-6 h-6 ${stat.color}`} />
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </motion.div>

        {/* Quick Actions */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          className="mb-8"
        >
          <h3 className="text-xl font-semibold text-gray-900 mb-4">Ações Rápidas</h3>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
            {menuItems.map((item, index) => (
              <motion.div
                key={index}
                whileHover={{ scale: 1.02 }}
                whileTap={{ scale: 0.98 }}
              >
                <Card 
                  className="cursor-pointer hover:shadow-lg transition-all duration-200 border-0 bg-gradient-to-br from-white to-gray-50"
                  onClick={item.action}
                >
                  <CardContent className="p-6">
                    <div className="flex items-start space-x-4">
                      <div className={`w-12 h-12 bg-gradient-to-r ${item.color} rounded-lg flex items-center justify-center flex-shrink-0`}>
                        <item.icon className="w-6 h-6 text-white" />
                      </div>
                      <div className="flex-1">
                        <h4 className="font-semibold text-gray-900 mb-1">{item.title}</h4>
                        <p className="text-sm text-gray-600">{item.description}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            ))}
          </div>
        </motion.div>

        {/* Recent People */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.3 }}
        >
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <Users className="w-5 h-5" />
                <span>Pessoas Cadastradas Recentemente</span>
              </CardTitle>
              <CardDescription>
                Últimas pessoas adicionadas ao sistema
              </CardDescription>
            </CardHeader>
            <CardContent>
              {stats.loading ? (
                <div className="space-y-3">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="animate-pulse flex items-center space-x-4">
                      <div className="w-10 h-10 bg-gray-200 rounded-full"></div>
                      <div className="flex-1 space-y-2">
                        <div className="h-4 bg-gray-200 rounded w-1/4"></div>
                        <div className="h-3 bg-gray-200 rounded w-1/3"></div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : recentPeople.length > 0 ? (
                <div className="space-y-4">
                  {recentPeople.map((pessoa) => (
                    <div key={pessoa.id} className="flex items-center space-x-4 p-3 rounded-lg hover:bg-gray-50 transition-colors">
                      <div className="w-10 h-10 bg-gradient-to-r from-blue-500 to-blue-600 rounded-full flex items-center justify-center">
                        <User className="w-5 h-5 text-white" />
                      </div>
                      <div className="flex-1">
                        <p className="font-medium text-gray-900">{pessoa.nome}</p>
                        <p className="text-sm text-gray-500">
                          {pessoa.email || 'Email não informado'} • CPF: {pessoa.cpf}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm text-gray-500">
                          {pessoasService.formatarData(pessoa.dataCadastro)}
                        </p>
                        <p className="text-xs text-gray-400">
                          {pessoa.idade} anos
                        </p>
                      </div>
                    </div>
                  ))}
                  
                  <div className="pt-4 border-t">
                    <Button
                      variant="outline"
                      onClick={() => navigate('/pessoas')}
                      className="w-full"
                    >
                      Ver todas as pessoas
                    </Button>
                  </div>
                </div>
              ) : (
                <div className="text-center py-8">
                  <Users className="w-12 h-12 text-gray-300 mx-auto mb-4" />
                  <p className="text-gray-500 mb-4">Nenhuma pessoa cadastrada ainda</p>
                  <Button onClick={() => navigate('/pessoas/novo')}>
                    <UserPlus className="w-4 h-4 mr-2" />
                    Cadastrar primeira pessoa
                  </Button>
                </div>
              )}
            </CardContent>
          </Card>
        </motion.div>
      </main>
    </div>
  );
};

export default Dashboard;
