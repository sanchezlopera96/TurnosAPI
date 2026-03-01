# TurnosAPI — Sistema de Agendamiento de Turnos Bancarios

## Arquitectura

Clean Architecture con 4 capas:
```
TurnosAPI/
├── TurnosAPI.Domain/          ← Entidades, Enums, Interfaces, Excepciones
├── TurnosAPI.Application/     ← Servicios, DTOs, Interfaces
├── TurnosAPI.Infrastructure/  ← EF Core, Repositorios, JWT, BCrypt
├── TurnosAPI.API/             ← Controladores, Middleware, Servicios en segundo plano
└── TurnosAPI.Tests/           ← Pruebas unitarias con xUnit + Moq
```

### Patrones y Principios de Diseño
- **SOLID** — Aplicado en todas las capas
- **GRASP** — Experto en Información, Controlador, Creador, Bajo Acoplamiento, Alta Cohesión
- **Patrón Repositorio** — Abstrae el acceso a datos de la lógica de negocio
- **Unidad de Trabajo (Unit of Work)** — Coordina transacciones atómicas entre repositorios
- **Clean Architecture** — Las dependencias apuntan hacia adentro, el dominio no tiene dependencias externas

## Requisitos Previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server Express (o cualquier instancia de SQL Server)
- [dotnet-ef tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

## Configuración y Ejecución

### 1. Clonar el repositorio
```bash
git clone https://github.com/tu-usuario/TurnosAPI.git
cd TurnosAPI
```

### 2. Configurar appsettings

Copia el archivo de ejemplo y actualiza con tu configuración:
```bash
cp TurnosAPI.API/appsettings.example.json TurnosAPI.API/appsettings.json
```

Edita `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TurnosDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "TurnosAPI_SuperSecretKey_2024_MustBe32CharsMin!",
    "Issuer": "TurnosAPI",
    "Audience": "TurnosAPI",
    "ExpirationHours": "8"
  }
}
```

### 3. Instalar dotnet-ef (si no está instalado)
```bash
dotnet tool install --global dotnet-ef
```

### 4. Ejecutar migraciones
```bash
dotnet ef migrations add InitialCreate --project TurnosAPI.Infrastructure --startup-project TurnosAPI.API
dotnet ef database update --project TurnosAPI.Infrastructure --startup-project TurnosAPI.API
```

### 5. Ejecutar la API
```bash
dotnet run --project TurnosAPI.API
```

Swagger UI: `https://localhost:7000/swagger`

## Ejecutar Pruebas
```bash
dotnet test
```

## Endpoints de la API

### Autenticación
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | /api/auth/login-admin | Público | Login administrador (usuario + contraseña) |
| POST | /api/auth/login-client | Público | Login cliente (cédula) |

### Sucursales
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| GET | /api/branches | Cualquiera | Listar sucursales activas |
| GET | /api/branches/{id} | Cualquiera | Obtener sucursal por id |
| POST | /api/branches | Admin | Crear sucursal |
| PUT | /api/branches/{id} | Admin | Actualizar sucursal |

### Turnos
| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | /api/appointments | Cliente | Crear turno |
| GET | /api/appointments | Admin | Listar todos los turnos |
| GET | /api/appointments/{id} | Cualquiera | Obtener turno por id |
| GET | /api/appointments/my-appointments | Cliente | Turnos del cliente autenticado |
| PUT | /api/appointments/{id}/activate | Cliente | Activar turno en sucursal |
| PUT | /api/appointments/{id}/status | Admin | Actualizar estado (Attended/Cancelled) |

## Reglas de Negocio

- Los clientes se autentican únicamente con su **cédula**
- Los administradores se autentican con **usuario + contraseña**
- Los turnos expiran a los **15 minutos** si no son activados en la sucursal
- Máximo **5 turnos por cliente por día**
- Un servicio en segundo plano verifica y expira turnos cada **1 minuto**
- Ciclo de vida del turno: `Pendiente → Activo → Atendido` o `Pendiente → Expirado/Cancelado`

## Seguridad

- Autenticación con JWT Bearer
- Autorización basada en roles (`Admin`, `Cliente`)
- Contraseñas hasheadas con BCrypt (factor de trabajo 12)
- CORS configurado para el frontend Angular (`http://localhost:4200`)