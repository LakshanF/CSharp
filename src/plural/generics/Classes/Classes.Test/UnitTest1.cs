using System;
using Xunit;

namespace Classes.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Buffer_Default()
        {
            Buffer<double> buffer = new Buffer<double>();
            buffer.Write(2.0);
            
            Assert.Equal(2.0, buffer.Read());
        }
    }
}
