using System;
using System.Collections.Generic;
using System.Text;

namespace CPReservationApi.Model
{
    public class TableStatusGroupModel
    {
        public int group_id { get; set; }
        public string group_name { get; set; }
        public List<TableStatusGroupItem> group_items { get; set; }
    }

    public class TableStatusGroupItem
    {
        public int id { get; set; }
        public string color { get; set; }
        //public int table_status_group_id { get; set; }
        public string name { get; set; }
        //public int SortOrder { get; set; }
    }

    public class CreateTableStatusGroupRequest
    {
        public int id { get; set; }
        public int member_id { get; set; }
        public string name { get; set; }
        public int user_id { get; set; } = 0;
        public List<UpdateTableStatusGroupItem> table_status_group_item { get; set; }
    }

    public class UpdateTableStatusGroupItem
    {
        public int Id { get; set; }
        public int TableStatusGroupId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }

    public class UpdateTableStatusGroupItemRequest
    {
        public List<UpdateTableStatusGroupItem> table_status_group_item { get; set; }
    }

    public class CreateTableStatusGroupResponseModel
    {
        public int id { get; set; }
    }

    public class BlockTablesRequest
    {
        public List<int> table_ids { get; set; }
        public int member_id { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public bool force { get; set; }
    }

    public class DeleteBlockTablesRequest
    {
        public List<int> ids { get; set; }
    }

    public class BlockTables
    {
        public int table_id { get; set; }
        public List<BlockedTime> blocked { get; set; }
    }

    public class BlockedTime
    {
        public int id { get; set; }
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
    }

    public class TableStatusDetails
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public int FloorplanId { get; set; } = 0;
        public int InventoryMode { get; set; }
        public int LocationId { get; set; } = 0;
        public int TableId { get; set; } 
    }

    public class TableDetails
    {
        public int table_id { get; set; }
        public AssignedServer assigned_server { get; set; }
        public List<Schedule> schedule { get; set; }
    }

    public class AssignedServer
    {
        public int user_id { get; set; }
        public int current_guest_count { get; set; }
        public int total_guest_count { get; set; }
    }

    public class Schedule
    {
        public DateTime start_date { get; set; }
        public DateTime end_date { get; set; }
        public int status { get; set; }
        public int transaction_id { get; set; }
        public int transaction_type { get; set; }
        public int server_id { get; set; }
    }
}
