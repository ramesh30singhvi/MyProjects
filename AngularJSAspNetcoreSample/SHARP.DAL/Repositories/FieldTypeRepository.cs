using SHARP.DAL.Models;
using SHARP.DAL.Repositories.Interfaces;

namespace SHARP.DAL.Repositories
{
    public class FieldTypeRepository : GenericRepository<FieldType>, IFieldTypeRepository
    {
        public FieldTypeRepository(SHARPContext context) : base(context)
        {
        }
    }
}