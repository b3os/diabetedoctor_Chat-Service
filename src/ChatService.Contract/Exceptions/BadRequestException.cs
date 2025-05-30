﻿namespace ChatService.Contract.Exceptions;
public abstract class BadRequestException : DomainException
{
    protected BadRequestException(string message, string? errorCode = null)
        : base("Bad Request", message, errorCode)
    {
    }
}
