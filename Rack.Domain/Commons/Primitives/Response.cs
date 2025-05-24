#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rack.Domain.Entities;
using Rack.Domain.Enum;

namespace Rack.Domain.Commons.Primitives;

public enum ErrorType
{
    Failure,
    NotFound,
    Validation,
    Conflict,
    Unauthorized,
    BadRequest,
    Forbidden,
    InternalServerError
}

public sealed class Error(ErrorCode code, string message, ErrorType type = ErrorType.Failure)
{
    public static readonly Error None = new(ErrorCode.None, string.Empty, ErrorType.Failure);

    public ErrorCode Code { get; } = code;
    public string Message { get; } = message;
    public ErrorType Type { get; } = type;

    public static Error New(ErrorCode code, string message, ErrorType type = ErrorType.Failure)
        => new(code, message, type);

    public static Error Failure(ErrorCode code = ErrorCode.GeneralFailure, string message = "Có lỗi xảy ra")
        => new(code, message, ErrorType.Failure);

    public static Error BadRequest(ErrorCode code = ErrorCode.GeneralBadRequest, string message = "Có lỗi xảy ra")
        => new(code, message, ErrorType.Failure);

    public static Error NotFound(ErrorCode code = ErrorCode.GeneralNotFound, string message = "Không tìm thấy")
        => new(code, message, ErrorType.NotFound);

    public static Error Validation(ErrorCode code = ErrorCode.GeneralValidation, string message = "Vui lòng nhập đúng định dạng")
        => new(code, message, ErrorType.Validation);

    public static Error Conflict(ErrorCode code = ErrorCode.GeneralConflict, string message = "Xung đột dữ liệu")
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(ErrorCode code = ErrorCode.GeneralUnauthorized, string message = "Vui lòng đăng nhập để tiếp tục")
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(ErrorCode code = ErrorCode.GeneralForbidden, string message = "Không có quyền truy cập")
        => new(code, message, ErrorType.Forbidden);

    public static Error InternalServerError(ErrorCode code = ErrorCode.GeneralInternalServerError, string message = "Lỗi máy chủ nội bộ")
        => new(code, message, ErrorType.InternalServerError);
}

public class Response
{
    public Response(bool isSuccess, HttpStatusCodeEnum statusCode, string? message = null, Error? error = null)
    {
        if (isSuccess && error is not null)
            throw new InvalidOperationException("Success response cannot contain an error.");
        if (!isSuccess && error is null)
            throw new InvalidOperationException("Failure response must include an error.");

        IsSuccess = isSuccess;
        StatusCode = statusCode;
        Message = message ?? (isSuccess ? "Lấy dữ liệu thành công" : error!.Message);
        Error = isSuccess ? null : error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public HttpStatusCodeEnum StatusCode { get; }

    public string Message { get; }

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public Error? Error { get; }

    public static Response Success(string? message = null, HttpStatusCodeEnum statusCode = HttpStatusCodeEnum.OK)
        => new(true, statusCode, message);

    public static Response Failure(Error error, HttpStatusCodeEnum statusCode = HttpStatusCodeEnum.BadRequest)
        => new(false, statusCode, error.Message, error);

    public static Response Failure(string message, HttpStatusCodeEnum statusCode = HttpStatusCodeEnum.BadRequest)
        => new(false, statusCode, message, Error.Failure(message: message));

    public static implicit operator Response(Error error)
        => Failure(error);
}

public class Response<T> : Response
{
    private readonly T? _data;

    protected Response(T? data, bool isSuccess, HttpStatusCodeEnum statusCode, string? message = null, Error? error = null)
    : base(isSuccess, statusCode, message, error)
    {
        _data = data;
    }

    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault)]
    public T Data => IsSuccess && _data is not null
        ? _data
        : throw new InvalidOperationException("Lỗi tải dữ liệu");

    public static Response<T> Success(T data, string? message = null, HttpStatusCodeEnum statusCode = HttpStatusCodeEnum.OK)
        => new(data, true, statusCode, message);

    public static Response<T> Failure(Error error, HttpStatusCodeEnum statusCode = HttpStatusCodeEnum.BadRequest)
        => new(default, false, statusCode, error.Message, error);

    public static implicit operator Response<T>(T data)
        => Success(data);

    public static implicit operator Response<T>(Error error)
        => Failure(error);
}

public class PagedResponse<T> : Response<IReadOnlyList<T>>
{
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedResponse(IReadOnlyList<T> data, int totalCount, int pageNumber, int pageSize, string? message = null)
        : base(data, true, HttpStatusCodeEnum.OK, message)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0;
    }

    public static PagedResponse<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize,
        string? message = null)
    {
        if (pageNumber <= 0) throw new ArgumentOutOfRangeException(nameof(pageNumber));
        if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));

        return new PagedResponse<T>(items, totalCount, pageNumber, pageSize, message);
    }
}