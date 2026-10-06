import type { Sesion } from "./types";

const CLAVE_SESION = "cm_sesion";

export class ApiError extends Error {
  constructor(public status: number, message: string) {
    super(message);
  }
}

export function leerSesion(): Sesion | null {
  if (typeof window === "undefined") return null;
  try {
    const raw = localStorage.getItem(CLAVE_SESION);
    if (!raw) return null;
    const sesion = JSON.parse(raw) as Sesion;
    if (new Date(sesion.expira).getTime() <= Date.now()) {
      localStorage.removeItem(CLAVE_SESION);
      return null;
    }
    return sesion;
  } catch {
    return null;
  }
}

export function guardarSesion(sesion: Sesion | null) {
  try {
    if (sesion) localStorage.setItem(CLAVE_SESION, JSON.stringify(sesion));
    else localStorage.removeItem(CLAVE_SESION);
  } catch {
    // sin localStorage la sesión solo vive mientras la pestaña esté abierta
  }
}

async function mensajeDeError(res: Response): Promise<string> {
  try {
    const data = await res.json();
    if (data?.mensaje) return data.mensaje;
    if (data?.errores) return [].concat(data.errores).join(" ");
    if (data?.errors) {
      return Object.values(data.errors as Record<string, string[]>).flat().join(" ");
    }
  } catch {
    // la respuesta no trae JSON
  }

  switch (res.status) {
    case 401: return "Tu sesión terminó. Inicia sesión de nuevo.";
    case 403: return "Tu usuario no tiene permiso para esta sección.";
    case 404: return "No se encontró lo que buscabas.";
    default: return `La API respondió con un error (${res.status}).`;
  }
}

export async function api<T>(ruta: string, opciones: RequestInit = {}): Promise<T> {
  const sesion = leerSesion();
  const headers = new Headers(opciones.headers);
  if (opciones.body) headers.set("Content-Type", "application/json");
  if (sesion) headers.set("Authorization", `Bearer ${sesion.token}`);

  let res: Response;
  try {
    res = await fetch(`/api${ruta}`, { ...opciones, headers });
  } catch {
    throw new ApiError(0, "No hay conexión con la API. Revisa que esté corriendo.");
  }

  if (res.status === 401 && sesion) {
    guardarSesion(null);
    window.dispatchEvent(new Event("sesion-expirada"));
  }

  if (!res.ok) {
    throw new ApiError(res.status, await mensajeDeError(res));
  }

  if (res.status === 204) return undefined as T;
  return (await res.json()) as T;
}
