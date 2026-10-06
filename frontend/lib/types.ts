export type Cliente = {
  id: number;
  nombre: string;
  email: string;
  fechaRegistro: string;
  totalOrdenes: number;
};

export type Orden = {
  id: number;
  clienteId: number;
  nombreCliente: string;
  fechaOrden: string;
  montoTotal: number;
};

export type Reporte = {
  reporte: string;
  descripcion: string;
  url: string;
  urlPdf: string;
  urlExcel: string;
};

export type Sesion = {
  token: string;
  expira: string;
  email: string;
  roles: string[];
};
