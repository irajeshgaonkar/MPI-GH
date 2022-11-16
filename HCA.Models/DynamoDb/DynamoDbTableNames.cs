using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HCA.Models.DynamoDb
{
    public static class DynamoDbTableNames
    {
        public const string Notification = "mpi_notifications";
    }

    public static class DynamoDbNotificationColumnNames
    {
        public const string LinkId = "link_id";

        public const string TrackingId = "transaction_id";

        public const string TimeStamp = "time_stamp";

        public const string SourceSystemName = "source_system";

        public const string SourceSystemId = "source_system_id";

        public const string Operation = "operation";

        public const string Request = "request";

        public const string Response = "response";

        public const string PreviousLinkId = "previous_link_id";

        public const string Message = "message";


    }
}
