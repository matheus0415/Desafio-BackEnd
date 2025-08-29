# Guia de Configuração e Execução - MotoRent

## Requisitos
- .NET 8 SDK
- Docker (para banco de dados PostgreSQL)
- (Opcional) Ferramenta gráfica para visualizar o banco, como DBeaver ou pgAdmin

## Configuração do Banco de Dados

O projeto utiliza PostgreSQL via Docker. Para subir o banco localmente:

```sh
docker-compose up -d
```

Isso criará um container com o banco configurado conforme o arquivo `docker-compose.yml`.

## Configuração do Projeto

1. Clone o repositório:
   ```sh
   git clone <url-do-repo>
   cd Desafio-BackEnd
   ```
2. Restaure os pacotes NuGet:
   ```sh
   dotnet restore
   ```
3. (Opcional) Ajuste as strings de conexão em `MotoRent.Api/appsettings.Development.json` se necessário.

## Execução do Projeto

Execute a API com:
```sh
dotnet run --project MotoRent.Api/MotoRent.Api.csproj
```
A API estará disponível em: http://localhost:5218

## Documentação da API

Acesse o Swagger UI em:
```
http://localhost:5218
```

## Migrações do Banco de Dados

Para aplicar ou criar novas migrações:
```sh
dotnet ef migrations add NomeDaMigracao -p MotoRent.Infrastructure -s MotoRent.Api
```
```sh
dotnet ef database update -p MotoRent.Infrastructure -s MotoRent.Api
```

## Visualizando o Banco de Dados

Você pode usar ferramentas como DBeaver, pgAdmin ou TablePlus para conectar no banco PostgreSQL. Os dados de acesso estão em `docker-compose.yml` e/ou `appsettings.Development.json`.

## Estrutura do Projeto
- MotoRent.Api: API ASP.NET Core
- MotoRent.Application: Serviços e regras de negócio
- MotoRent.Domain: Entidades e contratos de domínio
- MotoRent.Infrastructure: Persistência e integrações
- MotoRent.Tests: Testes automatizados

---

