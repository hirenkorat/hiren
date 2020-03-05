using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;

internal class Repository : RepositoryBase
{
    public Repository(FileLocations fileLocations, string configPath) : base(fileLocations, configPath)
    {
    }

    public override void SaveAssignments(IEnumerable<Assignment> assignments, string saveToPath)
    {
        var path = GetPath(saveToPath);
        //if (!Directory.Exists(GetPath("data/out")))
        //    Directory.CreateDirectory(GetPath("data/out"));
        //if (!File.Exists(path))
        //    File.Create(path).Close();

        using (var writer = new StreamWriter(path))
        using (var csvWriter = new CsvWriter(writer))
        {
            csvWriter.WriteHeader<AssignmentOutput>();
            csvWriter.NextRecord();
            List<AssignmentOutput> list = assignments.Select(e => new AssignmentOutput { TaskId = e.Task.Id, PersonId = e.Person.Id, Day = e.Day }).ToList();
            csvWriter.WriteRecords(list);
            writer.Flush();
        }
    }
}
