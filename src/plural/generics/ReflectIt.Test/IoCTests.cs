using System;
using Xunit;

namespace ReflectIt.Test
{
    public class IoCTests
    {
        [Fact]
        public void Can_Resolve_Types()
        {
            var ioc = new Container();
            ioc.For<ILogger>().Use<SQLServiceLogger>();

            var logger = ioc.Resolve<ILogger>();
            Assert.Equal(typeof(SQLServiceLogger), logger.GetType());
        }

        [Fact]
        public void Can_Resolve_Types_Wihtout_Default_Ctor()
        {
            var ioc = new Container();
            ioc.For<ILogger>().Use<SQLServiceLogger>();
            ioc.For<IRepository<Employee>>().Use<SQLRepositry<Employee>>();

            var repository = ioc.Resolve<IRepository<Employee>>();
            Assert.Equal(typeof(SQLRepositry<Employee>), repository.GetType());
        }

        [Fact]
        public void Can_Resolve_Concrete_Type()
        {
            var ioc = new Container();
            ioc.For<ILogger>().Use<SQLServiceLogger>();
            //ioc.For<IRepository<Employee>>().Use<SQLRepositry<Employee>>();
            ioc.For(typeof(IRepository<>)).Use(typeof(SQLRepositry<>));

            var service = ioc.Resolve<InvoiceService>();
            Assert.NotNull(service);

        }

    }
}
