using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace taskmanager.DTOs
{
    public record ContStatusTaskDTO
    {
        public int TotalTasks { get; init; }
        public int TotalTasksToDo { get; init; }
        public int TotalTasksInProgress { get; init; }
        public int TotalTasksDone { get; init; }
        public int TotalTasksCanceled { get; init; }
    }
}