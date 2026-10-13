using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.Json;
using Computator.NET.Charting;
using Computator.NET.Core.Compilation;
using Computator.NET.Core.Evaluation;
using Computator.NET.Core.Natives;
using Computator.NET.WebApi.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Computator.NET.WebApi.Execution
{
    public static class WorkerRuntime
    {
        public static int Run()
        {
            // Redirected Windows consoles otherwise inherit a machine code page.
            // Match the parent's explicit UTF-8 pipes before opening text readers.
            Console.InputEncoding = new UTF8Encoding(false, true);
            Console.OutputEncoding = new UTF8Encoding(false, true);
            // Keep the protocol writer private. Ordinary Console.Write in trusted
            // scripts must not corrupt the response stream or retain server logs.
            var protocol = Console.Out;
            Console.SetOut(TextWriter.Null);
            try
            {
                var source = new StringBuilder(); var buffer = new char[4096]; int count;
                while ((count = Console.In.Read(buffer, 0, buffer.Length)) != 0)
                {
                    if (source.Length + count > 262144) throw new ArgumentException("Worker request is too large.");
                    source.Append(buffer, 0, count);
                }
                var message = JsonSerializer.Deserialize<WorkerRequest>(source.ToString(), WorkerProcess.JsonOptions);
                if (message?.Request == null || !Validator.TryValidateObject(message.Request, new ValidationContext(message.Request), new List<ValidationResult>(), true))
                    throw new ArgumentException("Invalid worker request.");
                GSLInitializer.Initialize();
                var controller = new ExecutionV2Controller(new ModeDeterminer(new TslCompiler()), new ChartFactory());
                // Calling the shared controller implementation directly deliberately
                // bypasses MVC's worker dispatch filter, not its validation service.
                var result = message.Chart ? controller.Chart(message.Request) : controller.Execute(message.Request);
                WorkerReply reply;
                if (result is FileContentResult image)
                    reply = new WorkerReply { Status = 200, Image = Convert.ToBase64String(image.FileContents) };
                else if (result is ObjectResult value)
                    reply = new WorkerReply { Status = value.StatusCode ?? 200, Json = JsonSerializer.Serialize(value.Value, WorkerProcess.JsonOptions) };
                else throw new InvalidOperationException("Unsupported worker result.");
                protocol.Write(JsonSerializer.Serialize(reply, WorkerProcess.JsonOptions));
                protocol.Flush();
                return 0;
            }
            catch (Exception exception)
            {
                protocol.Write(JsonSerializer.Serialize(new WorkerReply { Status = 400,
                    Json = JsonSerializer.Serialize(new { title = "Execution failed.", detail = exception.Message, status = 400 }, WorkerProcess.JsonOptions) }, WorkerProcess.JsonOptions));
                protocol.Flush(); return 0;
            }
        }
    }
}
