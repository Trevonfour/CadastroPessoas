import { useState, useEffect } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, useParams } from 'react-router-dom';
import { 
  ArrowLeft, Save, User, Mail, Calendar, MapPin, 
  AlertCircle, CheckCircle, RefreshCw 
} from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import pessoasService from '../services/pessoasService';

const PessoaForm = () => {
  const { id } = useParams();
  const navigate = useNavigate();
  const isEditing = !!id;

  const [formData, setFormData] = useState({
    nome: '',
    sexo: '',
    email: '',
    dataNascimento: '',
    naturalidade: '',
    nacionalidade: 'Brasileira',
    cpf: ''
  });

  const [loading, setLoading] = useState(false);
  const [loadingData, setLoadingData] = useState(isEditing);
  const [error, setError] = useState('');
  const [success, setSuccess] = useState(false);
  const [cpfError, setCpfError] = useState('');
  const [emailError, setEmailError] = useState('');

  useEffect(() => {
    if (isEditing) {
      carregarPessoa();
    }
  }, [id, isEditing]);

  const carregarPessoa = async () => {
    try {
      setLoadingData(true);
      const pessoa = await pessoasService.obterPessoa(id);
      
      setFormData({
        nome: pessoa.nome,
        sexo: pessoa.sexo,
        email: pessoa.email || '',
        dataNascimento: pessoasService.formatarDataInput(pessoa.dataNascimento),
        naturalidade: pessoa.naturalidade || '',
        nacionalidade: pessoa.nacionalidade || 'Brasileira',
        cpf: pessoa.cpf
      });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoadingData(false);
    }
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    
    // Formatação especial para CPF
    if (name === 'cpf') {
      const cpfLimpo = value.replace(/[^\d]/g, '');
      const cpfFormatado = pessoasService.formatarCpf(cpfLimpo);
      setFormData(prev => ({ ...prev, [name]: cpfFormatado }));
      
      // Validação em tempo real do CPF
      if (cpfLimpo.length === 11) {
        if (pessoasService.validarCpf(cpfLimpo)) {
          setCpfError('');
        } else {
          setCpfError('CPF inválido');
        }
      } else {
        setCpfError('');
      }
    } else {
      setFormData(prev => ({ ...prev, [name]: value }));
    }

    // Validação em tempo real do email
    if (name === 'email') {
      if (value && !pessoasService.validarEmail(value)) {
        setEmailError('Email deve ter formato válido');
      } else {
        setEmailError('');
      }
    }

    // Limpa erros gerais
    if (error) setError('');
  };

  const handleSelectChange = (name, value) => {
    setFormData(prev => ({ ...prev, [name]: value }));
    if (error) setError('');
  };

  const validateForm = () => {
    if (!formData.nome.trim()) {
      return 'Nome é obrigatório';
    }
    if (!formData.sexo) {
      return 'Sexo é obrigatório';
    }
    if (!formData.dataNascimento) {
      return 'Data de nascimento é obrigatória';
    }
    if (!formData.cpf) {
      return 'CPF é obrigatório';
    }
    if (!pessoasService.validarCpf(formData.cpf)) {
      return 'CPF inválido';
    }
    if (formData.email && !pessoasService.validarEmail(formData.email)) {
      return 'Email deve ter formato válido';
    }

    // Validação de data de nascimento
    const dataNasc = new Date(formData.dataNascimento);
    const hoje = new Date();
    if (dataNasc >= hoje) {
      return 'Data de nascimento deve ser no passado';
    }

    const idade = pessoasService.calcularIdade(formData.dataNascimento);
    if (idade > 150) {
      return 'Idade não pode ser superior a 150 anos';
    }

    return null;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError('');

    const validationError = validateForm();
    if (validationError) {
      setError(validationError);
      setLoading(false);
      return;
    }

    try {
      const dadosPessoa = {
        ...formData,
        cpf: pessoasService.limparCpf(formData.cpf),
        email: formData.email || null
      };

      if (isEditing) {
        await pessoasService.atualizarPessoa(id, dadosPessoa);
      } else {
        await pessoasService.criarPessoa(dadosPessoa);
      }

      setSuccess(true);
      setTimeout(() => {
        navigate('/pessoas');
      }, 2000);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (loadingData) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="text-center">
          <RefreshCw className="w-8 h-8 animate-spin text-blue-600 mx-auto mb-4" />
          <p className="text-gray-600">Carregando dados da pessoa...</p>
        </div>
      </div>
    );
  }

  if (success) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-green-50 via-white to-green-50 flex items-center justify-center p-4">
        <motion.div
          initial={{ opacity: 0, scale: 0.9 }}
          animate={{ opacity: 1, scale: 1 }}
          transition={{ duration: 0.5 }}
          className="w-full max-w-md"
        >
          <Card className="shadow-xl border-0 bg-white/80 backdrop-blur-sm text-center">
            <CardContent className="pt-8 pb-8">
              <motion.div
                initial={{ scale: 0 }}
                animate={{ scale: 1 }}
                transition={{ delay: 0.2, type: "spring", stiffness: 200 }}
                className="mx-auto w-16 h-16 bg-gradient-to-r from-green-600 to-green-700 rounded-full flex items-center justify-center mb-4"
              >
                <CheckCircle className="w-8 h-8 text-white" />
              </motion.div>
              <h2 className="text-2xl font-bold text-gray-900 mb-2">
                {isEditing ? 'Pessoa atualizada!' : 'Pessoa cadastrada!'}
              </h2>
              <p className="text-gray-600 mb-4">
                {isEditing 
                  ? 'Os dados foram atualizados com sucesso.'
                  : 'A pessoa foi cadastrada com sucesso no sistema.'
                }
              </p>
              <div className="animate-spin rounded-full h-6 w-6 border-b-2 border-green-600 mx-auto"></div>
            </CardContent>
          </Card>
        </motion.div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <header className="bg-white shadow-sm border-b">
        <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between items-center h-16">
            <div className="flex items-center space-x-4">
              <Button
                variant="ghost"
                size="sm"
                onClick={() => navigate('/pessoas')}
                className="text-gray-600 hover:text-gray-900"
              >
                <ArrowLeft className="w-4 h-4 mr-2" />
                Voltar
              </Button>
              <div className="w-8 h-8 bg-gradient-to-r from-blue-600 to-blue-700 rounded-lg flex items-center justify-center">
                <User className="w-5 h-5 text-white" />
              </div>
              <div>
                <h1 className="text-xl font-semibold text-gray-900">
                  {isEditing ? 'Editar Pessoa' : 'Cadastrar Nova Pessoa'}
                </h1>
                <p className="text-sm text-gray-500">
                  {isEditing ? 'Atualize os dados da pessoa' : 'Preencha os dados da nova pessoa'}
                </p>
              </div>
            </div>
          </div>
        </div>
      </header>

      {/* Main Content */}
      <main className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
        >
          <Card className="shadow-lg">
            <CardHeader>
              <CardTitle className="flex items-center space-x-2">
                <User className="w-5 h-5" />
                <span>Dados Pessoais</span>
              </CardTitle>
              <CardDescription>
                Preencha todas as informações obrigatórias marcadas com *
              </CardDescription>
            </CardHeader>

            <form onSubmit={handleSubmit}>
              <CardContent className="space-y-6">
                {error && (
                  <motion.div
                    initial={{ opacity: 0, scale: 0.95 }}
                    animate={{ opacity: 1, scale: 1 }}
                    transition={{ duration: 0.2 }}
                  >
                    <Alert variant="destructive">
                      <AlertCircle className="h-4 w-4" />
                      <AlertDescription>{error}</AlertDescription>
                    </Alert>
                  </motion.div>
                )}

                {/* Nome */}
                <div className="space-y-2">
                  <Label htmlFor="nome" className="text-sm font-medium">
                    Nome Completo *
                  </Label>
                  <Input
                    id="nome"
                    name="nome"
                    type="text"
                    placeholder="Digite o nome completo"
                    value={formData.nome}
                    onChange={handleChange}
                    className="h-11"
                    required
                  />
                </div>

                {/* Sexo e CPF */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="sexo" className="text-sm font-medium">
                      Sexo *
                    </Label>
                    <Select
                      value={formData.sexo}
                      onValueChange={(value) => handleSelectChange('sexo', value)}
                    >
                      <SelectTrigger className="h-11">
                        <SelectValue placeholder="Selecione o sexo" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="M">Masculino</SelectItem>
                        <SelectItem value="F">Feminino</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="cpf" className="text-sm font-medium">
                      CPF *
                    </Label>
                    <Input
                      id="cpf"
                      name="cpf"
                      type="text"
                      placeholder="000.000.000-00"
                      value={formData.cpf}
                      onChange={handleChange}
                      className={`h-11 ${cpfError ? 'border-red-500' : ''}`}
                      maxLength={14}
                      disabled={isEditing} // CPF não pode ser alterado
                      required
                    />
                    {cpfError && (
                      <p className="text-xs text-red-600">{cpfError}</p>
                    )}
                  </div>
                </div>

                {/* Email */}
                <div className="space-y-2">
                  <Label htmlFor="email" className="text-sm font-medium">
                    Email
                  </Label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                    <Input
                      id="email"
                      name="email"
                      type="email"
                      placeholder="Digite o email (opcional)"
                      value={formData.email}
                      onChange={handleChange}
                      className={`pl-10 h-11 ${emailError ? 'border-red-500' : ''}`}
                    />
                  </div>
                  {emailError && (
                    <p className="text-xs text-red-600">{emailError}</p>
                  )}
                </div>

                {/* Data de Nascimento */}
                <div className="space-y-2">
                  <Label htmlFor="dataNascimento" className="text-sm font-medium">
                    Data de Nascimento *
                  </Label>
                  <div className="relative">
                    <Calendar className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                    <Input
                      id="dataNascimento"
                      name="dataNascimento"
                      type="date"
                      value={formData.dataNascimento}
                      onChange={handleChange}
                      className="pl-10 h-11"
                      max={new Date().toISOString().split('T')[0]}
                      required
                    />
                  </div>
                  {formData.dataNascimento && (
                    <p className="text-xs text-gray-600">
                      Idade: {pessoasService.calcularIdade(formData.dataNascimento)} anos
                    </p>
                  )}
                </div>

                {/* Naturalidade e Nacionalidade */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  <div className="space-y-2">
                    <Label htmlFor="naturalidade" className="text-sm font-medium">
                      Naturalidade
                    </Label>
                    <div className="relative">
                      <MapPin className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                      <Input
                        id="naturalidade"
                        name="naturalidade"
                        type="text"
                        placeholder="Cidade de nascimento"
                        value={formData.naturalidade}
                        onChange={handleChange}
                        className="pl-10 h-11"
                      />
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label htmlFor="nacionalidade" className="text-sm font-medium">
                      Nacionalidade
                    </Label>
                    <Input
                      id="nacionalidade"
                      name="nacionalidade"
                      type="text"
                      placeholder="Nacionalidade"
                      value={formData.nacionalidade}
                      onChange={handleChange}
                      className="h-11"
                    />
                  </div>
                </div>

                {/* Informações adicionais */}
                <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                  <div className="flex items-start space-x-3">
                    <div className="flex-shrink-0">
                      <div className="w-6 h-6 bg-blue-100 rounded-full flex items-center justify-center">
                        <AlertCircle className="w-3 h-3 text-blue-600" />
                      </div>
                    </div>
                    <div className="text-sm text-blue-700">
                      <p className="font-medium mb-1">Informações importantes:</p>
                      <ul className="space-y-1 text-xs">
                        <li>• Campos marcados com * são obrigatórios</li>
                        <li>• O CPF deve ser válido e único no sistema</li>
                        <li>• {isEditing ? 'O CPF não pode ser alterado após o cadastro' : 'Verifique se o CPF está correto antes de salvar'}</li>
                        <li>• O email é opcional mas deve ter formato válido se preenchido</li>
                      </ul>
                    </div>
                  </div>
                </div>
              </CardContent>

              <div className="px-6 py-4 bg-gray-50 border-t flex justify-end space-x-3">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => navigate('/pessoas')}
                  disabled={loading}
                >
                  Cancelar
                </Button>
                
                <Button
                  type="submit"
                  disabled={loading || !!cpfError || !!emailError}
                  className="bg-gradient-to-r from-blue-600 to-blue-700 hover:from-blue-700 hover:to-blue-800"
                >
                  {loading ? (
                    <div className="flex items-center space-x-2">
                      <RefreshCw className="w-4 h-4 animate-spin" />
                      <span>{isEditing ? 'Atualizando...' : 'Salvando...'}</span>
                    </div>
                  ) : (
                    <div className="flex items-center space-x-2">
                      <Save className="w-4 h-4" />
                      <span>{isEditing ? 'Atualizar' : 'Salvar'}</span>
                    </div>
                  )}
                </Button>
              </div>
            </form>
          </Card>
        </motion.div>
      </main>
    </div>
  );
};

export default PessoaForm;
