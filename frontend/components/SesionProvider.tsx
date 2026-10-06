"use client";

import { createContext, useCallback, useContext, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api, guardarSesion, leerSesion } from "@/lib/api";
import type { Sesion } from "@/lib/types";

type SesionContexto = {
  sesion: Sesion | null;
  cargando: boolean;
  esAdmin: boolean;
  iniciarSesion: (email: string, password: string) => Promise<void>;
  registrarse: (email: string, password: string) => Promise<void>;
  cerrarSesion: () => void;
};

const Contexto = createContext<SesionContexto | null>(null);

export function SesionProvider({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const [sesion, setSesion] = useState<Sesion | null>(null);
  const [cargando, setCargando] = useState(true);

  useEffect(() => {
    setSesion(leerSesion());
    setCargando(false);

    const alExpirar = () => {
      setSesion(null);
      router.replace("/login?expirada=1");
    };
    window.addEventListener("sesion-expirada", alExpirar);
    return () => window.removeEventListener("sesion-expirada", alExpirar);
  }, [router]);

  const aplicar = useCallback((nueva: Sesion) => {
    guardarSesion(nueva);
    setSesion(nueva);
  }, []);

  const iniciarSesion = useCallback(async (email: string, password: string) => {
    aplicar(await api<Sesion>("/auth/login", { method: "POST", body: JSON.stringify({ email, password }) }));
  }, [aplicar]);

  const registrarse = useCallback(async (email: string, password: string) => {
    aplicar(await api<Sesion>("/auth/register", { method: "POST", body: JSON.stringify({ email, password }) }));
  }, [aplicar]);

  const cerrarSesion = useCallback(() => {
    guardarSesion(null);
    setSesion(null);
    router.replace("/login");
  }, [router]);

  return (
    <Contexto.Provider
      value={{
        sesion,
        cargando,
        esAdmin: sesion?.roles.includes("Admin") ?? false,
        iniciarSesion,
        registrarse,
        cerrarSesion,
      }}
    >
      {children}
    </Contexto.Provider>
  );
}

export function useSesion() {
  const ctx = useContext(Contexto);
  if (!ctx) throw new Error("useSesion debe usarse dentro de SesionProvider");
  return ctx;
}
