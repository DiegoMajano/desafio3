"use client";

import { useState } from "react";
import { api, ApiError } from "@/lib/api";
import type { Cliente } from "@/lib/types";

type Props = {
  cliente?: Cliente;
  onGuardado: (cliente: Cliente) => void;
  onCancelar: () => void;
};

export function ClienteForm({ cliente, onGuardado, onCancelar }: Props) {
  const [nombre, setNombre] = useState(cliente?.nombre ?? "");
  const [email, setEmail] = useState(cliente?.email ?? "");
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);

  async function enviar(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setGuardando(true);
    try {
      const body = JSON.stringify({ nombre, email });
      const guardado = cliente
        ? await api<Cliente>(`/clientes/${cliente.id}`, { method: "PUT", body })
        : await api<Cliente>("/clientes", { method: "POST", body });
      onGuardado(guardado);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo guardar el cliente.");
    } finally {
      setGuardando(false);
    }
  }

  return (
    <form onSubmit={enviar} className="formulario">
      <label className="campo">
        <span>Nombre</span>
        <input value={nombre} onChange={(e) => setNombre(e.target.value)} required minLength={2} maxLength={100} autoFocus />
      </label>
      <label className="campo">
        <span>Email</span>
        <input type="email" value={email} onChange={(e) => setEmail(e.target.value)} required maxLength={150} />
      </label>
      {error && <p className="error-form" role="alert">{error}</p>}
      <div className="acciones-form">
        <button type="button" className="boton boton-secundario" onClick={onCancelar}>Cancelar</button>
        <button type="submit" className="boton boton-primario" disabled={guardando}>
          {guardando ? "Guardando…" : cliente ? "Guardar cambios" : "Crear cliente"}
        </button>
      </div>
    </form>
  );
}
