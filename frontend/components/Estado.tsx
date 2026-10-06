export function Cargando({ texto = "Cargando…" }: { texto?: string }) {
  return <p className="estado">{texto}</p>;
}

export function ErrorCarga({ mensaje, onReintentar }: { mensaje: string; onReintentar: () => void }) {
  return (
    <div className="estado estado-error">
      <p>{mensaje}</p>
      <button className="boton boton-secundario" onClick={onReintentar}>Reintentar</button>
    </div>
  );
}
