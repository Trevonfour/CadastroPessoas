Stefanini Cadastro de Pessoas - Frontend
Interface moderna e responsiva desenvolvida em React 19 e Vite para o sistema de Cadastro de Pessoas da Stefanini. O projeto foi construído com foco em uma experiência de usuário fluida, componentes reutilizáveis e um código limpo e manutenível.

[Imagem da tela de dashboard da aplicação]

✨ Funcionalidades Principais
🔐 Autenticação e Segurança
Telas Completas: Login, Registro, Recuperação e Redefinição de Senha com validação de formulários em tempo real.

Indicador de Força de Senha: Feedback visual para o usuário durante o cadastro.

Gerenciamento de Sessão: Rotas protegidas que redirecionam para o login caso o usuário não esteja autenticado.

👥 Gerenciamento de Pessoas
Dashboard Central: Visão geral com estatísticas e atalhos para as principais ações.

Listagem Dinâmica: Tabela de pessoas com busca inteligente e paginação assíncrona.

Formulários Modernos: Modais para Cadastro e Edição com validações e feedback instantâneo.

Exportação de Dados: Funcionalidade para exportar a lista de pessoas para um arquivo .csv.

🎨 Experiência de Usuário (UX/UI)
Design Responsivo: Interface adaptada para desktops, tablets e celulares.

Animações Fluidas: Transições suaves e micro-interações com Framer Motion.

Componentes Reutilizáveis: Construído com base no shadcn/ui para consistência visual.

Notificações Contextuais: Alertas de sucesso e erro para guiar o usuário.

🛠️ Stack de Tecnologias
Tecnologia

Propósito

React 19

Biblioteca principal para a construção da UI.

Vite

Ferramenta de build e servidor de desenvolvimento de alta performance.

React Router DOM

Gerenciamento de rotas e navegação na aplicação.

Tailwind CSS

Framework CSS utility-first para estilização rápida e consistente.

shadcn/ui

Coleção de componentes de UI acessíveis e reutilizáveis.

Framer Motion

Biblioteca para criação de animações complexas e fluidas.

Lucide React

Pacote de ícones SVG leves e customizáveis.

Zod

Validação de esquemas de dados para formulários.

React Hook Form

Gerenciamento de formulários.

Jest & RTL

Framework e biblioteca para testes de unidade e componentes.

🚀 Começando
Pré-requisitos
Node.js: Versão 18 ou superior.

pnpm: (Recomendado) npm install -g pnpm.

API Backend: O backend do projeto deve estar em execução.

Instalação e Execução
Clone este repositório e navegue até a pasta do frontend:

git clone <https://github.com/Trevonfour/CadastroPessoas.git>
cd <PASTA_DO_PROJETO>/frontend

Instale as dependências:

pnpm install

Configure o Ambiente:

Crie um arquivo .env.local na raiz da pasta frontend.

Adicione a URL da sua API backend:

VITE_API_BASE_URL=https://localhost:64138/api

Inicie a aplicação em modo de desenvolvimento:

pnpm dev

Abra seu navegador no endereço: http://localhost:5173

📜 Scripts Disponíveis
pnpm dev: Inicia o servidor de desenvolvimento com Hot Reload.

pnpm build: Compila a aplicação para produção na pasta dist/.

pnpm preview: Inicia um servidor local para visualizar a build de produção.

🧪 Testes
O projeto utiliza Jest e React Testing Library para garantir a qualidade dos componentes e da lógica.

Executar todos os testes uma vez:

pnpm test

Executar testes em modo interativo (watch):

pnpm test:watch

📂 Estrutura do Projeto
A estrutura de pastas segue as melhores práticas para projetos React, visando escalabilidade e organização.

src/
├── assets/         # Imagens, fontes e outros arquivos estáticos
├── components/     # Componentes de UI reutilizáveis (ex: Button, Input)
├── contexts/       # Contextos React para gerenciamento de estado global (AuthContext)
├── hooks/          # Hooks customizados
├── lib/            # Funções utilitárias (ex: formatação de datas)
├── pages/          # Componentes que representam as páginas da aplicação
├── services/       # Módulos para fazer as chamadas à API
├── App.jsx         # Componente raiz com a configuração do roteador
└── main.jsx        # Ponto de entrada da aplicação React