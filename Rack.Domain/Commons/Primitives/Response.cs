using System;
using System.Collections.Generic;

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

public sealed class Error
{
    public static Error None => new Error(string.Empty, string.Empty, ErrorType.Failure);

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error New(string code, string message, ErrorType type = ErrorType.Failure)
        => new Error(code, message, type);

    public static Error NotFound(string message)
        => new Error("404", message, ErrorType.NotFound);

    public static Error Validation(string message)
        => new Error("400", message, ErrorType.Validation);

    public static Error Conflict(string message)
        => new Error("409", message, ErrorType.Conflict);

    public static Error Unauthorized(string message)
        => new Error("401", message, ErrorType.Unauthorized);

    public static Error Forbidden(string message)
        => new Error("403", message, ErrorType.Forbidden);
}

public class Response
{
    protected Response(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Success response cannot have error");

        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Failure response requires error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public string Message { get; }

    public Response(string message)
    {
        Message = message;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Response Success()
        => new Response(true, Error.None);

    public static Response Failure(Error error)
        => new Response(false, error);

    public static Response Failure(string message)
        => new Response(message);

    public static implicit operator Response(Error error)
        => Failure(error);
}

public class Response<T> : Response
{
    private readonly T _data;

    protected Response(T data, bool isSuccess, Error error)
        : base(isSuccess, error)
    {
        _data = data;
    }

    public Response(string message)
        : base(message)
    {
    }

    public T Data => IsSuccess
        ? _data
        : throw new InvalidOperationException("Cannot access data from failed response");

    public static Response<T> Success(T data)
        => new Response<T>(data, true, Error.None);

    public new static Response<T> Failure(Error error)
        => new Response<T>(default!, false, error);

    public new static Response<T> Failure(string message)
        => new Response<T>(message);

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

    private PagedResponse(
        IReadOnlyList<T> data,
        int totalCount,
        int pageNumber,
        int pageSize)
        : base(data, true, Error.None)
    {
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }

    public static PagedResponse<T> Create(
        IReadOnlyList<T> items,
        int totalCount,
        int pageNumber,
        int pageSize)
    {
        return new PagedResponse<T>(items, totalCount, pageNumber, pageSize);
    }
}