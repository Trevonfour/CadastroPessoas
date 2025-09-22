API de Cadastro de Pessoas - Stefanini
API RESTful robusta e escalável, desenvolvida em .NET 8, para o desafio de Cadastro de Pessoas da Stefanini. A solução é focada em boas práticas de desenvolvimento, alta cobertura de testes e manutenibilidade.

🚀 Principais Funcionalidades
🔐 Autenticação & Segurança
Autenticação via JWT: Geração de tokens stateless para acesso seguro.

Registro de Usuários: Sistema completo com hash de senhas (BCrypt).

Recuperação de Senha: Fluxo seguro para redefinição de senhas.

Middleware de Autorização: Proteção de endpoints baseada em tokens.

👥 Gerenciamento de Pessoas (CRUD)
Cadastro, Consulta, Atualização e Remoção: Operações CRUD completas.

Consulta Paginada com Filtros: Busca otimizada por nome, CPF ou email.

Soft Delete: Pessoas não são deletadas fisicamente, apenas inativadas.

Validação de CPF: Algoritmo oficial para garantir a validade dos CPFs.

⚙️ Arquitetura & Recursos
Versionamento de API: Suporte para v1 e v2 via URL, Header ou Query String.

Documentação Interativa: Geração automática de documentação com Swagger (OpenAPI).

Tratamento Global de Exceções: Middleware centralizado para respostas de erro padronizadas.

Logging Estruturado: Logs detalhados com Serilog para monitoramento e depuração.

CORS: Configuração flexível para permitir a comunicação com frontends.

Health Checks: Endpoints para monitoramento da saúde da aplicação.

Alta Cobertura de Testes: Testes de unidade e integração garantindo a qualidade do código.

🛠️ Tecnologias Utilizadas
Ferramenta / Biblioteca

Propósito

.NET 8

Framework principal da aplicação

Entity Framework Core

ORM para manipulação de dados

xUnit & Moq

Frameworks para testes de unidade

JWT Bearer

Autenticação baseada em token

Swagger (Swashbuckle)

Documentação de API

Serilog

Sistema de logging estruturado

BCrypt.Net-Next

Geração de hash de senhas

InMemory Database

Banco de dados para desenvolvimento e testes

🔧 Começando
Pré-requisitos
.NET 8 SDK

Um editor de código como Visual Studio 2022 ou VS Code

(Opcional) Postman para testar os endpoints.

Instalação e Execução
Clone o repositório:

git clone <URL_DO_SEU_REPOSITORIO>
cd <PASTA_DO_PROJETO>/backend

Restaure as dependências do .NET:

dotnet restore

Execute a aplicação:

dotnet run

A API estará rodando. Procure no terminal a URL de escuta, geralmente https://localhost:64138.

Acesse os recursos:

Documentação Swagger: https://localhost:64138

Health Check: https://localhost:64138/health

📚 Documentação da API
A API está documentada com Swagger e pode ser acessada interativamente no navegador.

🔑 Autenticação
Para acessar endpoints protegidos, primeiro obtenha um token através do endpoint de login e inclua-o no header de suas requisições:

Authorization: Bearer <SEU_TOKEN_JWT>

Usuário padrão (criado via Seed):

Usuário: admin

Senha: Stefanini@2024

Principais Endpoints
POST /api/v1/auth/login
Autentica um usuário e retorna um token JWT.

<details>
<summary>Exemplo de Request/Response</summary>

Request Body:

{
  "usuario": "admin",
  "senha": "Stefanini@2024"
}

Response (200 OK):

{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiracao": "2025-09-22T12:00:00Z",
  "usuario": {
    "id": 1,
    "nomeUsuario": "admin",
    "email": "admin@stefanini.com",
    "nomeCompleto": "Administrador do Sistema",
    "perfil": "Admin"
  }
}

</details>

GET /api/pessoas
Lista todas as pessoas de forma paginada. (Requer autenticação)

Query Parameters:

Parâmetro

Tipo

Descrição

Padrão

pagina

int

Número da página desejada.

1

tamanhoPagina

int

Quantidade de itens por página.

10

filtro

string

Texto para filtrar por nome, CPF ou email.

null

<details>
<summary>Exemplo de Response</summary>

Response (200 OK):

{
  "pessoas": [
    {
      "id": 1,
      "nome": "João Silva Santos",
      "sexo": "M",
      "email": "joao.silva@email.com",
      "dataNascimento": "1990-05-15T00:00:00Z",
      "idade": 35,
      "naturalidade": "São Paulo",
      "nacionalidade": "Brasileira",
      "cpf": "123.456.789-01",
      "dataCadastro": "2025-09-22T10:00:00Z",
      "dataAtualizacao": "2025-09-22T10:00:00Z"
    }
  ],
  "total": 1,
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalPaginas": 1
}

</details>

POST /api/pessoas
Cria uma nova pessoa. (Requer autenticação)

<details>
<summary>Exemplo de Request</summary>

Request Body:

{
  "nome": "Maria Oliveira Costa",
  "sexo": "F",
  "email": "maria.oliveira@email.com",
  "dataNascimento": "1985-08-22",
  "naturalidade": "Rio de Janeiro",
  "nacionalidade": "Brasileira",
  "cpf": "987.654.321-09"
}

</details>

Outros Endpoints de Pessoas
GET /api/pessoas/v1/{id}: Busca uma pessoa pelo ID (versão 1).

GET /api/pessoas/cpf/{cpf}: Busca uma pessoa pelo CPF.

PUT /api/pessoas/{id}: Atualiza os dados de uma pessoa.

DELETE /api/pessoas/{id}: Inativa uma pessoa (soft delete).

🌐 Versionamento da API
A API suporta versionamento para futuras evoluções. A versão pode ser especificada de três formas:

URL: /api/v2/pessoas/{id}

Header: x-api-version: 2.0

Query String: /api/pessoas/{id}?api-version=2.0

O endpoint GET /api/v2/pessoas/{id} é um exemplo que retorna dados adicionais não presentes na v1.

🧪 Testes
O projeto possui uma suíte completa de testes de unidade e integração para garantir a qualidade e a estabilidade do código.

Para rodar todos os testes, execute:

dotnet test

Para gerar o relatório de cobertura de testes (requer reportgenerator):

# 1. Coletar os dados
dotnet test --collect:"XPlat Code Coverage"

# 2. Gerar o relatório
reportgenerator -reports:"StefaniniCadastroPessoas.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html

O relatório será gerado na pasta coveragereport.
