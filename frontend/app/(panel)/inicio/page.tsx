"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import Link from "next/link";
import { api } from "@/lib/api";
import { aFecha, dinero, fechaCorta } from "@/lib/formato";
import type { Cliente, Orden } from "@/lib/types";
import { useSesion } from "@/components/SesionProvider";
import { Cargando, ErrorCarga } from "@/components/Estado";

const UN_MES = 30 * 24 * 60 * 60 * 1000;

export default function InicioPage() {
  const { sesion } = useSesion();
  const [clientes, setClientes] = useState<Cliente[] | null>(null);
  const [ordenes, setOrdenes] = useState<Orden[] | null>(null);
  const [error, setError] = useState("");

  const cargar = useCallback(async () => {
    setError("");
    try {
      const [c, o] = await Promise.all([api<Cliente[]>("/clientes"), api<Orden[]>("/ordenes")]);
      setClientes(c);
      setOrdenes(o);
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudo cargar el resumen.");
    }
  }, []);

  useEffect(() => { cargar(); }, [cargar]);

  const resumen = useMemo(() => {
    if (!clientes || !ordenes) return null;

    const ingresos = ordenes.reduce((s, o) => s + o.montoTotal, 0);
    const porCliente = new Map<number, { nombre: string; total: number }>();
    for (const o of ordenes) {
      const actual = porCliente.get(o.clienteId) ?? { nombre: o.nombreCliente, total: 0 };
      actual.total += o.montoTotal;
      porCliente.set(o.clienteId, actual);
    }
    const ranking = [...porCliente.entries()]
      .map(([id, v]) => ({ id, ...v }))
      .sort((a, b) => b.total - a.total)
      .slice(0, 6);

    const limite = Date.now() - UN_MES;
    const nuevosSinOrdenes = clientes
      .filter((c) => c.totalOrdenes === 0 && aFecha(c.fechaRegistro).getTime() >= limite);

    return { ingresos, ranking, nuevosSinOrdenes, recientes: ordenes.slice(0, 6) };
  }, [clientes, ordenes]);

  if (error) return <ErrorCarga mensaje={error} onReintentar={cargar} />;
  if (!resumen || !clientes || !ordenes) return <Cargando />;

  const maximo = resumen.ranking[0]?.total ?? 1;
  const nombre = sesion?.email.split("@")[0];

  return (
    <>
      <header className="encabezado">
        <div>
          <h1>Hola, {nombre}</h1>
          <p className="resumen-frase">
            Llevas <strong>{clientes.length}</strong> clientes y <strong>{ordenes.length}</strong> órdenes,
            con <strong>{dinero(resumen.ingresos)}</strong> facturados en total.
          </p>
        </div>
      </header>

      <div className="tablero">
        <section className="bloque bloque-ancho">
          <h2>Clientes que más facturan</h2>
          {resumen.ranking.length === 0 ? (
            <p className="vacio">Todavía no hay órdenes. <Link href="/ordenes">Registra la primera</Link> para ver el ranking.</p>
          ) : (
            <ol className="ranking">
              {resumen.ranking.map((r) => (
                <li key={r.id}>
                  <Link href={`/clientes/${r.id}`} className="ranking-nombre">{r.nombre}</Link>
                  <span className="ranking-barra" aria-hidden="true">
                    <span style={{ width: `${Math.max(4, (r.total / maximo) * 100)}%` }} />
                  </span>
                  <span className="ranking-monto">{dinero(r.total)}</span>
                </li>
              ))}
            </ol>
          )}
        </section>

        <section className="bloque">
          <h2>Órdenes recientes</h2>
          {resumen.recientes.length === 0 ? (
            <p className="vacio">Sin órdenes por ahora.</p>
          ) : (
            <ul className="lista-simple">
              {resumen.recientes.map((o) => (
                <li key={o.id}>
                  <div>
                    <Link href={`/clientes/${o.clienteId}`}>{o.nombreCliente}</Link>
                    <span className="secundario">{fechaCorta(o.fechaOrden)}</span>
                  </div>
                  <span className="cifra">{dinero(o.montoTotal)}</span>
                </li>
              ))}
            </ul>
          )}
        </section>

        <section className="bloque">
          <h2>Clientes nuevos sin órdenes</h2>
          <p className="secundario">Se registraron en los últimos 30 días y aún no compran.</p>
          {resumen.nuevosSinOrdenes.length === 0 ? (
            <p className="vacio">Todos los clientes nuevos ya tienen al menos una orden.</p>
          ) : (
            <ul className="lista-simple">
              {resumen.nuevosSinOrdenes.map((c) => (
                <li key={c.id}>
                  <div>
                    <Link href={`/clientes/${c.id}`}>{c.nombre}</Link>
                    <span className="secundario">{c.email}</span>
                  </div>
                  <span className="secundario">{fechaCorta(c.fechaRegistro)}</span>
                </li>
              ))}
            </ul>
          )}
        </section>
      </div>
    </>
  );
}
