namespace MaerskApi.Library
{
    public static class StandardLogger

    {
        public enum LoggingEnum
        {
            Start = -1,
            End = 0,
            Ok = 1,
            NoContent = 2,
            ParamNotvalid = 4,
            NotFound = 5,
            NoCalc = 6,
            DataInconsistency = 7,
            ServerError = 8
        }
        const string mes = "ID: {0}, Controller: {1}, Action: {2}, Status: {3}, Parameters: {4}, Logged at: {5} UTC.";
        /* User to be added for production: */
        public static string LoggerLoader(Guid Id, string controller, string action, string actionparam, LoggingEnum e, DateTimeOffset dt)
        {
            string Message;
            string status = string.Empty;

            switch (e)
            {
                case LoggingEnum.Start:
                    status = "Start";
                    break;

                case LoggingEnum.End:
                    status = "End";
                    break;

                case LoggingEnum.Ok:
                    status = "OK";
                    break;

                case LoggingEnum.NoContent:
                    status = "OK. No content";
                    break;

                case LoggingEnum.NoCalc:
                    status = "Could not be calculated";
                    break;

                case LoggingEnum.ParamNotvalid:
                    status = "Invalid parameter";
                    break;

                case LoggingEnum.NotFound:
                    status = "Not found";
                    break;

                case LoggingEnum.DataInconsistency:
                    status = "Data inconsistency";
                    break;

                case LoggingEnum.ServerError:
                    status = "Internal Server Error";
                    break;
            }
            Message = string.Format(mes, Id, controller, action, status, actionparam, dt.ToString());
            return Message;
        }
    }
}
