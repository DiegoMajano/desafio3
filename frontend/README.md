# Frontend – CompanyManagement

Interfaz web en Next.js 15 (React 19 + TypeScript) para la API de .NET.

## Cómo correrlo

Necesita Node.js 18.18 o superior y la API corriendo en `http://localhost:5151`.

```bash
cd frontend
npm install
npm run dev
```

Abrir http://localhost:3000.

Si la API está en otra dirección, crear un archivo `.env.local` (hay un ejemplo en `.env.example`):

```
API_URL=http://localhost:5151
```

## Cómo se conecta con la API

Next.js reenvía todo lo que empieza con `/api` hacia la API de .NET (ver `next.config.mjs`), así que el navegador nunca habla directo con el puerto 5151 y no hace falta configurar CORS.

El token que devuelve `/api/auth/login` se guarda en `localStorage` y se manda en el header `Authorization` en cada petición (`lib/api.ts`). Si la API responde 401 se borra la sesión y se regresa al login.

## Pantallas

| Ruta              | Qué hace |
|-------------------|----------|
| `/login`          | Inicio de sesión |
| `/registro`       | Crear cuenta (queda con rol User) |
| `/inicio`         | Resumen: totales, clientes que más facturan, órdenes recientes y clientes nuevos sin órdenes |
| `/clientes`       | Lista con búsqueda, crear, editar y eliminar |
| `/clientes/[id]`  | Detalle del cliente con su historial de órdenes y botón para registrar una nueva |
| `/ordenes`        | Todas las órdenes, filtro por cliente y registro de órdenes |
| `/reportes`       | Solo Admin. Enlaces a los 3 reportes de SSRS (normal, PDF y Excel) |

El menú de Reportes solo aparece si el usuario tiene rol Admin; aun así la API es la que valida el permiso.

## Estructura

```
frontend/
├── app/
│   ├── (auth)/          login y registro
│   ├── (panel)/         pantallas que requieren sesión (con el menú lateral)
│   ├── globals.css
│   └── layout.tsx
├── components/          formularios, modal, avisos y manejo de sesión
└── lib/                 cliente de la API, tipos y formatos de fecha/moneda
```
