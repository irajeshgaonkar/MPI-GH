using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Core.Processors.File;

public interface IOutputFileWriter
{
    Task WriteFile(string requestId);
}
