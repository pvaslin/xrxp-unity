using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;

namespace XRXP.Recorder.Storage
{
    public class BackupStorageSync : IDataStorage
    {
        private StreamWriter _stream;
        private readonly string _directory;
        private string _path;

        public BackupStorageSync(string directory)
        {
            _directory = $"{directory}/backup";
        }

        public void Open(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }
            _path = $"{_directory}/Trace_{DateTime.Now:yyyy-MM-dd_HH_mm_ss}.backup.gz";
            _stream = new StreamWriter(
                new GZipStream(File.Create(_path), System.IO.Compression.CompressionLevel.Fastest));
            _stream.AutoFlush = true;
            Debug.Log($"XRXP.Recorder [BackupStorageSync]: Writing to {_path}");
        }

        public void Add(SerializedRecord record)
        {
            if (_stream == null || !_stream.BaseStream.CanWrite)
            {
                throw new XRXPException($"XRXP.Recorder [BackupStorageSync]: File not accessible: {_path}");
            }
            _stream.WriteLine(record.Json);
            _stream.Flush();
        }

        public void CompleteAdding() { }

        public int RemainingDataCount()
        {
            return 0;
        }

        public void Dispose()
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
                    Debug.LogError($"XRXP.Recorder [BackupStorageSync]: Error closing file: {e.Message}");
                }
                _stream = null;
            }
        }
    }
}
