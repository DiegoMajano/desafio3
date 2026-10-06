"use client";

import { createContext, useCallback, useContext, useState } from "react";

type Tipo = "ok" | "error";
type AvisoItem = { id: number; texto: string; tipo: Tipo };

const Contexto = createContext<(texto: string, tipo?: Tipo) => void>(() => {});

export function AvisoProvider({ children }: { children: React.ReactNode }) {
  const [avisos, setAvisos] = useState<AvisoItem[]>([]);

  const mostrar = useCallback((texto: string, tipo: Tipo = "ok") => {
    const id = Date.now() + Math.random();
    setAvisos((a) => [...a, { id, texto, tipo }]);
    setTimeout(() => setAvisos((a) => a.filter((x) => x.id !== id)), 4000);
  }, []);

  return (
    <Contexto.Provider value={mostrar}>
      {children}
      <div className="avisos" role="status" aria-live="polite">
        {avisos.map((a) => (
          <div key={a.id} className={`aviso aviso-${a.tipo}`}>{a.texto}</div>
        ))}
      </div>
    </Contexto.Provider>
  );
}

export const useAviso = () => useContext(Contexto);
