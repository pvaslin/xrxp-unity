using System;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public class BackupStorage : StorageBase
    {
        private StreamWriter _stream;
        private readonly string _directory;
        private string _path;

        public BackupStorage(string directory)
        {
            _directory = $"{directory}/backup";
        }

        protected override void OnOpen()
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            _path = $"{_directory}/Trace_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.backup.gz";
            _stream = new StreamWriter(
                new GZipStream(File.Create(_path), CompressionLevel.Fastest));
            _stream.AutoFlush = true;
            Debug.Log($"XRXP.Recorder [BackupStorage]: Writing to {_path}");
        }

        protected override void ProcessRecord(SerializedRecord record)
        {
            _stream.WriteLine(record.Json);
        }

        protected override void OnClose()
        {
            if (_stream != null)
            {
                try
                {
                    _stream.Flush();
                    _stream.Dispose();
                }
                catch (Exception e)
                {
                    Debug.LogError($"XRXP.Recorder [BackupStorage]: Error closing file: {e.Message}");
                }
                _stream = null;
            }
        }
    }
}
