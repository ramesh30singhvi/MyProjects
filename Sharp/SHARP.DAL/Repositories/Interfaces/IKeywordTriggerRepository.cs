using SHARP.Common.Filtration;
using SHARP.DAL.Models;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SHARP.DAL.Repositories.Interfaces
{
    public  interface IKeywordTriggerRepository :  IRepository<KeywordTrigger>
    {

        Task<KeywordTrigger[]> GetAsync(int keywordId,int formId);

    }
}
