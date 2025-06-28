# ApiEmprendimiento
Sistema de Gestión de Snacks y Aprendizaje
https://img.shields.io/badge/License-MIT-green.svg
https://img.shields.io/badge/.NET-8.0-512BD4
https://img.shields.io/badge/Flutter-3.22-02569B
https://img.shields.io/badge/Database-SQL_Server-CC2927

Sistema integral para gestión de snacks y tutorías de aprendizaje con arquitectura escalable y roles diferenciados.

#Diagrama de Arquitectura
![Arquitectura](https://github.com/user-attachments/assets/486ec373-1df2-4037-8603-69c203ed2743)

Componentes Clave
1. Servicios Principales
API .NET 8.0 (Puerto 5001):

Gestión centralizada de operaciones

Autenticación JWT con roles

Comunicación con SQL Server

Nicero Chat Service:

Comunicación en tiempo real

Soporte técnico integrado

Notificaciones push

2. Base de Datos
SQL Server (MET):

Almacenamiento transaccional

Modelo relacional optimizado

Backup automático diario

3. Módulo de Aprendizaje
Gestión de Tutorías:

Asignación de tutores

Seguimiento de progresos

Calendario de sesiones

Roles Especializados:

Tutor: Guía educativa, evaluaciones

CAM: Administración académica, reportes

Tecnologías Utilizadas
Capa	Tecnologías
Frontend	Flutter 3.22, Riverpod, Hive, Socket.io
Backend	.NET 8.0, Entity Framework Core, SignalR
Base de Datos	SQL Server 2022, Redis (cache)
Infraestructura	Docker, Azure App Service
Comunicación	REST API, WebSockets
Configuración del Entorno
Requisitos Mínimos
.NET 8.0 SDK

Flutter 3.22+

SQL Server 2022

Docker (opcional)
