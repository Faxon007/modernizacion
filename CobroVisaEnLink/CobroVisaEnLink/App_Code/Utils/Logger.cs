using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

/// <summary>
/// Descripción breve de Logger
/// </summary>
public static class Logger
{
    // Método para Bitácora (Info, Logs de usuario, etc.)
    public static void LogInfo(string mensaje)
    {
        Escribir("INFO", mensaje);
    }

    // Método para Errores
    public static void LogError(Exception ex, string contexto = "")
    {
        string detalle = string.IsNullOrEmpty(contexto) ? "" : $"Contexto: {contexto}{Environment.NewLine}";
        detalle += $"{ex.Message}{Environment.NewLine}{ex.StackTrace}";
        Escribir("ERROR", detalle);
    }

    private static void Escribir(string tipo, string contenido)
    {
        try
        {
            string folderPath = HttpContext.Current.Server.MapPath("~/Logs/");
            string filePath = Path.Combine(folderPath, $"log_{DateTime.Now:yyyyMMdd}.txt");

            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string linea = $"[{tipo}][{DateTime.Now:HH:mm:ss}] {contenido}{Environment.NewLine}" +
                           new string('-', 30) + Environment.NewLine;

            File.AppendAllText(filePath, linea);
        }
        catch { /* Evita que el log tire la app si falla */ }
    }
}