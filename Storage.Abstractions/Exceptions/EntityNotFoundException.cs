namespace Storage.Abstractions.Exceptions;

public class EntityNotFoundException(string message) : Exception(message);