"use client";

import { useEffect, useState } from "react";
import { api, ApiError } from "@/lib/api";
import type { Cliente, Orden } from "@/lib/types";

type Props = {
  clienteFijo?: Cliente;
  onGuardada: (orden: Orden) => void;
  onCancelar: () => void;
};

const hoy = () => new Date().toISOString().slice(0, 10);

export function OrdenForm({ clienteFijo, onGuardada, onCancelar }: Props) {
  const [clientes, setClientes] = useState<Cliente[]>(clienteFijo ? [clienteFijo] : []);
  const [clienteId, setClienteId] = useState(clienteFijo ? String(clienteFijo.id) : "");
  const [monto, setMonto] = useState("");
  const [fecha, setFecha] = useState(hoy());
  const [error, setError] = useState("");
  const [guardando, setGuardando] = useState(false);

  useEffect(() => {
    if (clienteFijo) return;
    api<Cliente[]>("/clientes")
      .then(setClientes)
      .catch(() => setError("No se pudo cargar la lista de clientes."));
  }, [clienteFijo]);

  async function enviar(e: React.FormEvent) {
    e.preventDefault();
    setError("");
    setGuardando(true);
    try {
      const orden = await api<Orden>("/ordenes", {
        method: "POST",
        body: JSON.stringify({
          clienteId: Number(clienteId),
          montoTotal: Number(monto),
          fechaOrden: fecha === hoy() ? null : `${fecha}T12:00:00Z`,
        }),
      });
      onGuardada(orden);
    } catch (err) {
      setError(err instanceof ApiError ? err.message : "No se pudo registrar la orden.");
    } finally {
      setGuardando(false);
    }
  }

  return (
    <form onSubmit={enviar} className="formulario">
      <label className="campo">
        <span>Cliente</span>
        <select value={clienteId} onChange={(e) => setClienteId(e.target.value)} required disabled={!!clienteFijo}>
          <option value="" disabled>Selecciona un cliente</option>
          {clientes.map((c) => (
            <option key={c.id} value={c.id}>{c.nombre}</option>
          ))}
        </select>
      </label>
      <div className="campos-fila">
        <label className="campo">
          <span>Monto (USD)</span>
          <input type="number" inputMode="decimal" min="0.01" step="0.01" value={monto}
                 onChange={(e) => setMonto(e.target.value)} required placeholder="0.00" />
        </label>
        <label className="campo">
          <span>Fecha</span>
          <input type="date" value={fecha} max={hoy()} onChange={(e) => setFecha(e.target.value)} required />
        </label>
      </div>
      {error && <p className="error-form" role="alert">{error}</p>}
      <div className="acciones-form">
        <button type="button" className="boton boton-secundario" onClick={onCancelar}>Cancelar</button>
        <button type="submit" className="boton boton-primario" disabled={guardando}>
          {guardando ? "Registrando…" : "Registrar orden"}
        </button>
      </div>
    </form>
  );
}
