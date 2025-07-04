using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class StartServerSessionResponse : BaseResponse
    {
        public StartServerSessionResponse()
        {
            data = new SessionModel();
        }
        public SessionModel data { get; set; }
    }

    public class StartServerSessionRequest
    {
        public List<int> location_id { get; set; }
        public int user_id { get; set; }


    }

    public class StartServerSessionV2Request
    {
        public List<int> floor_plan_id { get; set; }
        public int user_id { get; set; }

        public DateTime? session_start_date { get; set; }


    }

    public class StartServerSessionsResponse : BaseResponse
    {
        public StartServerSessionsResponse()
        {
            data = new List<SessionModel>();
        }
        public List<SessionModel> data { get; set; }
    }

    public class StartServerSessionsRequest
    {
        public List<int> floor_plan_id { get; set; }
        public List<int> user_ids { get; set; }
        public string session_start_date { get; set; }
    }

    public class EndServerSessionRequest
    {
        public int server_session_id { get; set; }
    }


    public class EndServerSessionResponse : BaseResponse
    {
    }

    public class AssignServerTableRequest
    {
        public int server_session_id { get; set; }
        public List<string> table_id { get; set; }
    }

    public class AssignServerTableResponse : BaseResponse
    {
    }

    public class UnassignServerTableRequest
    {
        public int server_session_id { get; set; }
        public List<string> table_id { get; set; }
    }
    public class UnassignServerTableResponse : BaseResponse
    {
    }

    public class AssignServerSectionRequest
    {
        public int server_session_id { get; set; }
        public List<string> server_section { get; set; }
    }

    public class AssignServerSectionResponse : BaseResponse
    {
    }

    public class UnassignServerSectionRequest
    {
        public int server_session_id { get; set; }
        public List<string> server_section { get; set; }
    }
    public class UnassignServerSectionResponse : BaseResponse
    {
    }

    public class SessionResponse : BaseResponse
    {
        public SessionResponse()
        {
            data = new List<SessionModel>();
        }
        public List<SessionModel> data { get; set; }
    }

    public class Server_SectionResponse : BaseResponse
    {
        public Server_SectionResponse()
        {
            data = new List<Server_SectionModel>();
        }
        public List<Server_SectionModel> data { get; set; }
    }

    public class Server_TableResponse : BaseResponse
    {
        public Server_TableResponse()
        {
            data = new List<Server_TableModel>();
        }
        public List<Server_TableModel> data { get; set; }
    }

    public class ServerTablebySessionIdsResponse : BaseResponse
    {
        public ServerTablebySessionIdsResponse()
        {
            data = new List<ServerTablebySessionIdsModel>();
        }
        public List<ServerTablebySessionIdsModel> data { get; set; }
    }

    public class CheckEndServerSessionRequest
    {
        public int location_id { get; set; }
    }

    public class CheckEndServerSessionResponse : BaseResponse
    {
    }
    public class ClearSessionRequest
    {
        public int table_id { get; set; }
    }

    public class ClearSessionModel
    {
        public int id { get; set; }
    }
    public class ClearSessionResponse : BaseResponse
    {
        public ClearSessionResponse()
        {
            data = new ClearSessionModel();
        }
        public ClearSessionModel data { get; set; }
    }
}
