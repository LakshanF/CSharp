using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReflectIt
{
    public interface ILogger { }

    public class SQLServiceLogger : ILogger { }

    public interface IRepository<T> { }

    public class SQLRepositry<T> : IRepository<T>
    {
        public SQLRepositry(ILogger logger)
        {

        }
    }

    public class Customer { }

    public class InvoiceService
    {
        public InvoiceService(IRepository<Customer> repository, ILogger logger)
        {

        }
    }
}