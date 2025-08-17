# ApiEmprendimiento
Sistema de Gestión de Emprendimientos  
![License](https://img.shields.io/badge/License-MIT-green.svg)  
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)  
![Flutter](https://img.shields.io/badge/Flutter-3.22-02569B)  
![Database](https://img.shields.io/badge/Database-SQL_Server-CC2927)  
![Chat](https://img.shields.io/badge/Chat-Node.js/MongoDB-68A063)  

Sistema integral para gestión de **emprendimientos, inventarios, productos y ventas**, con arquitectura escalable, autenticación **JWT** y chat en tiempo real.  

---

## 🔹 Diagrama de Arquitectura
![Arquitectura](https://img.plantuml.biz/plantuml/png/PLF1RjGm4BtxAqQzK4Gtgi85zO2QxUvAYzXssOJkWN2OEaFhajZ1TYe4uglu17wCiRsfclOKU-RDy_DxZZXt7gqVkbPuvLk2mluOb2Vf1ulGK0kbSfuwX1aKlLfslINTg4wHzaO8bDvO-Em6j8gtbkTGtpBDqSeQxHaAgtRmdcojZmyhU9lb-UiZG3VsuBU0aGHci_TZNVoPKCvHVW-CMwkqL3ssjSukbnMyoqxX3InNS64iHTuH6NYV41G3A_9AebJ__chuiAyYch_FSDirx9PbLUk3A7S2qvkxarglzGCl3-sjDSqWGmEkGzL5PYOATsZGumqwPYu_VOIIx0FVEJ_DSpPb7keDmG9v62gBsg6KocXiaqSyCd4XZ2i6XA6BE_IrlZGDPVUEFTZJJMduwZvEXkuB2_XgbQAkDx2ZuE1n6iZML-cuVJbLl37wrYpc32pQ3YLfJ0wCnxiq5c52lE8CDFra-UIW7tCY9yQaGxf1mraqE1eKjg4Sl3c3gzrcZFAnmm5nvnfRrHoYgmpdAoQi-v8nZmO8tePgrCMDBLaN7Z8B2iyQDiG-txCmbhpMtooIytP8hzxqoV-dhZ_3i1sBS8vbqujVW-N_fkyXHKF9aJWXnfSu-tAdceRqLzQKEZ6TbsLnpl3mE_u7)

---

## 🚀 Componentes Clave

### 1. Servicios Principales  
**API .NET 8.0 (Puerto 5001):**
- Gestión centralizada de **usuarios, emprendimientos, inventarios, productos y ventas**.  
- Autenticación **JWT** con claims de usuario y emprendimiento.  
- Conexión a **SQL Server** con **Entity Framework Core**.  

**Chat en Tiempo Real (Node.js + MongoDB):**
- Comunicación en tiempo real vía **WebSockets**.  
- Persistencia de mensajes con **MongoDB**.  
- Notificaciones push y soporte colaborativo.  

---

### 2. Base de Datos  
**SQL Server 2022 (para gestión transaccional):**
- Usuarios, emprendimientos, inventarios, productos y ventas.  
- Modelo relacional optimizado para consultas de inventarios y reportes.  
- **Backup automático diario**.  

**MongoDB (para chat en tiempo real):**
- Almacenamiento de mensajes.  
- Optimización para consultas rápidas en salas de chat.  

---

## 🛠 Tecnologías Utilizadas  

| Capa             | Tecnologías |
|------------------|-------------|
| **Frontend**     | Flutter 3.22, Riverpod, Hive, Socket.io |
| **Backend Principal** | .NET 8.0, Entity Framework Core |
| **Chat Service** | Node.js, Express, Socket.IO, MongoDB |
| **Base de Datos**| SQL Server 2022 (transaccional), MongoDB (chat), Redis (cache opcional) |
| **Infraestructura** | Docker, Azure App Service |
| **Comunicación** | REST API, WebSockets |

---

## ⚙️ Configuración del Entorno  

### 🔹 Requisitos Mínimos  
- **.NET 8.0 SDK**  
- **Flutter 3.22+**  
- **SQL Server 2022**  
- **MongoDB**  
- **Docker** (opcional, para despliegues en contenedores)  

### 🔹 Pasos de Instalación  

1. Clonar el repositorio:  
   ```bash
   git clone https://github.com/tu-usuario/ApiEmprendimiento.git

