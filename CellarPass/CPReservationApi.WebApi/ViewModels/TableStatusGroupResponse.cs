using CPReservationApi.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CPReservationApi.WebApi.ViewModels
{
    public class TableStatusGroupResponse : BaseResponse
    {
        public TableStatusGroupResponse()
        {
            data = new List<TableStatusGroupModel>();
        }
        public List<TableStatusGroupModel> data { get; set; }
    }

    public class TableStatusGroupItemResponse : BaseResponse
    {
        public TableStatusGroupItemResponse()
        {
            data = new List<TableStatusGroupItem>();
        }
        public List<TableStatusGroupItem> data { get; set; }
    }

    public class CreateTableStatusGroupResponse : BaseResponse
    {
        public CreateTableStatusGroupResponse()
        {
            data = new CreateTableStatusGroupResponseModel();
        }
        public CreateTableStatusGroupResponseModel data { get; set; }
    }

    public class BlockTablesResponse : BaseResponse
    {
        public BlockTablesResponse()
        {
            data = new BlockTableIds();
        }
        public BlockTableIds data { get; set; }
    }

    public class BlockTableIds
    {
        public List<int> ids { get; set; }
    }

    public class BlockTablesByMemberIdResponse : BaseResponse
    {
        public BlockTablesByMemberIdResponse()
        {
            data = new List<BlockTables>();
        }
        public List<BlockTables> data { get; set; }
    }

    public class TableDetailsResponse : BaseResponse
    {
        public TableDetailsResponse()
        {
            data = new TableDetails();
        }
        public TableDetails data { get; set; }
    }
}
