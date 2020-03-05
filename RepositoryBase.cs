using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CsvHelper;


/// <summary>An implementation of IRepository that works with .csv files</summary>
internal abstract class RepositoryBase : IRepository
{
    protected RepositoryBase(FileLocations fileLocations, string configPath)
    {
        FileLocations = fileLocations ?? throw new ArgumentNullException(nameof(fileLocations));
        ConfigPath = configPath ?? throw new ArgumentNullException(nameof(configPath));
    }

    protected FileLocations FileLocations { get; }
    protected string ConfigPath { get; }
    public IEnumerable<Skill> Skills { get; private set; } = Enumerable.Empty<Skill>();
    public IEnumerable<Person> People { get; private set; } = Enumerable.Empty<Person>();
    public IEnumerable<Task> Tasks { get; private set; } = Enumerable.Empty<Task>();

    public abstract void SaveAssignments(IEnumerable<Assignment> assignments, string saveToPath);

    public virtual void LoadData(string taskListPath)
    {
        Skills = LoadSkills();
        var skillsIndex = Skills.ToDictionary(i => i.Id);

        People = LoadPeople(skillsIndex);
        Tasks = LoadTasks(taskListPath, skillsIndex);
    }

    private IEnumerable<Skill> LoadSkills()
    {
        using (var reader = new StreamReader(GetPath(FileLocations.Skills)))
        using (var csv = new CsvReader(reader))
        {
            return csv.GetRecords<Skill>().ToList();
        }
    }

    private IEnumerable<Person> LoadPeople(Dictionary<int, Skill> skillsIndex)
    {
        List<Person> people;
        Dictionary<int, Person> peopleIndex;

        using (var reader = new StreamReader(GetPath(FileLocations.People)))
        using (var csv = new CsvReader(reader))
        {
            people = csv.GetRecords<Person>().ToList();
            peopleIndex = people.ToDictionary(i => i.Id);
        }

        using (var reader = new StreamReader(GetPath(FileLocations.SkillMatrix)))
        using (var csv = new CsvReader(reader))
        {
            var skillMatrixTypeDefinition = new
            {
                PersonId = default(int),
                SkillId = default(int)
            };

            var skillMatrix = csv.GetRecords(skillMatrixTypeDefinition);

            foreach (var item in skillMatrix)
            {
                if (!peopleIndex.TryGetValue(item.PersonId, out var person))
                    throw new InvalidOperationException($"Invalid skills matrix - no person found with id {item.PersonId}");

                if (!skillsIndex.TryGetValue(item.SkillId, out var skill))
                    throw new InvalidOperationException($"Invalid skills matrix - no skill found with id {item.SkillId}");

                // add skill to person
                person.Skills.Add(skill);
            }
        }

        return people;
    }

    private IEnumerable<Task> LoadTasks(string datasetPath, Dictionary<int, Skill> skillsIndex)
    {
        using (var reader = new StreamReader(GetPath(datasetPath)))
        using (var csv = new CsvReader(reader))
        {
            var rawTaskDefinition = new
            {
                Id = default(int),
                SkillRequired = default(int),
                IsPriority = default(bool)
            };

            return csv
              .GetRecords(rawTaskDefinition)
              .Select(item =>
              {
                  if (!skillsIndex.TryGetValue(item.SkillRequired, out var skill))
                      throw new InvalidOperationException($"Invalid task list - no skill found with id {item.SkillRequired}");

                  return new Task { Id = item.Id, SkillRequired = skill, IsPriority = item.IsPriority };
              })
              .ToList();
        }
    }

    protected string GetPath(string fileName)
      => Path.Combine(ConfigPath, fileName);
}
