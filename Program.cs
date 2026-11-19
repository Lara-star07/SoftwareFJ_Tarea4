using System;
using System.Collections.Generic;
using System.IO;

namespace SoftwareFJ
{
    // ==========================================
    // 1. EXCEPCIONES PERSONALIZADAS
    // ==========================================
    public class ClienteInvalidoException : Exception
    {
        public ClienteInvalidoException(string mensaje) : base(mensaje) { }
    }

    public class ServicioNoDisponibleException : Exception
    {
        public ServicioNoDisponibleException(string mensaje) : base(mensaje) { }
    }

    public class ReservaInvalidaException : Exception
    {
        public ReservaInvalidaException(string mensaje) : base(mensaje) { }
    }

    // ==========================================
    // 2. REGISTRO DE EVENTOS (ARCHIVO LOGS.TXT)
    // ==========================================
    public static class Logger
    {
        private static readonly string LogFilePath = "logs.txt";

        public static void RegistrarEvento(string tipo, string mensaje)
        {
            try
            {
                string lineaLog = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{tipo.ToUpper()}] {mensaje}";
                File.AppendAllText(LogFilePath, lineaLog + Environment.NewLine);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al escribir en logs: {ex.Message}");
            }
        }
    }

    // ==========================================
    // 3. ARQUITECTURA ORIENTADA A OBJETOS (POO)
    // ==========================================
    public abstract class EntidadBase
    {
        public int Id { get; protected set; }
        public DateTime FechaCreacion { get; protected set; }

        protected EntidadBase(int id)
        {
            Id = id;
            FechaCreacion = DateTime.Now;
        }
    }

    // Clase Cliente con Encapsulación y Validaciones
    public class Cliente : EntidadBase
    {
        public string Nombre { get; private set; }
        public string Identificacion { get; private set; }
        public string Correo { get; private set; }

        public Cliente(int id, string nombre, string identificacion, string correo) : base(id)
        {
            ValidarDatos(nombre, identificacion, correo);
            Nombre = nombre;
            Identificacion = identificacion;
            Correo = correo;
        }

        private void ValidarDatos(string nombre, string identificacion, string correo)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ClienteInvalidoException("El nombre del cliente no puede estar vacío.");
            if (string.IsNullOrWhiteSpace(identificacion) || identificacion.Length < 5)
                throw new ClienteInvalidoException("La identificación debe tener al menos 5 dígitos.");
            if (string.IsNullOrWhiteSpace(correo) || !correo.Contains("@"))
                throw new ClienteInvalidoException("El correo electrónico no es válido (debe contener '@').");
        }
    }

    // Clase Abstracta Servicio
    public abstract class Servicio : EntidadBase
    {
        public string NombreServicio { get; protected set; }
        public double TarifaBase { get; protected set; }
        public bool Disponible { get; protected set; }

        protected Servicio(int id, string nombreServicio, double tarifaBase, bool disponible) : base(id)
        {
            NombreServicio = nombreServicio;
            TarifaBase = tarifaBase;
            Disponible = disponible;
        }

        // Métodos abstractos (Polimorfismo en subclases)
        public abstract double CalcularCosto(int duracion);
        public abstract string DescribirServicio();
        public abstract void ValidarParametros(int duracion);

        // SOBRECARGA (Overload) del cálculo de costo
        public double CalcularCosto(int duracion, double porcentajeDescuento)
        {
            double costoBase = CalcularCosto(duracion);
            return costoBase - (costoBase * (porcentajeDescuento / 100.0));
        }

        public double CalcularCosto(int duracion, double porcentajeDescuento, double impuestoIVA)
        {
            double costoConDescuento = CalcularCosto(duracion, porcentajeDescuento);
            return costoConDescuento + (costoConDescuento * (impuestoIVA / 100.0));
        }
    }

    // Subclase 1: Reserva de Salas
    public class ReservaSala : Servicio
    {
        public int CapacidadPersonas { get; private set; }

        public ReservaSala(int id, string nombre, double tarifaPorHora, int capacidad, bool disponible)
            : base(id, nombre, tarifaPorHora, disponible)
        {
            CapacidadPersonas = capacidad;
        }

        public override double CalcularCosto(int duracionHoras)
        {
            ValidarParametros(duracionHoras);
            double recargoCapacidad = CapacidadPersonas > 10 ? 1.2 : 1.0;
            return TarifaBase * duracionHoras * recargoCapacidad;
        }

        public override string DescribirServicio()
        {
            return $"Reserva de Sala '{NombreServicio}' (Capacidad: {CapacidadPersonas} personas) - Tarifa/h: ${TarifaBase}";
        }

        public override void ValidarParametros(int duracionHoras)
        {
            if (duracionHoras <= 0 || duracionHoras > 12)
                throw new ServicioNoDisponibleException("La reserva de sala debe ser de entre 1 y 12 horas.");
            if (!Disponible)
                throw new ServicioNoDisponibleException($"La sala '{NombreServicio}' no está disponible.");
        }
    }

    // Subclase 2: Alquiler de Equipos
    public class AlquilerEquipo : Servicio
    {
        public bool IncluyeSeguro { get; private set; }

        public AlquilerEquipo(int id, string nombre, double tarifaPorDia, bool incluyeSeguro, bool disponible)
            : base(id, nombre, tarifaPorDia, disponible)
        {
            IncluyeSeguro = incluyeSeguro;
        }

        public override double CalcularCosto(int duracionDias)
        {
            ValidarParametros(duracionDias);
            double costo = TarifaBase * duracionDias;
            if (IncluyeSeguro) costo += 15.0 * duracionDias;
            return costo;
        }

        public override string DescribirServicio()
        {
            return $"Alquiler de Equipo '{NombreServicio}' (Seguro: {(IncluyeSeguro ? "Sí" : "No")}) - Tarifa/día: ${TarifaBase}";
        }

        public override void ValidarParametros(int duracionDias)
        {
            if (duracionDias <= 0 || duracionDias > 30)
                throw new ServicioNoDisponibleException("El alquiler de equipo debe ser entre 1 y 30 días.");
            if (!Disponible)
                throw new ServicioNoDisponibleException($"El equipo '{NombreServicio}' no está disponible actualmente.");
        }
    }

    // Subclase 3: Asesorías Especializadas
    public class AsesoriaEspecializada : Servicio
    {
        public string Especialista { get; private set; }

        public AsesoriaEspecializada(int id, string nombre, double tarifaHoraria, string especialista, bool disponible)
            : base(id, nombre, tarifaHoraria, disponible)
        {
            Especialista = especialista;
        }

        public override double CalcularCosto(int duracionHoras)
        {
            ValidarParametros(duracionHoras);
            return TarifaBase * duracionHoras;
        }

        public override string DescribirServicio()
        {
            return $"Asesoría Especializada '{NombreServicio}' con {Especialista} - Tarifa/h: ${TarifaBase}";
        }

        public override void ValidarParametros(int duracionHoras)
        {
            if (duracionHoras <= 0 || duracionHoras > 5)
                throw new ServicioNoDisponibleException("Las asesorías se limitan a un máximo de 5 horas por sesión.");
            if (!Disponible)
                throw new ServicioNoDisponibleException($"El especialista {Especialista} no está disponible.");
        }
    }

    // ==========================================
    // 4. CLASE RESERVA
    // ==========================================
    public class Reserva : EntidadBase
    {
        public Cliente ClienteAsociado { get; private set; }
        public Servicio ServicioContratado { get; private set; }
        public int Duracion { get; private set; }
        public string Estado { get; private set; }
        public double CostoTotal { get; private set; }

        public Reserva(int id, Cliente cliente, Servicio servicio, int duracion) : base(id)
        {
            if (cliente == null) throw new ReservaInvalidaException("No se puede crear una reserva sin un cliente válido.");
            if (servicio == null) throw new ReservaInvalidaException("No se puede crear una reserva sin un servicio seleccionado.");

            servicio.ValidarParametros(duracion);

            ClienteAsociado = cliente;
            ServicioContratado = servicio;
            Duracion = duracion;
            CostoTotal = servicio.CalcularCosto(duracion);
            Estado = "Confirmada";
        }

        public void Cancelar()
        {
            if (Estado == "Cancelada")
                throw new ReservaInvalidaException("La reserva ya se encuentra cancelada.");
            
            Estado = "Cancelada";
        }
    }

    // ==========================================
    // 5. PROGRAMA PRINCIPAL (10 OPERACIONES)
    // ==========================================
    class Program
    {
        private static List<Cliente> clientes = new List<Cliente>();
        private static List<Servicio> servicios = new List<Servicio>();
        private static List<Reserva> reservas = new List<Reserva>();

        static void Main(string[] args)
        {
            Console.WriteLine("=============================================================");
            Console.WriteLine(" SISTEMA DE GESTIÓN SOFTWARE FJ - SIMULACIÓN DE OPERACIONES ");
            Console.WriteLine("=============================================================\n");

            Logger.RegistrarEvento("INFO", "Inicio del sistema Software FJ.");

            // -------------------------------------------------------------
            // SIMULACIÓN OBLIGATORIA DE LAS 10 OPERACIONES
            // -------------------------------------------------------------

            // Op 1: Registro de cliente válido
            EjecutarOperacion("Op 1 - Registrar Cliente Válido 1", () => {
                Cliente c1 = new Cliente(1, "Carlos Pérez", "101829384", "carlos@mail.com");
                clientes.Add(c1);
                Console.WriteLine($"[ÉXITO] Cliente registrado: {c1.Nombre}");
            });

            // Op 2: Registro de cliente inválido (sin @ en correo)
            EjecutarOperacion("Op 2 - Registrar Cliente Inválido (Sin @)", () => {
                Cliente c2 = new Cliente(2, "María Gómez", "987654321", "correo_invalido.com");
                clientes.Add(c2);
            });

            // Op 3: Registro de cliente válido 2
            EjecutarOperacion("Op 3 - Registrar Cliente Válido 2", () => {
                Cliente c3 = new Cliente(3, "Ana Rodríguez", "554433221", "ana@empresa.org");
                clientes.Add(c3);
                Console.WriteLine($"[ÉXITO] Cliente registrado: {c3.Nombre}");
            });

            // Op 4: Creación de servicio de Sala válido
            EjecutarOperacion("Op 4 - Crear Servicio de Sala Válido", () => {
                Servicio s1 = new ReservaSala(101, "Sala Ejecutiva A", 50.0, 15, true);
                servicios.Add(s1);
                Console.WriteLine($"[ÉXITO] Servicio creado: {s1.DescribirServicio()}");
            });

            // Op 5: Creación de servicio no disponible
            EjecutarOperacion("Op 5 - Crear Servicio No Disponible", () => {
                Servicio s2 = new AlquilerEquipo(102, "Proyector 4K", 25.0, true, false); // Disponible = false
                servicios.Add(s2);
                Console.WriteLine($"[ÉXITO] Servicio registrado en catálogo (No disponible): {s2.NombreServicio}");
            });

            // Op 6: Creación de servicio de Asesoria
            EjecutarOperacion("Op 6 - Crear Servicio de Asesoría", () => {
                Servicio s3 = new AsesoriaEspecializada(103, "Arquitectura de Software", 100.0, "Dr. Luis Martínez", true);
                servicios.Add(s3);
                Console.WriteLine($"[ÉXITO] Servicio creado: {s3.DescribirServicio()}");
            });

            // Op 7: Procesamiento de reserva exitosa
            EjecutarOperacion("Op 7 - Procesar Reserva Exitosa", () => {
                Cliente cliente = clientes.Find(c => c.Id == 1);
                Servicio servicio = servicios.Find(s => s.Id == 101);
                Reserva r1 = new Reserva(1, cliente, servicio, 4);
                reservas.Add(r1);
                Console.WriteLine($"[ÉXITO] Reserva #{r1.Id} confirmada. Costo Total: ${r1.CostoTotal:F2}");
            });

            // Op 8: Intento de reserva de servicio NO disponible (Fallo controlado)
            EjecutarOperacion("Op 8 - Reservar Equipo No Disponible (Intento Fallido)", () => {
                Cliente cliente = clientes.Find(c => c.Id == 1);
                Servicio servicio = servicios.Find(s => s.Id == 102); // No disponible
                Reserva r2 = new Reserva(2, cliente, servicio, 3);
                reservas.Add(r2);
            });

            // Op 9: Intento de reserva con horas excedidas (Fallo controlado)
            EjecutarOperacion("Op 9 - Reservar Asesoría por 10 Horas (Excede Máximo)", () => {
                Cliente cliente = clientes.Find(c => c.Id == 3);
                Servicio servicio = servicios.Find(s => s.Id == 103);
                Reserva r3 = new Reserva(3, cliente, servicio, 10); // Máximo permitido: 5 horas
                reservas.Add(r3);
            });

            // Op 10: Demostración de Sobrecarga de Métodos y Cancelación
            EjecutarOperacion("Op 10 - Sobrecarga de Métodos y Cancelación", () => {
                Cliente cliente = clientes.Find(c => c.Id == 3);
                Servicio servicio = servicios.Find(s => s.Id == 101);

                // Pruebas de SOBRECARGA (Overload)
                double base3h = servicio.CalcularCosto(3);
                double desc15 = servicio.CalcularCosto(3, 15.0);
                double descIva = servicio.CalcularCosto(3, 15.0, 19.0);

                Console.WriteLine($"[SOBRECARGA] Costo base 3h: ${base3h:F2}");
                Console.WriteLine($"[SOBRECARGA] Costo con 15% desc: ${desc15:F2}");
                Console.WriteLine($"[SOBRECARGA] Costo con desc + 19% IVA: ${descIva:F2}");

                Reserva r4 = new Reserva(4, cliente, servicio, 3);
                reservas.Add(r4);
                r4.Cancelar();
                Console.WriteLine($"[ÉXITO] Reserva #{r4.Id} confirmada y posteriormente Cancelada.");
            });

            Console.WriteLine("\n=============================================================");
            Console.WriteLine(" SIMULACIÓN COMPLETADA SIN COLAPSO NI INTERRUPCIONES DE CÓDIGO");
            Console.WriteLine($" Total Reservas en Lista Interna: {reservas.Count}");
            Console.WriteLine(" Revisa el archivo 'logs.txt' generado en la carpeta de ejecución.");
            Console.WriteLine("=============================================================");

            Logger.RegistrarEvento("INFO", "Fin de la simulación de Software FJ.");
        }

        // Estructura de manejo de excepciones y registro de logs
        private static void EjecutarOperacion(string nombreOperacion, Action accion)
        {
            Console.WriteLine($"\n--- Ejecutando: {nombreOperacion} ---");
            try
            {
                accion();
                Logger.RegistrarEvento("SUCCESS", $"Operación exitosa: {nombreOperacion}");
            }
            catch (ClienteInvalidoException ex)
            {
                Console.WriteLine($"[EXCEPCIÓN CAPTURADA - CLIENTE] {ex.Message}");
                Logger.RegistrarEvento("ERROR", $"[ClienteInvalidoException] {nombreOperacion}: {ex.Message}");
            }
            catch (ServicioNoDisponibleException ex)
            {
                Console.WriteLine($"[EXCEPCIÓN CAPTURADA - SERVICIO] {ex.Message}");
                Logger.RegistrarEvento("ERROR", $"[ServicioNoDisponibleException] {nombreOperacion}: {ex.Message}");
            }
            catch (ReservaInvalidaException ex)
            {
                Console.WriteLine($"[EXCEPCIÓN CAPTURADA - RESERVA] {ex.Message}");
                Logger.RegistrarEvento("ERROR", $"[ReservaInvalidaException] {nombreOperacion}: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EXCEPCIÓN CAPTURADA - GENERAL] {ex.Message}");
                Logger.RegistrarEvento("ERROR", $"[Exception General] {nombreOperacion}: {ex.Message}");
            }
            finally
            {
                // Bloque finally exigido por la guía de la tarea
            }
        }
    }
