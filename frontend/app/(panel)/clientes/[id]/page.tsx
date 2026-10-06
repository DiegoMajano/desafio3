"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useParams, useRouter } from "next/navigation";
import { api, ApiError } from "@/lib/api";
import { dinero, fechaCorta } from "@/lib/formato";
import type { Cliente, Orden } from "@/lib/types";
import { Modal } from "@/components/Modal";
import { ClienteForm } from "@/components/ClienteForm";
import { OrdenForm } from "@/components/OrdenForm";
import { useAviso } from "@/components/Aviso";
import { Cargando, ErrorCarga } from "@/components/Estado";

export default function ClienteDetallePage() {
  const { id } = useParams<{ id: string }>();
  const router = useRouter();
  const aviso = useAviso();
  const [cliente, setCliente] = useState<Cliente | null>(null);
  const [ordenes, setOrdenes] = useState<Orden[]>([]);
  const [error, setError] = useState("");
  const [noExiste, setNoExiste] = useState(false);
  const [modal, setModal] = useState<"editar" | "orden" | "eliminar" | null>(null);

  const cargar = useCallback(async () => {
    setError("");
    try {
      const [c, o] = await Promise.all([
        api<Cliente>(`/clientes/${id}`),
        api<Orden[]>(`/ordenes/cliente/${id}`),
      ]);
      setCliente(c);
      setOrdenes(o);
    } catch (err) {
      if (err instanceof ApiError && err.status === 404) setNoExiste(true);
      else setError(err instanceof Error ? err.message : "No se pudo cargar el cliente.");
    }
  }, [id]);

  useEffect(() => { cargar(); }, [cargar]);

  async function eliminar() {
    try {
      await api(`/clientes/${id}`, { method: "DELETE" });
      aviso(`Cliente ${cliente?.nombre} eliminado`);
      router.replace("/clientes");
    } catch (err) {
      aviso(err instanceof Error ? err.message : "No se pudo eliminar el cliente.", "error");
    }
  }

  if (noExiste) {
    return (
      <div className="vacio-grande">
        Este cliente no existe o fue eliminado. <Link href="/clientes">Volver a clientes</Link>
      </div>
    );
  }
  if (error) return <ErrorCarga mensaje={error} onReintentar={cargar} />;
  if (!cliente) return <Cargando />;

  const total = ordenes.reduce((s, o) => s + o.montoTotal, 0);

  return (
    <>
      <Link href="/clientes" className="volver">Clientes</Link>
      <header className="encabezado">
        <div>
          <h1>{cliente.nombre}</h1>
          <p className="secundario">{cliente.email}</p>
          <p className="secundario">Cliente desde el {fechaCorta(cliente.fechaRegistro)}</p>
        </div>
        <div className="grupo-botones">
          <button className="boton boton-secundario" onClick={() => setModal("editar")}>Editar</button>
          <button className="boton boton-secundario peligro" onClick={() => setModal("eliminar")}>Eliminar</button>
          <button className="boton boton-primario" onClick={() => setModal("orden")}>Nueva orden</button>
        </div>
      </header>

      <dl className="datos-cliente">
        <div><dt>Órdenes</dt><dd>{ordenes.length}</dd></div>
        <div><dt>Total facturado</dt><dd>{dinero(total)}</dd></div>
        <div><dt>Ticket promedio</dt><dd>{ordenes.length ? dinero(total / ordenes.length) : "—"}</dd></div>
      </dl>

      <h2 className="subtitulo">Historial de órdenes</h2>
      {ordenes.length === 0 ? (
        <div className="vacio-grande">
          Este cliente todavía no tiene órdenes.{" "}
          <button className="enlace" onClick={() => setModal("orden")}>Registrar una orden</button>
        </div>
      ) : (
        <div className="tabla-envoltura">
          <table className="tabla">
            <thead>
              <tr><th>N.º</th><th>Fecha</th><th className="num">Monto</th></tr>
            </thead>
            <tbody>
              {ordenes.map((o) => (
                <tr key={o.id}>
                  <td className="secundario">#{o.id}</td>
                  <td>{fechaCorta(o.fechaOrden)}</td>
                  <td className="num cifra">{dinero(o.montoTotal)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      <Modal abierto={modal === "editar"} titulo="Editar cliente" onCerrar={() => setModal(null)}>
        <ClienteForm cliente={cliente} onCancelar={() => setModal(null)}
                     onGuardado={() => { setModal(null); aviso("Cambios guardados"); cargar(); }} />
      </Modal>

      <Modal abierto={modal === "orden"} titulo={`Nueva orden para ${cliente.nombre}`} onCerrar={() => setModal(null)}>
        <OrdenForm clienteFijo={cliente} onCancelar={() => setModal(null)}
                   onGuardada={(o) => { setModal(null); aviso(`Orden #${o.id} registrada`); cargar(); }} />
      </Modal>

      <Modal abierto={modal === "eliminar"} titulo="Eliminar cliente" onCerrar={() => setModal(null)}>
        <div className="formulario">
          <p>
            Se eliminará <strong>{cliente.nombre}</strong>
            {ordenes.length > 0 && <> junto con sus {ordenes.length} órdenes</>}. Esta acción no se puede deshacer.
          </p>
          <div className="acciones-form">
            <button className="boton boton-secundario" onClick={() => setModal(null)}>Cancelar</button>
            <button className="boton boton-peligro" onClick={eliminar}>Eliminar cliente</button>
          </div>
        </div>
      </Modal>
    </>
  );
}
