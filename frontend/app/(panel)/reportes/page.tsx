"use client";

import { useCallback, useEffect, useState } from "react";
import { api } from "@/lib/api";
import type { Reporte } from "@/lib/types";
import { useSesion } from "@/components/SesionProvider";
import { Cargando, ErrorCarga } from "@/components/Estado";

const RUTAS = [
  { ruta: "/reportes/clientes-activos", titulo: "Clientes activos", columnas: "Nombre, email y total de órdenes" },
  { ruta: "/reportes/ingresos-clientes", titulo: "Ingresos por cliente", columnas: "Nombre, email y monto total" },
  { ruta: "/reportes/clientes-inactivos", titulo: "Clientes inactivos", columnas: "Nombre, email y fecha de registro" },
];

type Fila = (typeof RUTAS)[number] & { datos: Reporte };

export default function ReportesPage() {
  const { esAdmin } = useSesion();
  const [reportes, setReportes] = useState<Fila[] | null>(null);
  const [error, setError] = useState("");

  const cargar = useCallback(async () => {
    setError("");
    try {
      const datos = await Promise.all(RUTAS.map((r) => api<Reporte>(r.ruta)));
      setReportes(RUTAS.map((r, i) => ({ ...r, datos: datos[i] })));
    } catch (err) {
      setError(err instanceof Error ? err.message : "No se pudieron obtener los reportes.");
    }
  }, []);

  useEffect(() => { if (esAdmin) cargar(); }, [esAdmin, cargar]);

  if (!esAdmin) {
    return (
      <>
        <header className="encabezado"><h1>Reportes</h1></header>
        <div className="vacio-grande">Los reportes solo están disponibles para usuarios administradores.</div>
      </>
    );
  }

  return (
    <>
      <header className="encabezado">
        <div>
          <h1>Reportes</h1>
          <p className="secundario">Se abren en SQL Server Reporting Services, en una pestaña nueva.</p>
        </div>
      </header>

      {error ? <ErrorCarga mensaje={error} onReintentar={cargar} /> : !reportes ? <Cargando /> : (
        <ul className="reportes">
          {reportes.map((r) => (
            <li key={r.ruta} className="reporte">
              <div className="reporte-info">
                <h2>{r.titulo}</h2>
                <p>{r.datos.descripcion}</p>
                <p className="secundario">Columnas: {r.columnas}</p>
              </div>
              <div className="grupo-botones">
                <a className="boton boton-primario" href={r.datos.url} target="_blank" rel="noreferrer">Abrir reporte</a>
                <a className="boton boton-secundario" href={r.datos.urlPdf} target="_blank" rel="noreferrer">PDF</a>
                <a className="boton boton-secundario" href={r.datos.urlExcel} target="_blank" rel="noreferrer">Excel</a>
              </div>
            </li>
          ))}
        </ul>
      )}
    </>
  );
}
