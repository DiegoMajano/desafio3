"use client";

import { useEffect, useRef } from "react";

type Props = {
  abierto: boolean;
  titulo: string;
  onCerrar: () => void;
  children: React.ReactNode;
};

export function Modal({ abierto, titulo, onCerrar, children }: Props) {
  const ref = useRef<HTMLDialogElement>(null);

  useEffect(() => {
    const dialogo = ref.current;
    if (!dialogo) return;
    if (abierto && !dialogo.open) dialogo.showModal();
    if (!abierto && dialogo.open) dialogo.close();
  }, [abierto]);

  return (
    <dialog
      ref={ref}
      className="modal"
      onClose={onCerrar}
      onClick={(e) => { if (e.target === ref.current) onCerrar(); }}
      aria-labelledby="modal-titulo"
    >
      <div className="modal-cuerpo">
        <header className="modal-encabezado">
          <h2 id="modal-titulo">{titulo}</h2>
          <button type="button" className="boton-icono" onClick={onCerrar} aria-label="Cerrar">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
              <path d="M6 6l12 12M18 6L6 18" />
            </svg>
          </button>
        </header>
        {abierto && children}
      </div>
    </dialog>
  );
}
