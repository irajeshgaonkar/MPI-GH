using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.Request
{
    public class ServiceAccount
    {
        /// <summary>
        /// Primary key for service accounts
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Source system Name
        /// </summary>
        public string SourceSystemName { get; set; }

        /// <summary>
        /// AppId
        /// </summary>
        public string AppId { get; set; }
    }
}
