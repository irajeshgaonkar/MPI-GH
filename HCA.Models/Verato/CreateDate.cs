namespace HCA.Models.Verato
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
