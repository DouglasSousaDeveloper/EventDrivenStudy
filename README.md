Este repositório tem como objetivo estudar e demonstrar uma arquitetura orientada a eventos (Event-Driven Architecture) utilizando .NET (C#), RabbitMQ, Docker, AWS e Kubernetes (EKS), com foco em boas práticas arquiteturais, separação de responsabilidades e código de nível sênior.
O projeto foi pensado para iniciar 100% em ambiente local, permitindo fácil execução e entendimento, e evoluir gradualmente para um cenário cloud-native, próximo do que é utilizado em ambientes reais de produção.
- Objetivos do Projeto
Compreender na prática os conceitos de Event-Driven Architecture
Aplicar mensageria assíncrona com RabbitMQ
Utilizar .NET moderno com boas práticas de arquitetura
Separar corretamente domínio, infraestrutura e integração
Trabalhar com Docker e Docker Compose localmente
Preparar a arquitetura para execução em Kubernetes (EKS)
Demonstrar padrões reais de mercado, evitando exemplos simplistas
-  Visão Geral da Arquitetura
A arquitetura é baseada em eventos de domínio publicados por serviços produtores e consumidos por serviços independentes, promovendo:
Baixo acoplamento
Escalabilidade
Resiliência
Evolução independente dos serviços
Componentes principais:
API Produtora de Eventos
Publica eventos de domínio (ex: OrderCreatedEvent)
Workers Consumidores
Processam eventos de forma assíncrona
Cada worker possui sua própria fila
RabbitMQ
Atua como message broker
Utiliza Exchanges do tipo topic
Shared (Biblioteca Compartilhada)
Eventos
Abstrações
Infraestrutura comum reutilizável
Docker & Docker Compose
Execução local
Kubernetes (EKS)
Preparado para evolução cloud-native
