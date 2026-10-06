"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { fechaCorta } from "@/lib/formato";
import type { Cliente } from "@/lib/types";
import { Modal } from "@/components/Modal";
import { ClienteForm } from "@/components/ClienteForm";
import { useAviso } from "@/components/Aviso";
import { Cargando, ErrorCarga } from "@/components/Estado";

export default function ClientesPage() {
  const aviso = useAviso();
  const [clientes, setClientes] = useState<Cliente[] | null>(null);
  const [error, setError] = useState("");
  const [busqueda, setBusqueda] = useState("");
  const [editando, setEditando] = useState<Cliente | null>(null);
  const [creando, setCreando] = useState(false);
  const [eliminando, setEliminando] = useState<Cliente | null>(null);

  const cargar = useCallback(async () => {
    setError("");
    try {
      setClientes(await api<Cliente[]>("/clientes"));
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudieron cargar los clientes.");
    }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const filtrados = useMemo(() => {
    const texto = busqueda.trim().toLowerCase();
    if (!clientes || !texto) return clientes;
    return clientes.filter((c) => c.nombre.toLowerCase().includes(texto) || c.email.toLowerCase().includes(texto));
  }, [clientes, busqueda]);

  async function confirmarEliminar() {
    if (!eliminando) return;
    try {
      await api(`/clientes/${eliminando.id}`, { method: "DELETE" });
      aviso(`Cliente ${eliminando.nombre} eliminado`);
      setEliminando(null);
      cargar();
    } catch (err) {
      aviso(err instanceof Error ? err.message : "No se pudo eliminar el cliente.", "error");
    }
  }

  return (
    <>
      <header className="encabezado">
        <h1>Clientes</h1>
        <button className="boton boton-primario" onClick={() => setCreando(true)}>Nuevo cliente</button>
      </header>

      <div className="herramientas">
        <input type="search" className="buscador" placeholder="Buscar por nombre o email"
               value={busqueda} onChange={(e) => setBusqueda(e.target.value)} aria-label="Buscar clientes" />
        {clientes && <span className="secundario">{filtrados?.length} de {clientes.length}</span>}
      </div>

      {error ? <ErrorCarga mensaje={error} onReintentar={cargar} /> : !filtrados ? <Cargando /> : (
        filtrados.length === 0 ? (
          <div className="vacio-grande">
            {clientes?.length === 0
              ? <>Aún no hay clientes. <button className="enlace" onClick={() => setCreando(true)}>Crea el primero</button>.</>
              : <>Ningún cliente coincide con “{busqueda}”.</>}
          </div>
        ) : (
          <div className="tabla-envoltura">
            <table className="tabla">
              <thead>
                <tr>
                  <th>Nombre</th>
                  <th>Email</th>
                  <th>Registro</th>
                  <th className="num">Órdenes</th>
                  <th><span className="sr-only">Acciones</span></th>
                </tr>
              </thead>
              <tbody>
                {filtrados.map((c) => (
                  <tr key={c.id}>
                    <td><Link href={`/clientes/${c.id}`} className="enlace-fila">{c.nombre}</Link></td>
                    <td className="secundario">{c.email}</td>
                    <td className="secundario">{fechaCorta(c.fechaRegistro)}</td>
                    <td className="num">{c.totalOrdenes}</td>
                    <td className="acciones-fila">
                      <button className="boton-texto" onClick={() => setEditando(c)}>Editar</button>
                      <button className="boton-texto peligro" onClick={() => setEliminando(c)}>Eliminar</button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )
      )}

      <Modal abierto={creando} titulo="Nuevo cliente" onCerrar={() => setCreando(false)}>
        <ClienteForm
          onCancelar={() => setCreando(false)}
          onGuardado={(c) => { setCreando(false); aviso(`Cliente ${c.nombre} creado`); cargar(); }}
        />
      </Modal>

      <Modal abierto={!!editando} titulo="Editar cliente" onCerrar={() => setEditando(null)}>
        {editando && (
          <ClienteForm
            cliente={editando}
            onCancelar={() => setEditando(null)}
            onGuardado={() => { setEditando(null); aviso("Cambios guardados"); cargar(); }}
          />
        )}
      </Modal>

      <Modal abierto={!!eliminando} titulo="Eliminar cliente" onCerrar={() => setEliminando(null)}>
        {eliminando && (
          <div className="formulario">
            <p>
              Se eliminará <strong>{eliminando.nombre}</strong>
              {eliminando.totalOrdenes > 0 && <> junto con sus {eliminando.totalOrdenes} órdenes</>}.
              Esta acción no se puede deshacer.
            </p>
            <div className="acciones-form">
              <button className="boton boton-secundario" onClick={() => setEliminando(null)}>Cancelar</button>
              <button className="boton boton-peligro" onClick={confirmarEliminar}>Eliminar cliente</button>
            </div>
          </div>
        )}
      </Modal>
    </>
  );
}
