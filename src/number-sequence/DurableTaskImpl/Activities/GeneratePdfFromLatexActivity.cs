using Azure.Storage.Blobs;
using DurableTask.Core;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using number_sequence.DataAccess;
using number_sequence.Models;
using number_sequence.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace number_sequence.DurableTaskImpl.Activities
{
    public sealed class GeneratePdfFromLatexActivity : AsyncTaskActivity<string, string>
    {
        private readonly NsStorage nsStorage;
        private readonly IServiceProvider serviceProvider;
        private readonly Sentinals sentinals;
        private readonly ILogger<GeneratePdfFromLatexActivity> logger;
        private readonly TelemetryClient telemetryClient;

        public GeneratePdfFromLatexActivity(
            NsStorage nsStorage,
            IServiceProvider serviceProvider,
            Sentinals sentinals,
            ILogger<GeneratePdfFromLatexActivity> logger,
            TelemetryClient telemetryClient)
        {
            this.nsStorage = nsStorage;
            this.serviceProvider = serviceProvider;
            this.sentinals = sentinals;
            this.logger = logger;
            this.telemetryClient = telemetryClient;
        }

        protected override async Task<string> ExecuteAsync(TaskContext context, string input)
        {
            // Basic setup
            using IOperationHolder<RequestTelemetry> op = this.telemetryClient.StartOperation<RequestTelemetry>(
                this.GetType().FullName,
                operationId: context.OrchestrationInstance.ExecutionId,
                parentOperationId: context.OrchestrationInstance.InstanceId);
            using CancellationTokenSource cts = new(TimeSpan.FromMinutes(5));
            CancellationToken cancellationToken = cts.Token;
            await this.sentinals.DBMigration.WaitForCompletionAsync(cancellationToken);

            using IServiceScope scope = this.serviceProvider.CreateScope();
            using NsContext nsContext = scope.ServiceProvider.GetRequiredService<NsContext>();

            LatexDocument latexDocument = await nsContext.LatexDocuments
                .FirstOrDefaultAsync(x => x.Id == input && x.ProcessedAt == default, cancellationToken);
            if (latexDocument == default)
            {
                throw new InvalidOperationException("Work was requested, but the document was not found.");
            }


            // Ensure the directories exist
            DirectoryInfo workingDir = new(Path.Combine(Path.GetTempPath(), latexDocument.Id));
            this.logger.LogInformation($"Working in {workingDir.FullName}");
            if (!workingDir.Exists)
            {
                workingDir.Create();
            }
            else
            {
                this.logger.LogWarning($"Working dir already existed. Deleting it.");
                workingDir.Delete(recursive: true);
            }


            // Download the input
            await foreach (BlobClient blob in this.nsStorage.EnumerateAllBlobsForLatexJobAsync(latexDocument.Id, cancellationToken))
            {
                if (!blob.Name.StartsWith($"{latexDocument.Id}/{NsStorage.C.LBP.Input}/"))
                {
                    this.logger.LogInformation($"Skipping {blob.Name} for input download.");
                    continue;
                }

                var blobFileInfo = new FileInfo(Path.Combine(workingDir.FullName, blob.Name.Substring($"{latexDocument.Id}/{NsStorage.C.LBP.Input}/".Length)));
                this.logger.LogInformation($"Downloading {blob.Name} to {blobFileInfo.FullName}");
                if (!blobFileInfo.Directory.Exists)
                {
                    blobFileInfo.Directory.Create();
                    this.logger.LogInformation($"Created {blobFileInfo.Directory.FullName}");
                }

                _ = await blob.DownloadToAsync(blobFileInfo.FullName, cancellationToken);
            }
            this.logger.LogInformation("Download complete.");
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            DateTimeOffset downloadCompleteTime = DateTimeOffset.UtcNow;


            // Set up the log files
            FileInfo stderr = new(Path.Combine(workingDir.FullName, "std.err"));
            using StreamWriter stderrStream = new(stderr.OpenWrite());
            FileInfo stdout = new(Path.Combine(workingDir.FullName, "std.out"));
            using StreamWriter stdoutStream = new(stdout.OpenWrite());


            // Run pdflatex twice
            ProcessStartInfo processStartInfo = new()
            {
                FileName = "/usr/bin/pdflatex",
                Arguments = $"-interaction=nonstopmode {Path.Combine(workingDir.FullName, $"{latexDocument.Id}.tex")}",
                WorkingDirectory = workingDir.FullName,

                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            // First execution
            using Process firstPassProcess = new()
            {
                StartInfo = processStartInfo
            };
            firstPassProcess.ErrorDataReceived += (_, data) => { if (data.Data != null) { this.logger.LogError(data.Data); } };
            firstPassProcess.ErrorDataReceived += (_, data) => { if (data.Data != null) { stderrStream.WriteLine($"{DateTimeOffset.UtcNow:u}: {data.Data.Replace("\n", $"\n{DateTimeOffset.UtcNow:u}: ")}"); } };
            firstPassProcess.OutputDataReceived += (_, data) => { if (data.Data != null) { this.logger.LogInformation(data.Data); } };
            firstPassProcess.OutputDataReceived += (_, data) => { if (data.Data != null) { stdoutStream.WriteLine($"{DateTimeOffset.UtcNow:u}: {data.Data.Replace("\n", $"\n{DateTimeOffset.UtcNow:u}: ")}"); } };
            _ = firstPassProcess.Start();
            firstPassProcess.BeginErrorReadLine();
            firstPassProcess.BeginOutputReadLine();
            await firstPassProcess.WaitForExitAsync(cancellationToken);

            // Handle first execution failure
            if (firstPassProcess.ExitCode != 0)
            {
                string msg = $"pdflatex ended with exit code {firstPassProcess.ExitCode}";
                this.logger.LogError(msg);
                stderrStream.WriteLine(msg);

                // Save
                stderrStream.Close();
                stdoutStream.Close();
                await this.UploadOutputDirAsync(latexDocument, workingDir, downloadCompleteTime, cancellationToken);
                latexDocument.ProcessedAt = DateTimeOffset.UtcNow;
                latexDocument.Successful = false;
                _ = await nsContext.SaveChangesAsync(cancellationToken);
                return default;
            }
            else
            {
                stderrStream.WriteLine();
                stderrStream.WriteLine("BEGINNING SECOND EXECUTION");
                stderrStream.WriteLine();
                stdoutStream.WriteLine();
                stdoutStream.WriteLine("BEGINNING SECOND EXECUTION");
                stdoutStream.WriteLine();
            }

            // Second execution
            using Process secondPassProcess = new()
            {
                StartInfo = processStartInfo
            };
            secondPassProcess.ErrorDataReceived += (_, data) => { if (data.Data != null) { this.logger.LogError(data.Data); } };
            secondPassProcess.ErrorDataReceived += (_, data) => { if (data.Data != null) { stderrStream.WriteLine($"{DateTimeOffset.UtcNow:u}: {data.Data.Replace("\n", $"\n{DateTimeOffset.UtcNow:u}: ")}"); } };
            secondPassProcess.OutputDataReceived += (_, data) => { if (data.Data != null) { this.logger.LogInformation(data.Data); } };
            secondPassProcess.OutputDataReceived += (_, data) => { if (data.Data != null) { stdoutStream.WriteLine($"{DateTimeOffset.UtcNow:u}: {data.Data.Replace("\n", $"\n{DateTimeOffset.UtcNow:u}: ")}"); } };
            _ = secondPassProcess.Start();
            secondPassProcess.BeginErrorReadLine();
            secondPassProcess.BeginOutputReadLine();
            await secondPassProcess.WaitForExitAsync(cancellationToken);

            // Handle second execution failure
            if (secondPassProcess.ExitCode != 0)
            {
                string msg = $"pdflatex ended with exit code {secondPassProcess.ExitCode}";
                this.logger.LogError(msg);
                stderrStream.WriteLine(msg);
                latexDocument.Successful = false;
            }
            else
            {
                latexDocument.Successful = true;
            }


            // Save
            stderrStream.Close();
            stdoutStream.Close();
            await this.UploadOutputDirAsync(latexDocument, workingDir, downloadCompleteTime, cancellationToken);
            latexDocument.ProcessedAt = DateTimeOffset.UtcNow;
            _ = await nsContext.SaveChangesAsync(cancellationToken);
            return default;
        }

        private async Task UploadOutputDirAsync(LatexDocument latexDocument, DirectoryInfo workingDir, DateTimeOffset downloadCompleteTime, CancellationToken cancellationToken)
        {
            foreach (FileInfo fileInfo in workingDir.EnumerateFiles("*", SearchOption.AllDirectories))
            {
                if (fileInfo.LastWriteTimeUtc < downloadCompleteTime)
                {
                    this.logger.LogInformation($"Not uploading {fileInfo.FullName} as last write time was {fileInfo.LastWriteTimeUtc:u} and download complete time was {downloadCompleteTime:u} ({downloadCompleteTime - fileInfo.LastWriteTimeUtc} difference)");
                    continue;
                }

                string targetPath = Path.Combine(NsStorage.C.LBP.Output, fileInfo.FullName.Substring(workingDir.FullName.Length + 1));
                this.logger.LogInformation($"Uploading {fileInfo.FullName} to {latexDocument.Id}/{targetPath}");
                BlobClient blobClient = this.nsStorage.GetBlobClientForLatexJob(latexDocument.Id, targetPath);
                _ = await blobClient.UploadAsync(fileInfo.FullName, overwrite: true, cancellationToken);
            }
            this.logger.LogInformation("Upload complete.");
        }
    }
}
