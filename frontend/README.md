'''
# Stefanini Cadastro de Pessoas - Frontend React

Frontend desenvolvido em React 19 para o sistema de cadastro de pessoas da Stefanini.

## ✨ Funcionalidades

### Autenticação
- ✅ Tela de Login com validação
- ✅ Tela de Registro com validação e indicador de força de senha
- ✅ Tela de Recuperação de Senha
- ✅ Tela de Redefinição de Senha
- ✅ Rotas protegidas e gerenciamento de sessão

### Gerenciamento de Pessoas
- ✅ Dashboard com estatísticas e ações rápidas
- ✅ Listagem de pessoas com paginação e busca
- ✅ Formulário de Cadastro e Edição com validações em tempo real
- ✅ Visualização de detalhes da pessoa
- ✅ Remoção de pessoa com confirmação
- ✅ Exportação de dados para CSV

### UX/UI
- ✅ Design moderno e responsivo
- ✅ Animações suaves com Framer Motion
- ✅ Componentes reutilizáveis com shadcn/ui
- ✅ Ícones da biblioteca Lucide React
- ✅ Notificações e alertas contextuais

## 🛠️ Tecnologias Utilizadas

- **React 19** - Biblioteca principal
- **Vite** - Build tool
- **React Router DOM** - Roteamento
- **Tailwind CSS** - Estilização
- **shadcn/ui** - Componentes de UI
- **Framer Motion** - Animações
- **Lucide React** - Ícones
- **Jest & React Testing Library** - Testes

## 📋 Pré-requisitos

- Node.js v18+
- pnpm (ou npm/yarn)

## 🔧 Instalação e Execução

1. **Navegue até a pasta do frontend**
```bash
cd stefanini-cadastro-pessoas/frontend/stefanini-cadastro-frontend
```

2. **Instale as dependências**
```bash
pnpm install
```

3. **Execute a aplicação em modo de desenvolvimento**
```bash
pnpm run dev
```

4. **Acesse a aplicação**
- Abra seu navegador em: http://localhost:5173

## scripts

- `pnpm dev`: Inicia o servidor de desenvolvimento.
- `pnpm build`: Gera a build de produção na pasta `dist`.
- `pnpm preview`: Inicia um servidor local para visualizar a build de produção.
- `pnpm test`: Executa os testes unitários e de integração.
- `pnpm test:coverage`: Executa os testes e gera um relatório de cobertura.

## 🔐 Autenticação

Para testar a aplicação, utilize as seguintes credenciais:

- **Usuário:** `admin`
- **Senha:** `admin123`

## 🧪 Testes

O projeto está configurado com Jest e React Testing Library para garantir a qualidade do código.

- **Executar todos os testes:**
```bash
pnpm test
```

- **Executar testes em modo watch:**
```bash
pnpm test:watch
```

- **Gerar relatório de cobertura:**
```bash
pnpm test:coverage
```

## 📂 Estrutura de Pastas

```
src/
├── components/       # Componentes reutilizáveis (UI e lógica)
├── contexts/         # Contextos React (Ex: AuthContext)
├── pages/            # Componentes de página (rotas)
├── services/         # Lógica de comunicação com a API
├── App.jsx           # Componente principal com as rotas
├── main.jsx          # Ponto de entrada da aplicação
└── ...
```

## 📞 Suporte

Para dúvidas ou suporte, entre em contato com a equipe de desenvolvimento.
'''
