using System.Diagnostics;
using System.Text.Json;
using Agents.Opi.Backend.Api.Resources;
using Agents.Opi.Backend.Application.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Agents.Opi.Backend.Api.Middleware;

public sealed class GlobalExceptionMiddleware(ILogger<GlobalExceptionMiddleware> logger) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            logger.LogWarning(exception, "Validation error. TraceId={TraceId}", GetTraceId(context));
            var errors = exception.Errors
                .Select(error => new ApiError(
                    string.IsNullOrWhiteSpace(error.ErrorCode) ? "VALIDATION_ERROR" : error.ErrorCode,
                    string.IsNullOrWhiteSpace(error.ErrorMessage) ? ApiMessages.Validation : error.ErrorMessage,
                    string.IsNullOrWhiteSpace(error.PropertyName) ? null : error.PropertyName))
                .ToList();

            await WriteWrapped(context, StatusCodes.Status422UnprocessableEntity, ApiMessages.ValidationTitle, ApiMessages.Validation, errors);
        }
        catch (UnauthorizedAccessException exception)
        {
            logger.LogWarning(exception, "Forbidden error. TraceId={TraceId}", GetTraceId(context));
            await WriteWrapped(context, StatusCodes.Status403Forbidden, ApiMessages.ForbiddenTitle, ApiMessages.Forbidden);
        }
        catch (ArgumentException exception)
        {
            logger.LogWarning(exception, "Bad request. TraceId={TraceId}", GetTraceId(context));
            await WriteWrapped(context, StatusCodes.Status400BadRequest, ApiMessages.BadRequestTitle, exception.Message);
        }
        catch (KeyNotFoundException exception)
        {
            logger.LogWarning(exception, "Not found. TraceId={TraceId}", GetTraceId(context));
            await WriteWrapped(context, StatusCodes.Status404NotFound, ApiMessages.NotFoundTitle, ApiMessages.NotFound);
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Conflict. TraceId={TraceId}", GetTraceId(context));
            await WriteWrapped(context, StatusCodes.Status409Conflict, ApiMessages.ConflictTitle, ApiMessages.Conflict);
        }
        catch (GenerativeAiException exception)
        {
            logger.LogError(exception, "Generative AI error. TraceId={TraceId}", GetTraceId(context));
            await WriteWrapped(context, StatusCodes.Status502BadGateway, ApiMessages.UnexpectedTitle, ApiMessages.GenerationFailed);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled error. TraceId={TraceId}", GetTraceId(context));

            if (TryGetPostgresException(exception, out var postgresException) &&
                postgresException.SqlState is PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.ForeignKeyViolation)
            {
                await WriteWrapped(context, StatusCodes.Status409Conflict, ApiMessages.ConflictTitle, ApiMessages.Conflict);
                return;
            }

            await WriteWrapped(context, StatusCodes.Status500InternalServerError, ApiMessages.UnexpectedTitle, ApiMessages.Unexpected);
        }
    }

    private static string GetTraceId(HttpContext context)
        => Activity.Current?.Id ?? context.TraceIdentifier;

    private static async Task WriteWrapped(
        HttpContext context,
        int status,
        string title,
        string data,
        IReadOnlyList<ApiError>? errors = null)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var body = new ApiResponse(status, title, data, GetTraceId(context), errors ?? []);
        await context.Response.WriteAsync(JsonSerializer.Serialize(body));
    }

    private static bool TryGetPostgresException(Exception exception, out PostgresException postgresException)
    {
        postgresException = null!;

        if (exception is PostgresException direct)
        {
            postgresException = direct;
            return true;
        }

        if (exception is DbUpdateException { InnerException: PostgresException fromDbUpdate })
        {
            postgresException = fromDbUpdate;
            return true;
        }

        if (exception.InnerException is PostgresException inner)
        {
            postgresException = inner;
            return true;
        }

        return false;
    }
}

public sealed record ApiResponse(
    int Status,
    string Title,
    string Data,
    string TraceId,
    IReadOnlyList<ApiError> Errors);

public sealed record ApiError(
    string Code,
    string Message,
    string? Field);
