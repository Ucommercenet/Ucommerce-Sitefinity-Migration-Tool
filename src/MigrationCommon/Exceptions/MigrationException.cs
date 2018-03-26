using System;

namespace MigrationCommon.Exceptions
{
    public class MigrationException : Exception
    {
        public MigrationException()
        {
        }

        public MigrationException(string message)
            : base(message)
        {
        }

        public MigrationException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
