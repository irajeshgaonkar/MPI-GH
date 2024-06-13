namespace HCA.Models.Request
{
    public class CustomDataMapping
    {
        /// <summary>
        /// Primary key, id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// source_system_name
        /// </summary>
        public string SourceSystemName { get; set; }

        /// <summary>
        /// InputIndex
        /// </summary>
        public int InputIndex { get; set; }
        /// <summary>
        /// input_column_name
        /// </summary>
        public string InputColumnName { get; set; }

        /// <summary>
        /// verato_request_path
        /// </summary>
        public string VeratoRequestPath { get; set; }

        /// <summary>
        /// verato_response_path
        /// </summary>
        public string VeratoResponsePath { get; set; }

        /// <summary>
        /// api_response_path
        /// </summary>
        public string APIResponsePath { get; set; }


        /// <summary>
        /// OutputIndex
        /// </summary>
        public int OutputIndex { get; set; }


        /// <summary>
        /// output_column_name
        /// </summary>
        public string OutputColumnName { get; set; }
    }
}
