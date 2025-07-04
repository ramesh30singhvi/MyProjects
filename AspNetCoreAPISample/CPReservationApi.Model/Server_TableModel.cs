using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class Server_TableModel
    {
        public int id { get; set; }
        public string table_id { get; set; }
        public int status { get; set; }
    }

    public class ServerTablebySessionIdsModel
    {
        public int server_session_id { get; set; }
        public List<Server_TableModel> server_table { get; set; }
        public List<Server_SectionModel> server_section { get; set; }
    }
}
