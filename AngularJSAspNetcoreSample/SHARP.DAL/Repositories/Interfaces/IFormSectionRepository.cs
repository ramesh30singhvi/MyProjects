using System;
using System.Threading.Tasks;
using SHARP.DAL.Models;

namespace SHARP.DAL.Repositories.Interfaces
{
	public interface IFormSectionRepository : IRepository<FormSection>
	{
		Task<FormSection[]> GetSectionsAsync(params int[] formVersionIds);
	}
}

