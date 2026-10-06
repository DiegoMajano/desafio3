const moneda = new Intl.NumberFormat("es-SV", { style: "currency", currency: "USD" });
const fecha = new Intl.DateTimeFormat("es-SV", { day: "2-digit", month: "short", year: "numeric" });

// la API guarda las fechas en UTC sin la Z al final
export function aFecha(valor: string) {
  return new Date(/[zZ]|[+-]\d\d:\d\d$/.test(valor) ? valor : `${valor}Z`);
}

export const dinero = (valor: number) => moneda.format(valor);
export const fechaCorta = (valor: string) => fecha.format(aFecha(valor));
