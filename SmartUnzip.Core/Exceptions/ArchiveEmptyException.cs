using System;

namespace SmartUnzip.Core.Exceptions;

public class ArchiveEmptyException(string message) : Exception(message);