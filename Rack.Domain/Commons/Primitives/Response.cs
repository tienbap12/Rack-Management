#nullable enable

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Rack.Domain.Enum;

namespace Rack.Domain.Commons.Primitives;

public enum ErrorType
{
    Failure,
    NotFound,
    Validation,
    Conflict,
    Unauthorized,
    Forbidden
}

public sealed class Error(string code, string message, ErrorType type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);

    public string Code { get; } = code;
    public string Message { get; } = message;
    public ErrorType Type { get; } = type;

    public static Error New(string code, string message, ErrorType type = ErrorType.Failure)
           => new(code, message, type);

    public static Error Failure(string code = "General.Failure", string message = "A failure has occurred.")
        => new(code, message, ErrorType.Failure);

    public static Error NotFound(string code = "General.NotFound", string message = "The requested resource was not found.")
        => new(code, message, ErrorType.NotFound);

    public static Error Validation(string code = "General.Validation", string message = "Validation error.")
        => new(code, message, ErrorType.Validation);

    public static Error Conflict(string code = "General.Conflict", string message = "A conflict occurred.")
        => new(code, message, ErrorType.Conflict);

    public static Error Unauthorized(string code = "General.Unauthorized", string message = "Unauthorized access.")
        => new(code, message, ErrorType.Unauthorized);

    public static Error Forbidden(string code = "General.Forbidden", string message = "Forbidden access.")
        => new(code, message, ErrorType.Forbidden);
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
        Message = message ?? (isSuccess ? "Request successful." : error!.Message);
        Error = error ?? Error.None;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public HttpStatusCodeEnum StatusCode { get; }

    public string Message { get; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Error Error { get; }

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

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public T Data => IsSuccess && _data is not null
        ? _data
        : throw new InvalidOperationException("Cannot access data from a failed response or when data is null on success.");

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