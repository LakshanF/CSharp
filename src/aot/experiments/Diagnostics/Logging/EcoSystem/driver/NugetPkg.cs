using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcoSystemDriver
{
    internal class PkgManager
    {
        string _fileName;
        public PkgManager(string fileName) 
        {
            _fileName = fileName;
        }
        public IEnumerator<NugetPkg> GetEnumerator()
        {
            foreach (string line in File.ReadLines(_fileName).Skip(1))
            {
                // PkgHash,ContainerPath,PkgHash1,Id,Version,max_majVersion2
                // 2e4da0ea248b472f96edd59763415425,lib/net6.0/KudoCode.Infrastructure.CodeGenerator.dll,2e4da0ea248b472f96edd59763415425,KudoCode.Infrastructure.CodeGenerator,1.1.0.8,1000000010000000000000008

                NugetPkg pkg = new NugetPkg();

                var values = line.Split(',');
                pkg.PkgHash = values[0];
                pkg.ContainerPath = values[1];
                pkg.Id = values[3];
                pkg.Version = values[4];
                yield return pkg;
            }
        }

    }
    internal class NugetPkg
    {
        public string? PkgHash { get; set; }
        public string? ContainerPath { get; set; }
        public string? Id { get; set; }
        public string? Version { get; set; }
    }
}
