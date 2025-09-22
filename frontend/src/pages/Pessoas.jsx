import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { 
  Users, UserPlus, Search, Edit, Trash2, Eye, Filter, 
  ChevronLeft, ChevronRight, ArrowLeft, RefreshCw, Download 
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useNavigate, useSearchParams } from 'react-router-dom';
import pessoasService from '../services/pessoasService';

const Pessoas = () => {
  const [pessoas, setPessoas] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [filtro, setFiltro] = useState('');
  const [paginacao, setPaginacao] = useState({
    pagina: 1,
    tamanhoPagina: 10,
    total: 0,
    totalPaginas: 0
  });
  const [deletingId, setDeletingId] = useState(null);

  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  useEffect(() => {
    const buscar = searchParams.get('buscar');
    if (buscar) {
      // Foca no campo de busca se veio do dashboard
      setTimeout(() => {
        document.getElementById('filtro')?.focus();
      }, 100);
    }
    
    carregarPessoas();
  }, [paginacao.pagina, searchParams]);

  const carregarPessoas = async () => {
    try {
      setLoading(true);
      setError('');
      
      const response = await pessoasService.listarPessoas(
        paginacao.pagina,
        paginacao.tamanhoPagina,
        filtro
      );
      
      setPessoas(response.pessoas);
      setPaginacao(prev => ({
        ...prev,
        total: response.total,
        totalPaginas: response.totalPaginas
      }));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  const handleFiltroChange = (e) => {
    setFiltro(e.target.value);
  };

  const handleBuscar = () => {
    setPaginacao(prev => ({ ...prev, pagina: 1 }));
    carregarPessoas();
  };

  const handleKeyPress = (e) => {
    if (e.key === 'Enter') {
      handleBuscar();
    }
  };

  const handlePaginaChange = (novaPagina) => {
    setPaginacao(prev => ({ ...prev, pagina: novaPagina }));
  };

  const handleRemover = async (id, nome) => {
    if (!window.confirm(`Tem certeza que deseja remover ${nome}?`)) {
      return;
    }

    try {
      setDeletingId(id);
      await pessoasService.removerPessoa(id);
      await carregarPessoas();
    } catch (err) {
      setError(err.message);
    } finally {
      setDeletingId(null);
    }
  };

  const exportarDados = () => {
    // Simula exportação dos dados
    const csvContent = [
      ['Nome', 'CPF', 'Email', 'Idade', 'Naturalidade', 'Data Cadastro'].join(','),
      ...pessoas.map(p => [
        p.nome,
        p.cpf,
        p.email || '',
        p.idade,
        p.naturalidade || '',
        pessoasService.formatarData(p.dataCadastro)
      ].join(','))
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv' });
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'pessoas.csv';
    a.click();
    window.URL.revokeObjectURL(url);
  };

  const renderPaginacao = () => {
    const paginas = [];
    const maxPaginas = 5;
    let inicio = Math.max(1, paginacao.pagina - Math.floor(maxPaginas / 2));
    let fim = Math.min(paginacao.totalPaginas, inicio + maxPaginas - 1);
    
    if (fim - inicio < maxPaginas - 1) {
      inicio = Math.max(1, fim - maxPaginas + 1);
    }

    for (let i = inicio; i <= fim; i++) {
      paginas.push(
        <Button
          key={i}
          variant={i === paginacao.pagina ? "default" : "outline"}
          size="sm"
          onClick={() => handlePaginaChange(i)}
          className="w-10 h-10"
        >
          {i}
        </Button>
      );
    }

    return paginas;
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-4">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/dashboard')}
                className="text-gray-600 hover:text-gray-900"
              >
                <ArrowLeft className="w-4 h-4 mr-2" />
                Voltar
              </Button>
              <div className="w-8 h-8 bg-gradient-to-r from-blue-600 to-blue-700 rounded-lg flex items-center justify-center">
                <Users className="w-5 h-5 text-white" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-gray-900">Gerenciar Pessoas</h1>
                <p className="text-sm text-gray-500">
                  {paginacao.total} pessoa{paginacao.total !== 1 ? 's' : ''} cadastrada{paginacao.total !== 1 ? 's' : ''}
                </p>
              </div>
            </div>
            
            <div className="flex items-center space-x-3">
              <Button
                variant="outline"
                size="sm"
                onClick={exportarDados}
                disabled={pessoas.length === 0}
              >
                <Download className="w-4 h-4 mr-2" />
                Exportar
              </Button>
              
              <Button
                onClick={() => navigate('/pessoas/novo')}
                className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
              >
                <UserPlus className="w-4 h-4 mr-2" />
                Nova Pessoa
              </Button>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        {/* Filtros */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="mb-6"
        >
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <Filter className="w-5 h-5" />
                <span>Filtros e Busca</span>
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex space-x-4">
                <div className="flex-1">
                  <div className="relative">
                    <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                    <Input
                      id="filtro"
                      placeholder="Buscar por nome, CPF ou email..."
                      value={filtro}
                      onChange={handleFiltroChange}
                      onKeyPress={handleKeyPress}
                      className="pl-10"
                    />
                  </div>
                </div>
                <Button onClick={handleBuscar} disabled={loading}>
                  {loading ? (
                    <RefreshCw className="w-4 h-4 animate-spin" />
                  ) : (
                    <Search className="w-4 h-4" />
                  )}
                </Button>
              </div>
            </CardContent>
          </Card>
        </motion.div>

        {/* Error Alert */}
        {error && (
          <motion.div
            initial={{ opacity: 0, scale: 0.95 }}
            animate={{ opacity: 1, scale: 1 }}
            className="mb-6"
          >
            <Alert variant="destructive">
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          </motion.div>
        )}

        {/* Lista de Pessoas */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.1 }}
        >
          <Card>
            <CardHeader>
              <CardTitle>Lista de Pessoas</CardTitle>
              <CardDescription>
                Gerencie todas as pessoas cadastradas no sistema
              </CardDescription>
            </CardHeader>
            <CardContent>
              {loading ? (
                <div className="space-y-4">
                  {[1, 2, 3, 4, 5].map((i) => (
                    <div key={i} className="animate-pulse flex items-center space-x-4 p-4 border rounded-lg">
                      <div className="w-12 h-12 bg-gray-200 rounded-full"></div>
                      <div className="flex-1 space-y-2">
                        <div className="h-4 bg-gray-200 rounded w-1/4"></div>
                        <div className="h-3 bg-gray-200 rounded w-1/3"></div>
                      </div>
                      <div className="flex space-x-2">
                        <div className="w-8 h-8 bg-gray-200 rounded"></div>
                        <div className="w-8 h-8 bg-gray-200 rounded"></div>
                        <div className="w-8 h-8 bg-gray-200 rounded"></div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : pessoas.length > 0 ? (
                <div className="space-y-4">
                  {pessoas.map((pessoa, index) => (
                    <motion.div
                      key={pessoa.id}
                      initial={{ opacity: 0, x: -20 }}
                      animate={{ opacity: 1, x: 0 }}
                      transition={{ duration: 0.3, delay: index * 0.1 }}
                      className="flex items-center space-x-4 p-4 border rounded-lg hover:bg-gray-50 transition-colors"
                    >
                      <div className="w-12 h-12 bg-gradient-to-r from-blue-500 to-blue-600 rounded-full flex items-center justify-center flex-shrink-0">
                        <span className="text-white font-semibold">
                          {pessoa.nome.charAt(0).toUpperCase()}
                        </span>
                      </div>
                      
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center space-x-2 mb-1">
                          <h3 className="font-semibold text-gray-900 truncate">
                            {pessoa.nome}
                          </h3>
                          <Badge variant="secondary" className="text-xs">
                            {pessoa.sexo === 'M' ? 'Masculino' : 'Feminino'}
                          </Badge>
                        </div>
                        <div className="flex items-center space-x-4 text-sm text-gray-500">
                          <span>CPF: {pessoa.cpf}</span>
                          <span>•</span>
                          <span>{pessoa.idade} anos</span>
                          {pessoa.email && (
                            <>
                              <span>•</span>
                              <span className="truncate">{pessoa.email}</span>
                            </>
                          )}
                        </div>
                        <div className="text-xs text-gray-400 mt-1">
                          Cadastrado em {pessoasService.formatarData(pessoa.dataCadastro)}
                        </div>
                      </div>
                      
                      <div className="flex items-center space-x-2 flex-shrink-0">
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/pessoas/${pessoa.id}`)}
                          className="text-blue-600 hover:text-blue-700 hover:bg-blue-50"
                        >
                          <Eye className="w-4 h-4" />
                        </Button>
                        
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => navigate(`/pessoas/${pessoa.id}/editar`)}
                          className="text-green-600 hover:text-green-700 hover:bg-green-50"
                        >
                          <Edit className="w-4 h-4" />
                        </Button>
                        
                        <Button
                          variant="ghost"
                          size="sm"
                          onClick={() => handleRemover(pessoa.id, pessoa.nome)}
                          disabled={deletingId === pessoa.id}
                          className="text-red-600 hover:text-red-700 hover:bg-red-50"
                        >
                          {deletingId === pessoa.id ? (
                            <RefreshCw className="w-4 h-4 animate-spin" />
                          ) : (
                            <Trash2 className="w-4 h-4" />
                          )}
                        </Button>
                      </div>
                    </motion.div>
                  ))}
                </div>
              ) : (
                <div className="text-center py-12">
                  <Users className="w-16 h-16 text-gray-300 mx-auto mb-4" />
                  <h3 className="text-lg font-medium text-gray-900 mb-2">
                    {filtro ? 'Nenhuma pessoa encontrada' : 'Nenhuma pessoa cadastrada'}
                  </h3>
                  <p className="text-gray-500 mb-6">
                    {filtro 
                      ? 'Tente ajustar os filtros de busca ou cadastre uma nova pessoa.'
                      : 'Comece cadastrando a primeira pessoa no sistema.'
                    }
                  </p>
                  <Button
                    onClick={() => navigate('/pessoas/novo')}
                    className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
                  >
                    <UserPlus className="w-4 h-4 mr-2" />
                    Cadastrar Pessoa
                  </Button>
                </div>
              )}

              {/* Paginação */}
              {paginacao.totalPaginas > 1 && (
                <div className="flex items-center justify-between pt-6 border-t">
                  <div className="text-sm text-gray-500">
                    Mostrando {((paginacao.pagina - 1) * paginacao.tamanhoPagina) + 1} a{' '}
                    {Math.min(paginacao.pagina * paginacao.tamanhoPagina, paginacao.total)} de{' '}
                    {paginacao.total} resultados
                  </div>
                  
                  <div className="flex items-center space-x-2">
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePaginaChange(paginacao.pagina - 1)}
                      disabled={paginacao.pagina === 1}
                    >
                      <ChevronLeft className="w-4 h-4" />
                    </Button>
                    
                    {renderPaginacao()}
                    
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handlePaginaChange(paginacao.pagina + 1)}
                      disabled={paginacao.pagina === paginacao.totalPaginas}
                    >
                      <ChevronRight className="w-4 h-4" />
                    </Button>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </motion.div>
      </main>
    </div>
  );
};

export default Pessoas;
