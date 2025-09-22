#!/bin/bash

# Navega para o diretório do backend
cd "$(dirname "$0")"

# Restaura as dependências do NuGet
dotnet restore StefaniniCadastroPessoasSolution.sln

# Executa os testes e a coleta de cobertura de código
dotnet test StefaniniCadastroPessoasSolution.sln --collect:"XPlat Code Coverage" --results-directory TestResults/ --settings coverlet.runsettings

# Abre o relatório de cobertura (descomente a linha apropriada para o seu sistema operacional)
# Para Windows:
# start TestResults/*/coverage.cobertura.xml

# Para macOS:
# open TestResults/*/coverage.cobertura.xml

# Para Linux:
# xdg-open TestResults/*/coverage.cobertura.xml


echo "Execução dos testes concluída. O relatório de cobertura foi salvo em TestResults/"

