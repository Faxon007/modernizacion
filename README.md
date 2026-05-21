# CobroVisaEnLink - Sistema Modernizado

Este repositorio contiene la versión modernizada y desacoplada del sistema **CobroVisaEnLink**, migrado de WebForms ASP.NET legacy a una arquitectura moderna y escalable.

---

## Arquitectura del Proyecto (Monorepo)

El proyecto está organizado en un único repositorio para facilitar el desarrollo, pruebas y despliegue integrado:

```text
/ (Raíz del Repositorio)
├── .vscode/               # Tareas automatizadas para Visual Studio Code
├── Backend/               # API Web en ASP.NET Core (.NET 8)
│   ├── Controllers/       # Endpoints REST expuestos
│   ├── Models/            # Modelos de datos estructurados
│   ├── Repositories/      # Capa de datos (Mock y Dapper para Oracle)
│   └── Services/          # Lógica de negocio (VisaEnLink y acortador de URLs)
├── Frontend/              # Interfaz SPA en Angular 18+ con Tailwind CSS
│   ├── src/app/core/      # Guardias, Interceptores, Servicios y Stores (Signals)
│   └── src/app/feature/   # Componentes y pantallas de negocio rediseñados
├── .gitignore             # Filtro maestro de archivos a ignorar en Git
└── README.md              # Documentación general de instalación y uso
```

---

## Requisitos Previos

Antes de clonar y ejecutar el proyecto, asegúrate de tener instalado:
* **.NET 8 SDK** (para el Backend)
* **Node.js** v18 o superior y **npm** (para el Frontend)
* **Visual Studio Code** (con la extensión recomendada C# Dev Kit de Microsoft)

---

## Cómo Ejecutar el Proyecto desde VS Code

Hemos configurado **tareas automáticas** en `.vscode/tasks.json` para que puedas levantar la aplicación completa con un solo comando:

1. Abre la carpeta raíz del proyecto en Visual Studio Code.
2. Abre la paleta de comandos (`Ctrl+Shift+P` en Windows/Linux o `Cmd+Shift+P` en Mac).
3. Selecciona **Tasks: Run Build Task** (o presiona el atajo directo `Ctrl+Shift+B`).
4. Selecciona la opción **Ejecutar Aplicación Completa (Backend + Frontend)**.
5. VS Code abrirá terminales integradas e iniciará automáticamente:
   * **Backend**: Levantará en `http://localhost:5000`
   * **Frontend**: Levantará en `http://localhost:4200`

---

## Modo de Prueba Offline / Datos Simulados (Mock Mode)

Para probar e interactuar con la aplicación sin necesidad de configurar una conexión de base de datos Oracle o certificados de red física:

### 1. Configuración en el Backend
En [appsettings.json](file:///Backend/appsettings.json), la bandera `UseMockData` está activa por defecto:
```json
"Database": {
  "UseMockData": true
}
```
* **Qué hace**: Registra repositorios de pruebas mock. Omite la lectura/desencriptación de `db.cef2` y simula la base de datos Oracle con datos estáticos (clientes, menús dinámicos, listas de cobros y parámetros).

### 2. Configuración en el Frontend
En [environment.development.ts](file:///Frontend/src/environments/environment.development.ts), la bandera `useDummyData` está activa por defecto:
```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000',
  useDummyData: true
};
```
* **Qué hace**: Activa el `mockInterceptor` en Angular. Al loguearte con cualquier usuario en la vista de Login, el interceptor simulará el login exitoso, creará un JWT token dummy, y cargará la navegación dinámica estructurada.

---

## Headers y Seguridad del API (`x-transaction-id`)

El backend exige en cada endpoint de negocio que se envíe un GUID en el header `x-transaction-id`.

* **Pruebas en Swagger**: Al acceder a la interfaz Swagger (`http://localhost:5000`), el header `x-transaction-id` vendrá preconfigurado con el valor sugerido de prueba `3fa85f64-5717-4562-b3fc-2c963f66afa6` para que puedas probar los endpoints directamente.
* **Integración del Frontend**: El interceptor de Angular `auth.ts` genera automáticamente un GUID único e inyecta el header `x-transaction-id` en cada petición HTTP de forma transparente al usuario.
