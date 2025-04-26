namespace Rack.Domain.Enum
{
    public enum HttpStatusCodeEnum
    {
        None = 0,
        OK = 200,
        Created = 201,
        BadRequest = 400,
        Unauthorized = 401,
        Forbidden = 403,
        NotFound = 404,
        Conflict = 409,
        InternalServerError = 500
    }

    public enum ErrorCode
    {
        None,
        GeneralFailure,
        GeneralNotFound,
        GeneralBadRequest,
        GeneralValidation,
        GeneralConflict,
        GeneralUnauthorized,
        GeneralForbidden,
        GeneralInternalServerError
    }
}