<Query Kind="Program">
  <NuGetReference>Mono.Cecil</NuGetReference>
  <Namespace>Mono.Cecil.Cil</Namespace>
  <Namespace>Mono.Cecil</Namespace>
</Query>


static string dir;
static string fileName;

void Main()
{
	dir = Path.Combine(new FileInfo(Util.CurrentQuery.FilePath).DirectoryName, "InstrumentTest1");
	fileName = dir + @"\bin\Debug\net6.0\InstrumentTest1.dll";
	
	DotnetBuild();
	EditDll();
	CopyDll();
	DotnetRun();
}

void DotnetBuild()
{
	var p = new Process();
	p.StartInfo.FileName = $"dotnet";
	p.StartInfo.Arguments = "build";
	p.StartInfo.WorkingDirectory = dir;
	p.Start();
	p.WaitForExit();
}

void DotnetRun()
{
	var p = new Process();
	p.StartInfo.FileName = new FileInfo(fileName).FullName.Replace(".dll", ".exe").Dump();
	//p.StartInfo.CreateNoWindow = true;
	//p.StartInfo.WorkingDirectory = new FileInfo(fileName).DirectoryName;
	//p.StartInfo.RedirectStandardOutput = true;
	p.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
	{
		if (!String.IsNullOrEmpty(e.Data))
		{
			e.Data.Dump();
		}
	});
	p.Start();
	p.WaitForExit();
}

void EditDll()
{
	using var define = Mono.Cecil.AssemblyDefinition.ReadAssembly(fileName);
	foreach (var t in define.MainModule.Types)
	{
		foreach (var m in t.Methods)
		{
			//if (m.Name == "<Main>$")
			//{
				AddMethod(m);	
			//}
			
			foreach (var i in m.Body.Instructions)
			{
				if (i.OpCode == OpCodes.Ldstr)
				{
					//i.Operand = "Hello instruction";
				}
			}
		}
	}
	define.Write(fileName.Replace("InstrumentTest1.dll", "InstrumentTest1.edited.dll"));
}

void CopyDll()
{
	//System.IO.File.Copy(fileName, fileName.Replace("InstrumentTest1.dll", "InstrumentTest1.orignal.dll"), true);
	System.IO.File.Copy(fileName.Replace("InstrumentTest1.dll", "InstrumentTest1.edited.dll"), fileName, true);
}

void AddMethod(MethodDefinition m)
{
	var newMd = m.Module.ImportReference(
					typeof(Console).GetMethod("WriteLine", new[] { typeof(string) }));

	var processor = m.Body.GetILProcessor();

	{
		var newInstruction0 = processor.Create(OpCodes.Ldstr, "start from instrction");
		var newInstruction1 = processor.Create(OpCodes.Call, newMd);
		{
			var firstInstruction = m.Body.Instructions[0];
			processor.InsertBefore(firstInstruction, newInstruction1);
		}
		{
			var firstInstruction = m.Body.Instructions[0];
			processor.InsertBefore(firstInstruction, newInstruction0);
		}
	}
	{
		{
			var newInstruction0 = processor.Create(OpCodes.Ldstr, "end from instrction");
			var newInstruction1 = processor.Create(OpCodes.Call, newMd);
			{
				var lastInstruction = m.Body.Instructions[m.Body.Instructions.Count - 1];
				processor.InsertBefore(lastInstruction, newInstruction0);
			}
			{
				var lastInstruction = m.Body.Instructions[m.Body.Instructions.Count - 1];
				processor.InsertBefore(lastInstruction, newInstruction1);
			}
		
		}
	}
}
