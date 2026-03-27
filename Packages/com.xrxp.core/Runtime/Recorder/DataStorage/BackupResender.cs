using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public class BackupResender
    {
        private readonly string _backupDirectory;
        private readonly IDataStorage _recordTarget;
        private readonly IDataStorage _fileTarget;
        private readonly CancellationToken _cancellationToken;
        private Thread _thread;
        private volatile bool _isRunning;

        private static readonly Regex IdRegex = new Regex("\"Id\"\\s*:\\s*\"([^\"]+)\"", RegexOptions.Compiled);

        public BackupResender(string backupDirectory, IDataStorage recordTarget, IDataStorage fileTarget, CancellationToken cancellationToken)
        {
            _backupDirectory = backupDirectory;
            _recordTarget = recordTarget;
            _fileTarget = fileTarget;
            _cancellationToken = cancellationToken;
        }

        public bool IsRunning => _isRunning;

        public int GetPendingFileCount()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                return 0;
            }
            return Directory.GetFiles(_backupDirectory, "*.backup.gz", SearchOption.TopDirectoryOnly).Length;
        }

        public string[] GetPendingFiles()
        {
            if (!Directory.Exists(_backupDirectory))
            {
                return Array.Empty<string>();
            }
            var files = Directory.GetFiles(_backupDirectory, "*.backup.gz", SearchOption.TopDirectoryOnly);
            Array.Sort(files); // oldest first (filename contains timestamp)
            return files;
        }

        public void ResendAll()
        {
            if (_isRunning)
            {
                Debug.LogWarning("XRXP.Recorder [BackupResender]: Resend already in progress.");
                return;
            }

            var pendingFiles = GetPendingFiles();
            if (pendingFiles.Length == 0)
            {
                Debug.Log("XRXP.Recorder [BackupResender]: No backup files to resend.");
                return;
            }

            _thread = new Thread(() => ResendThread(pendingFiles))
            {
                IsBackground = true,
                Name = "BackupResender"
            };
            _thread.Start();
        }

        private void ResendThread(string[] files)
        {
            _isRunning = true;
            try
            {
                Debug.Log($"XRXP.Recorder [BackupResender]: Starting resend of {files.Length} backup file(s)...");

                for (int i = 0; i < files.Length; i++)
                {
                    if (_cancellationToken.IsCancellationRequested)
                    {
                        Debug.LogWarning("XRXP.Recorder [BackupResender]: Resend cancelled.");
                        return;
                    }

                    string file = files[i];
                    Debug.Log($"XRXP.Recorder [BackupResender]: Resending file {i + 1}/{files.Length}: {Path.GetFileName(file)}");

                    if (ResendRecordFile(file))
                    {
                        try
                        {
                            File.Move(file, file + ".sent");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"XRXP.Recorder [BackupResender]: Failed to rename {file}: {e.Message}");
                        }
                    }
                }

                // Resend media files from backup/sessions/ subfolders
                ResendMediaFiles();

                Debug.Log("XRXP.Recorder [BackupResender]: Resend complete.");
            }
            catch (Exception e)
            {
                Debug.LogError($"XRXP.Recorder [BackupResender]: Resend failed: {e.Message}");
            }
            finally
            {
                _isRunning = false;
            }
        }

        private bool ResendRecordFile(string filePath)
        {
            try
            {
                using (var fileStream = File.OpenRead(filePath))
                using (var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress))
                using (var reader = new StreamReader(gzipStream))
                {
                    string line;
                    int count = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (_cancellationToken.IsCancellationRequested) return false;
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string id = ExtractId(line) ?? $"resend_{count}";
                        var record = new SerializedRecord(id, line);
                        _recordTarget.Add(record);
                        count++;
                    }
                    Debug.Log($"XRXP.Recorder [BackupResender]: Resent {count} records from {Path.GetFileName(filePath)}");
                }
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"XRXP.Recorder [BackupResender]: Error reading {filePath}: {e.Message}");
                return false;
            }
        }

        private void ResendMediaFiles()
        {
            if (_fileTarget == null) return;

            string sessionsDir = Path.Combine(_backupDirectory, "sessions");
            if (!Directory.Exists(sessionsDir)) return;

            var mediaFiles = Directory.GetFiles(sessionsDir, "*", SearchOption.AllDirectories);
            if (mediaFiles.Length == 0) return;

            Debug.Log($"XRXP.Recorder [BackupResender]: Resending {mediaFiles.Length} media file(s)...");

            foreach (var mediaFile in mediaFiles)
            {
                if (_cancellationToken.IsCancellationRequested) return;

                string fileName = Path.GetFileNameWithoutExtension(mediaFile);
                var record = new SerializedRecord(fileName, "", mediaFile);
                _fileTarget.Add(record);
            }
        }

        private static string ExtractId(string json)
        {
            var match = IdRegex.Match(json);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
