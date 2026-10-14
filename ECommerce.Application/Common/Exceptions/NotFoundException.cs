using System;
using System.Collections.Generic;
using System.Text;

namespace ECommerce.Application.Common.Exceptions
{
    public class NotFoundException:Exception
    {
        public NotFoundException(string entity, object key)
        : base($"{entity} with id '{key}' was not found.") { }

    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
}
