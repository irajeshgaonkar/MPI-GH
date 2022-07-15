namespace HCA.Data
{
    public class DataConstants
    {
        public static Statuses Statuses { get; set; } = new Statuses();

        public static OperationTypes OperationTypes { get; set; } = new OperationTypes();
    }

    public class Statuses
    {
        public string NotStarted = "Not Started";

        public string Pending = "Pending";

        public string Processing = "Processing";

        public string Failed = "Failed";

        public string ParsingFailed = "Parsing Failed";

        public string DataLoaded = "Data Loaded";

        public string ParsingFile = "Parsing File";

        public string Succeded = "Succeded";
    }

    public class OperationTypes
    {
        public string Post = "VE Post";
        public string Link = "VE Link";
        public string UnLink = "VE Un Link";
        public string Merge = "VE Merge";
        public string UnMerge = "VE Un Merge";
    }
}

