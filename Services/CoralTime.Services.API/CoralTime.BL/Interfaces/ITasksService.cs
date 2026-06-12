using CoralTime.ViewModels.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CoralTime.BL.Interfaces
{
    public interface ITasksService
    {
        IQueryable<TaskTypeView> Get();

        TaskTypeView GetById(int id);

        TaskTypeView Create(TaskTypeView timeEntryTypeData);

        TaskTypeView Update(TaskTypeView timeEntryTypeData);

        bool Delete(int id);
    }
}