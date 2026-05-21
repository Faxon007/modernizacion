namespace Backend.Common
{
    public static class ErrorCatalog
    {
        public static readonly AppError NotFound = new(1, "Recurso no encontrado.", 404);
        public static readonly AppError Unauthorized = new(2, "No autorizado.", 401);
        public static readonly AppError Forbidden = new(3, "Acceso denegado.", 403);
        public static readonly AppError ValidationFailed = new(4, "Error de validación.", 422);
        public static readonly AppError InternalError = new(5, "Error interno del servidor.", 500);
        public static readonly AppError BadRequest = new(6, "Solicitud inválida.", 400);
        public static readonly AppError Conflict = new(7, "Conflicto con el estado actual.", 409);

        public static readonly AppError MissingTransactionId = new(8, "El header 'x-transaction-id' es requerido.", 400);
        public static readonly AppError InvalidTransactionId = new(9, "El header 'x-transaction-id' debe ser un GUID válido.", 400);

        public static readonly AppError InvalidCredentials = new(10, "Usuario o contraseña incorrectos.", 401);
        public static readonly AppError TokenRequired = new(11, "Se requiere autenticación.", 401);
        public static readonly AppError FieldsRequired = new(12, "Username y password son requeridos.", 400);

        public static readonly AppError CampaignNotFound = new(13, "La campaña solicitada no existe.", 404);
        public static readonly AppError CampaignDuplicated = new(14, "Ya existe una campaña registrada con ese código.", 409);
        public static readonly AppError CampaignFieldsRequired = new(15, "Descripción y status son obligatorios.", 400);
        public static readonly AppError CampaignUpdateFailed = new(16, "No se pudo actualizar la campaña.", 500);

        public static readonly AppError ExternalApiError = new(17, "Error al consumir la API externa.", 502);
        public static readonly AppError ExternalApiTimeout = new(18, "Tiempo de espera agotado con la API externa.", 504);
        public static readonly AppError SoapServiceError = new(19, "Error al consumir el servicio SOAP.", 502);
        public static readonly AppError SoapParseError = new(20, "Error al procesar la respuesta SOAP.", 502);
        public static readonly AppError ExternalNotFound = new(21, "Recurso no encontrado en el servicio externo.", 404);
    }
}
