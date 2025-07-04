using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class TableColumnGroupRepository : GenericRepository<TableColumnGroup>, ITableColumnGroupRepository
    {
        public TableColumnGroupRepository(SHARPContext context) : base(context)
        {
        }
    }
}
