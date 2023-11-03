using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoSystemDriver
{
    internal class NugetPkg2
    {
        public string? PkgHash { get; set; }
        public string? Id { get; set; }
        public string? ContainerPath { get; set; }
        public string? Version { get; set; }
        public long? DownloadCount { get; set; }

        public NugetPkg2(string line)
        {
            var values = line.Split(',');
            PkgHash = values[0];
            Id = values[1];
            ContainerPath = values[2];
            Version = values[3];
            DownloadCount = values[4] == "" ? null : (long?)long.Parse(values[4]);
        }
    }
}
