# Informe de Arquitectura y Decisiones de Diseño
## Sistema de Agendamiento de Turnos Bancarios

---

## 1. Descripción General

El sistema está compuesto por dos proyectos:

- **TurnosAPI**: API REST desarrollada en ASP.NET Core 8 con Clean Architecture
- **TurnosWeb**: Frontend desarrollado en Angular 21 con componentes standalone

---

## 2. Arquitectura de la Solución

### 2.1 Backend — Clean Architecture

La solución backend está dividida en 4 capas con dependencias que apuntan hacia adentro:
```
TurnosAPI.Domain          ← Núcleo del negocio (sin dependencias externas)
TurnosAPI.Application     ← Casos de uso (depende de Domain)
TurnosAPI.Infrastructure  ← Implementaciones técnicas (depende de Application)
TurnosAPI.API             ← Capa de presentación (depende de Infrastructure)
TurnosAPI.Tests           ← Pruebas unitarias
```

**Capa Domain:**
- Entidades con lógica de negocio encapsulada (`Appointment`, `Branch`, `AdminUser`)
- Interfaces de repositorios (`IAppointmentRepository`, `IBranchRepository`, `IAdminUserRepository`)
- Interfaz de unidad de trabajo (`IUnitOfWork`)
- Excepciones de dominio (`DomainException`)
- Enumeraciones (`AppointmentStatus`, `UserRole`)

**Capa Application:**
- Servicios de aplicación (`AppointmentService`, `BranchService`, `AuthService`)
- DTOs de entrada y salida
- Interfaces de servicios e infraestructura (`IJwtTokenGenerator`, `IPasswordHasher`)
- Objeto genérico de respuesta (`ApiResponse<T>`)

**Capa Infrastructure:**
- Implementación de repositorios con Entity Framework Core
- Unidad de trabajo (`UnitOfWork`)
- Generador de tokens JWT (`JwtTokenGenerator`)
- Hash de contraseñas con BCrypt (`BcryptPasswordHasher`)
- Configuraciones de entidades EF Core

**Capa API:**
- Controladores REST (`AuthController`, `AppointmentsController`, `BranchesController`)
- Middleware de manejo de excepciones (`ExceptionHandlingMiddleware`)
- Servicio en segundo plano para expiración de turnos (`AppointmentExpirationService`)
- Configuración de la aplicación (`Program.cs`)

### 2.2 Frontend — Angular 21
```
src/app/
├── core/
│   ├── models/        ← Interfaces TypeScript (espejo de los DTOs del backend)
│   ├── services/      ← Servicios HTTP con caché
│   ├── interceptors/  ← Interceptor JWT automático
│   └── guards/        ← Guards de autenticación y autorización
├── features/
│   ├── auth/          ← Módulo de autenticación
│   ├── appointments/  ← Módulo de turnos
│   └── branches/      ← Módulo de sucursales
└── shared/
    └── components/    ← Componentes reutilizables
```

---

## 3. Patrones de Diseño Implementados

### 3.1 Principios SOLID

| Principio | Implementación |
|-----------|----------------|
| **S** — Single Responsibility | Cada clase tiene una única responsabilidad. `AppointmentService` orquesta, `JwtTokenGenerator` solo genera tokens, `BcryptPasswordHasher` solo hashea |
| **O** — Open/Closed | `IPasswordHasher` permite cambiar BCrypt por Argon2 sin modificar la capa de aplicación |
| **L** — Liskov Substitution | Todos los repositorios son intercambiables a través de sus interfaces |
| **I** — Interface Segregation | Interfaces separadas: `IAppointmentRepository`, `IBranchRepository`, `IAdminUserRepository` |
| **D** — Dependency Inversion | Los servicios dependen de `IUnitOfWork`, nunca de `AppDbContext` directamente |

### 3.2 Principios GRASP

| Principio | Implementación |
|-----------|----------------|
| **Information Expert** | La entidad `Appointment` es responsable de su propia lógica de negocio (`Activate()`, `Expire()`, `Cancel()`, `IsWithinTimeWindow()`) |
| **Creator** | `Appointment` genera su propio código (`T{HHmmss}{random}`) y tiempo de expiración en el constructor |
| **Controller** | Los controladores HTTP delegan a los servicios de aplicación |
| **Low Coupling** | Las capas se comunican únicamente a través de interfaces |
| **High Cohesion** | Cada módulo agrupa responsabilidades relacionadas |

### 3.3 Patrones Estructurales

- **Repository Pattern**: Abstrae el acceso a datos de la lógica de negocio
- **Unit of Work**: Coordina transacciones atómicas entre repositorios
- **Background Service**: Servicio en segundo plano para expirar turnos automáticamente cada minuto

---

## 4. Decisiones de Diseño

### 4.1 Autenticación

Se implementó JWT Bearer Authentication con dos flujos diferenciados:

- **Administradores**: Autenticación con usuario y contraseña hasheada con BCrypt (factor de trabajo 12)
- **Clientes**: Autenticación únicamente con número de cédula — no requiere contraseña ya que la identidad se valida por documento

Los claims del token incluyen: `NameIdentifier` (usuario o cédula), `Role`, `Sub`, `Jti`, `Iat`.

### 4.2 Reglas de Negocio

- Los turnos tienen una ventana de **15 minutos** para ser activados desde su creación
- Un cliente no puede crear un nuevo turno si tiene uno **Pendiente o Activo**
- Máximo **5 turnos por cliente por día** (contando todos los estados)
- El ciclo de vida del turno es: `Pendiente → Activo → Atendido` o `Pendiente → Expirado/Cancelado`
- La lógica de transición de estados está encapsulada en la entidad `Appointment` siguiendo el principio de Information Expert

### 4.3 Expiración Automática

Se implementó un `BackgroundService` que verifica cada minuto los turnos con estado `Pendiente` cuyo `ExpiresAt < DateTime.UtcNow` y los marca como `Expirado`. Esto garantiza consistencia en la base de datos independientemente de la actividad del frontend.

### 4.4 Manejo de Errores

- Las violaciones de reglas de negocio lanzan `DomainException` que el middleware captura y devuelve como `400 Bad Request`
- Los errores no controlados retornan `500 Internal Server Error` con un mensaje genérico
- Todos los errores se envuelven en `ApiResponse<T>` con `Success = false` y un mensaje descriptivo

### 4.5 Eficiencia y Escalabilidad

- Todas las operaciones de base de datos son **asíncronas** (`async/await`)
- Los repositorios reciben `CancellationToken` para cancelar operaciones en vuelo
- Se implementaron **índices** en las columnas de búsqueda frecuente: `CustomerIdNumber`, `(CustomerIdNumber, CreatedAt)`, `Status`, `BranchId`
- EF Core utiliza `EnableRetryOnFailure` para reintentos automáticos en fallos transitorios
- El frontend implementa **caché en memoria** para las sucursales, evitando peticiones repetidas
- La arquitectura permite **escalabilidad horizontal** ya que el estado se mantiene en la base de datos y los tokens JWT son stateless

---

## 5. Seguridad

| Aspecto | Implementación |
|---------|----------------|
| Autenticación | JWT Bearer Tokens con expiración configurable |
| Autorización | Role-based: `[Authorize(Roles = "Admin")]` y `[Authorize(Roles = "Client")]` |
| Contraseñas | BCrypt con factor de trabajo 12 |
| CORS | Configurado para permitir solo el origen del frontend (`http://localhost:4200`) |
| Validación | Los DTOs usan Data Annotations para validación de entrada |
| Secretos | La clave JWT y la cadena de conexión están en `appsettings.json` excluido del repositorio con `.gitignore` |
| Campos sensibles | El campo de cédula en el formulario de creación de turno es de solo lectura y se toma del token |

---

## 6. Pruebas Unitarias

### 6.1 Backend — xUnit + Moq

**Proyecto**: `TurnosAPI.Tests`

**Herramientas**:
- `xUnit` — Framework de pruebas
- `Moq` — Mocking de dependencias
- `FluentAssertions` — Aserciones legibles
- `coverlet.collector` — Cobertura de código

**Pruebas implementadas**:

`AppointmentEntityTests` (11 pruebas):
- Creación de turno con datos válidos
- Validación de cédula vacía
- Validación de sucursal vacía
- Activación de turno pendiente dentro de la ventana de tiempo
- Error al activar turno ya activo
- Marcar como atendido cuando está activo
- Error al marcar como atendido cuando está pendiente
- Cancelación de turno pendiente
- Error al cancelar turno ya atendido
- Expiración de turno pendiente
- Error al expirar turno activo

`AppointmentServiceTests` (6 pruebas):
- Crear turno con datos válidos
- Error al crear turno cuando se alcanza el límite diario
- Error al crear turno cuando la sucursal no existe
- Error al activar turno que no pertenece al cliente
- Error al obtener turno por id inexistente
- Error al actualizar con estado inválido

### 6.2 Frontend — Vitest

**Herramientas**:
- `Vitest 4.0.18` — Framework de pruebas incluido en Angular 21
- `@angular/core/testing` — TestBed para pruebas de componentes
- `HttpTestingController` — Mock de peticiones HTTP

**Pruebas implementadas**:

`AuthServiceTests` (9 pruebas):
- Creación del servicio
- Login de cliente guarda token en localStorage
- Login de admin guarda token en localStorage
- Logout limpia localStorage
- `isAuthenticated` retorna true con token
- `isAuthenticated` retorna false sin token
- `isAdmin` retorna true con rol Admin
- `isClient` retorna true con rol Client
- `getToken` retorna token del localStorage

`AppointmentServiceTests` (7 pruebas):
- Creación del servicio
- `create` hace POST al endpoint correcto
- `getAll` hace GET a appointments
- `getMyAppointments` hace GET a my-appointments
- `activate` hace PUT al endpoint de activación
- `updateStatus` hace PUT al endpoint de status
- `getById` hace GET por id

`AuthGuardTests` (5 pruebas):
- Creación del guard
- Retorna true cuando el usuario está autenticado sin rol requerido
- Redirige a login cuando no está autenticado
- Retorna true cuando el usuario tiene el rol requerido
- Redirige a unauthorized cuando el usuario no tiene el rol requerido

**Total: 23 pruebas en backend + 21 pruebas en frontend = 44 pruebas unitarias**

---

## 7. Historial de Versiones

El desarrollo se realizó siguiendo **GitFlow** con ramas de features:

### TurnosAPI
| Rama | Descripción |
|------|-------------|
| `feature/domain-layer` | Entidades, enums, interfaces, excepciones |
| `feature/application-layer` | DTOs, servicios, interfaces de aplicación |
| `feature/infrastructure-layer` | EF Core, repositorios, JWT, BCrypt |
| `feature/api-layer` | Controladores, middleware, background service, Program.cs |
| `feature/tests-and-docs` | Pruebas unitarias y documentación |

### TurnosWeb
| Rama | Descripción |
|------|-------------|
| `feature/core-setup` | Modelos, servicios, interceptor, guards, rutas |
| `feature/ui-components` | Componentes visuales con tema corporativo |
| `feature/docs` | Documentación del frontend |
| `feature/unit-tests` | Pruebas unitarias con Vitest |

---

## 8. Pasos para Ejecutar el Proyecto

### 8.1 Requisitos Previos

- .NET 8 SDK
- Node.js 22+
- Angular CLI 21+
- SQL Server Express
- dotnet-ef tools: `dotnet tool install --global dotnet-ef`

### 8.2 Base de Datos

**Opción A — Migraciones EF Core (recomendado):**
```bash
dotnet ef migrations add InitialCreate --project TurnosAPI.Infrastructure --startup-project TurnosAPI.API
dotnet ef database update --project TurnosAPI.Infrastructure --startup-project TurnosAPI.API
```

**Opción B — Script SQL manual:**
Ejecutar el archivo `database/TurnosDB_Setup.sql` en SQL Server Management Studio.

### 8.3 Backend
```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/TurnosAPI.git
cd TurnosAPI

# Configurar appsettings
cp TurnosAPI.API/appsettings.example.json TurnosAPI.API/appsettings.json
# Editar appsettings.json con la cadena de conexión y JWT settings

# Ejecutar
dotnet run --project TurnosAPI.API --launch-profile http

# Swagger disponible en:
# http://localhost:5005/swagger
```

### 8.4 Frontend
```bash
# Clonar repositorio
git clone https://github.com/tu-usuario/TurnosWeb.git
cd TurnosWeb

# Instalar dependencias
npm install

# Ejecutar
ng serve

# Disponible en: http://localhost:4200
```

### 8.5 Usuarios de Prueba

| Tipo | Credencial | Contraseña |
|------|-----------|------------|
| Administrador | `admin` | `AdminPass5955.*` |
| Cliente | `2222222222` | *(no requiere)* |
| Cliente | `1111111111` | *(no requiere)* |
| Cliente | `3333333333` | *(no requiere)* |

### 8.6 Ejecutar Pruebas

**Backend:**
```bash
cd TurnosAPI
dotnet test
```

**Frontend:**
```bash
cd TurnosWeb
ng test
```

---

## 9. Repositorios

| Proyecto | URL |
|----------|-----|
| Backend API | https://github.com/tu-usuario/TurnosAPI |
| Frontend Angular | https://github.com/tu-usuario/TurnosWeb |