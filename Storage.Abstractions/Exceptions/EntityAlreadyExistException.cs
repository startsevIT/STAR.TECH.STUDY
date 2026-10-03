namespace Storage.Abstractions.Exceptions;

public class EntityAlreadyExistException(string message) : Exception(message);