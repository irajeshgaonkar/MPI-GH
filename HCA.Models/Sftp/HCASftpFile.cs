using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Sftp;

public class HcaSftpFile
{
    public string FullName { get; set; }

    public string Name { get; set; }

    public DateTime LastModified { get; set; }
}
