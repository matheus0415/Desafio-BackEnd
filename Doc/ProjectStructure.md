# Estrutura de Pastas e Camadas

Este documento descreve a arquitetura e organização de diretórios do projeto MotoRent.

## Camadas e Pastas

### 1. Api

- **Controllers**: Pontos de entrada para as requisições HTTP.
- **Configurations**: Configurações de middlewares, autenticação, Swagger, etc.
- **DTOs**: Objetos de transferência de dados (Request/Response).
- **Filters**: Filtros globais para tratamento de exceções, validações, etc.
- **Middlewares**: Middlewares customizados (log, autenticação, etc.).
- **Messaging**: Publicação e consumo de mensagens (RabbitMQ, Kafka, etc.).

### 2. Application

- **Services**: Implementação da lógica de negócio e orquestração de processos.
- **Interfaces**: Contratos a serem implementados pela infraestrutura.
- **Commands**: Padrão CQRS – operações de escrita.
- **Queries**: Padrão CQRS – operações de leitura.
- **Mappings**: Mapeamento entre entidades e DTOs (AutoMapper, etc.).

### 3. Domain

- **Entities**: Entidades centrais com identidade própria.
- **ValueObjects**: Objetos de valor (imutáveis, sem identidade).
- **Enums**: Enumerações específicas do domínio.
- **Events**: Eventos de domínio para comunicação interna.
- **Exceptions**: Exceções personalizadas do domínio.
- **Repositories**: Interfaces de repositórios para persistência.

### 4. Infrastructure

- **Persistence**: Camada de dados (Contexts, Migrations).
- **Repositories**: Implementações concretas dos repositórios.
- **Messaging**: Implementações de mensageria.
- **Configurations**: Configurações específicas da infraestrutura.
- **ExternalServices**: Integrações com APIs de terceiros e serviços externos.

### 5. Tests

- **Unit**: Testes unitários para classes isoladas.
- **Integration**: Testes de integração com serviços reais.
- **Functional**: Testes ponta a ponta (E2E).

---
