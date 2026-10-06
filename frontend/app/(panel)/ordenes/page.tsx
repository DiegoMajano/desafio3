"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { dinero, fechaCorta } from "@/lib/formato";
import type { Orden } from "@/lib/types";
import { Modal } from "@/components/Modal";
import { OrdenForm } from "@/components/OrdenForm";
import { useAviso } from "@/components/Aviso";
import { Cargando, ErrorCarga } from "@/components/Estado";

export default function OrdenesPage() {
  const aviso = useAviso();
  const [ordenes, setOrdenes] = useState<Orden[] | null>(null);
  const [error, setError] = useState("");
  const [clienteFiltro, setClienteFiltro] = useState("");
  const [creando, setCreando] = useState(false);

  const cargar = useCallback(async () => {
    setError("");
    try {
      setOrdenes(await api<Orden[]>("/ordenes"));
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudieron cargar las órdenes.");
    }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const clientes = useMemo(() => {
    const mapa = new Map<number, string>();
    ordenes?.forEach((o) => mapa.set(o.clienteId, o.nombreCliente));
    return [...mapa.entries()].sort((a, b) => a[1].localeCompare(b[1]));
  }, [ordenes]);

  const visibles = useMemo(
    () => (clienteFiltro ? ordenes?.filter((o) => String(o.clienteId) === clienteFiltro) : ordenes),
    [ordenes, clienteFiltro],
  );

  const total = visibles?.reduce((s, o) => s + o.montoTotal, 0) ?? 0;

  return (
    <>
      <header className="encabezado">
        <h1>Órdenes</h1>
        <button className="boton boton-primario" onClick={() => setCreando(true)}>Nueva orden</button>
      </header>

      <div className="herramientas">
        <select value={clienteFiltro} onChange={(e) => setClienteFiltro(e.target.value)} aria-label="Filtrar por cliente">
          <option value="">Todos los clientes</option>
          {clientes.map(([id, nombre]) => <option key={id} value={id}>{nombre}</option>)}
        </select>
        {visibles && <span className="secundario">{visibles.length} órdenes por {dinero(total)}</span>}
      </div>

      {error ? <ErrorCarga mensaje={error} onReintentar={cargar} /> : !visibles ? <Cargando /> : (
        visibles.length === 0 ? (
          <div className="vacio-grande">
            Aún no hay órdenes. <button className="enlace" onClick={() => setCreando(true)}>Registra la primera</button>.
          </div>
        ) : (
          <div className="tabla-envoltura">
            <table className="tabla">
              <thead>
                <tr><th>N.º</th><th>Cliente</th><th>Fecha</th><th className="num">Monto</th></tr>
              </thead>
              <tbody>
                {visibles.map((o) => (
                  <tr key={o.id}>
                    <td className="secundario">#{o.id}</td>
                    <td><Link href={`/clientes/${o.clienteId}`} className="enlace-fila">{o.nombreCliente}</Link></td>
                    <td className="secundario">{fechaCorta(o.fechaOrden)}</td>
                    <td className="num cifra">{dinero(o.montoTotal)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )
      )}

      <Modal abierto={creando} titulo="Nueva orden" onCerrar={() => setCreando(false)}>
        <OrdenForm onCancelar={() => setCreando(false)}
                   onGuardada={(o) => { setCreando(false); aviso(`Orden #${o.id} registrada`); cargar(); }} />
      </Modal>
    </>
  );
}
