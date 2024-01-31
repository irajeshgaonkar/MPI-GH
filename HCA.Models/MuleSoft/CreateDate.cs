using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.MuleSoft
{
    /// <summary>
    /// 
    /// </summary>
    public class CreateDate
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="CDate"></param>
        public CreateDate(string CDate)
        {
            if (CDate != null)
            {
                createDate = CDate;
            }
            else
            {
                createDate = null;
            }
        }

        /// <summary>
        /// CreateDate
        /// </summary>
        public string createDate { get; set; } = null;


    }


   
}
