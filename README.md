# ApiEmprendimiento
Sistema de Gestión de Emprendimientos y Snacks Educativos  
![License](https://img.shields.io/badge/License-MIT-green.svg)  
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)  
![Flutter](https://img.shields.io/badge/Flutter-3.22-02569B)  
![Database](https://img.shields.io/badge/Database-SQL_Server-CC2927)  

Sistema integral para gestión de **emprendimientos, inventarios, productos y tutorías de aprendizaje**, con arquitectura escalable, autenticación JWT y roles diferenciados.  

---

## 🔹 Diagrama de Arquitectura
![Arquitectura](https://github.com/user-attachments/assets/486ec373-1df2-4037-8603-69c203ed2743)

---

## 🚀 Componentes Clave

### 1. Servicios Principales  
**API .NET 8.0 (Puerto 5001):**
- Gestión centralizada de usuarios, productos, inventarios y ventas.  
- Autenticación **JWT** con roles y claims.  
- Conexión a **SQL Server** con Entity Framework Core.  

**Nicero Chat Service (extensión planificada):**
- Comunicación en tiempo real con **SignalR / WebSockets**.  
- Soporte técnico integrado en la app.  
- Notificaciones push.  

---

### 2. Base de Datos  
**SQL Server 2022 (MET):**
- Almacenamiento transaccional relacional.  
- Optimización para consultas de productos e inventarios.  
- **Backup automático diario** configurado en la infraestructura.  

---

### 3. Módulo de Aprendizaje  
**Gestión de Tutorías:**
- Creación de sesiones de tutoría.  
- Asignación de tutores a estudiantes.  
- Seguimiento del progreso.  
- Calendario de sesiones con integración móvil.  

**Roles Especializados:**
- **Tutor:** guía educativa, evaluaciones de alumnos.  
- **CAM:** administración académica, reportes y estadísticas.  

---

## 🛠 Tecnologías Utilizadas  

| Capa             | Tecnologías |
|------------------|-------------|
| **Frontend**     | Flutter 3.22, Riverpod, Hive, Socket.io |
| **Backend**      | .NET 8.0, Entity Framework Core, SignalR |
| **Base de Datos**| SQL Server 2022, Redis (cache) |
| **Infraestructura** | Docker, Azure App Service |
| **Comunicación** | REST API, WebSockets |

---

## ⚙️ Configuración del Entorno  

### 🔹 Requisitos Mínimos  
- **.NET 8.0 SDK**  
- **Flutter 3.22+**  
- **SQL Server 2022**  
- **Docker** (opcional, para despliegues rápidos en contenedores)  

### 🔹 Pasos de Instalación  
1. Clonar el repositorio:  
   ```bash
   git clone https://github.com/tu-usuario/ApiEmprendimiento.git
