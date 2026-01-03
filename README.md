Cadastro de Pessoas (Full Stack)
Projeto Full Stack completo desenvolvido como solução para o desafio de Cadastro de Pessoas. A aplicação é composta por uma API RESTful em .NET 8 e uma interface moderna em React 19, seguindo as melhores práticas de desenvolvimento, testes e arquitetura de software.

![Imagem da tela de dashboard da aplicação]

🏛️ Arquitetura do Projeto
Esta solução é um monorepo que contém dois projetos independentes, mas conectados:

/backend: Uma API .NET 8 robusta e segura, responsável por toda a lógica de negócio, autenticação, acesso a dados e validações.

/frontend: Uma Single-Page Application (SPA) em React 19, que consome a API e oferece uma experiência de usuário rica, interativa e responsiva.

✨ Funcionalidades em Destaque
Backend (API .NET 8)
✅ Autenticação segura com JWT e hash de senhas BCrypt.

✅ Operações CRUD completas para o gerenciamento de pessoas.

✅ Versionamento de API (v1, v2) para futuras evoluções.

✅ Documentação interativa com Swagger (OpenAPI).

✅ Logging estruturado com Serilog e tratamento global de exceções.

✅ Alta cobertura de testes (78%), incluindo testes de unidade e integração.

Frontend (React 19)
✅ Interface moderna e totalmente responsiva com Tailwind CSS.

✅ Componentes de UI de alta qualidade baseados no shadcn/ui.

✅ Animações fluidas com Framer Motion para uma UX aprimorada.

✅ Gerenciamento de estado e autenticação centralizado com React Context.

✅ Roteamento protegido para garantir que apenas usuários logados acessem áreas restritas.

✅ Funcionalidades extras como busca dinâmica, paginação e exportação para CSV.

🛠️ Stack de Tecnologias
Área

Tecnologias Utilizadas

Backend

.NET 8, Entity Framework Core, xUnit, Moq, JWT, Serilog, Swagger, Asp.Versioning

Frontend

React 19, Vite, React Router, Tailwind CSS, shadcn/ui, Framer Motion, Zod

Banco de Dados

InMemory Database (para desenvolvimento e testes)

🚀 Como Começar (Guia Completo)
Siga estes passos para rodar a aplicação completa (Backend + Frontend) em sua máquina local.

Pré-requisitos
.NET 8 SDK

Node.js v18+

pnpm (Recomendado: npm install -g pnpm)

1. Backend (API)
Primeiro, inicie o servidor da API.

# 1. Navegue para a pasta do backend
cd backend

# 2. Restaure as dependências do .NET
dotnet restore

# 3. Execute a API
dotnet run

A API estará rodando. Anote a URL que aparece no terminal (ex: https://localhost:7123).

2. Frontend (Aplicação React)
Em um novo terminal, inicie a aplicação frontend.

# 1. Navegue para a pasta do frontend
cd frontend

# 2. Instale as dependências do Node.js
pnpm install

# 3. Crie o arquivo de ambiente
# Crie um arquivo chamado .env.local na raiz da pasta /frontend
# e adicione a URL da sua API que você anotou no passo anterior:
VITE_API_BASE_URL=https://localhost:7123

# 4. Execute a aplicação
pnpm dev

A aplicação frontend estará acessível em http://localhost:5173.

Credenciais Padrão
Para fazer login, utilize o usuário administrador que é criado por padrão:

Usuário: admin

Senha: Stefanini@2024

🧪 Testes e Qualidade
A qualidade do código é garantida por uma suíte de testes abrangente.

Executar todos os testes (backend e frontend):

# Na pasta /backend
dotnet test

# Na pasta /frontend
pnpm test

Gerar Relatório de Cobertura (Backend):

# Na pasta /backend
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:"StefaniniCadastroPessoas.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport"



