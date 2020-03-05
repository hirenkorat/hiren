using System.Collections.Generic;
using System.Linq;

namespace manpower
{
    internal class TaskPlanner : ITaskPlanner
    {
        private IRepository repository;
        List<Person> personList = new List<Person>();
        List<Assignment> assignmentList = new List<Assignment>();
        int day = 0;
        public TaskPlanner(IRepository repository)
        {
            this.repository = repository;
        }
        public Assignment GetAssignment(Task task)
        {
            Assignment assignment = new Assignment();

            var person = new Person();
            var skilledPerson = personList.Where(e => e.Skills.Contains(task.SkillRequired)).OrderBy(e => e.Skills.Count).ToArray();
            for (int i = 0; i < skilledPerson.Length; i++)
            {
                var objPerson = skilledPerson[i];
                if (assignmentList.Any(e => e.Person.Id == objPerson.Id && e.Day == day))
                {
                    if (i == (skilledPerson.Length - 1))
                    {
                        day++;
                        return this.GetAssignment(task);
                    }
                    continue;
                }
                person = objPerson;
                break;
            }
            assignment.Day = day;
            assignment.Task = task;
            assignment.Person = person;
            return assignment;
        }
        public IEnumerable<Assignment> Execute()
        {
            personList = repository.People.ToList();

            List<Task> taskList = new List<Task>();
            taskList = repository.Tasks.ToList();
            var priorityTask = taskList.Where(e => e.IsPriority).ToArray();
            var normalTask = taskList.Where(e => !e.IsPriority).ToArray();
            for (int i = 0; i < priorityTask.Length; i++)
            {
                var task = priorityTask[i];
                day = 1;
                var assignment = GetAssignment(task);
                assignmentList.Add(assignment);
            }
            for (int i = 0; i < normalTask.Length; i++)
            {
                var task = normalTask[i];
                day = 1;
                var assignment = GetAssignment(task);
                assignmentList.Add(assignment);
            }
            return assignmentList.OrderBy(e => e.Day);
        }
    }
}