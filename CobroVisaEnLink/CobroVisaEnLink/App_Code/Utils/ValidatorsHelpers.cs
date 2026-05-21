using System.Text.RegularExpressions;
/// <summary>
/// Clase estatica pra validar funcionalidades generales.
/// </summary>
public class ValidatorsHelpers
{

    /// <summary>
    /// Metodo ValidateEmail(string email): Metodo que valida por medio de Regex si un email es valido o no.
    /// </summary>
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;

        return Regex.IsMatch(email.Trim(), @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z", RegexOptions.IgnoreCase);
    }


    /// <summary>
    /// Metodo ValidateTelefono(string telefono): Metodo que valida por medio de Regex si el numero de telefono es valido.
    /// </summary>
    public static bool ValidateTelefono(string telefono)
    {
        if (string.IsNullOrEmpty(telefono))
            return false;

        return Regex.IsMatch(telefono, @"^([\S]|[\d]{8})$");
    }
}