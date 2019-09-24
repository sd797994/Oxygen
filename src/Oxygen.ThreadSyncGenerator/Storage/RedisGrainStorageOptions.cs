using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace Oxygen.ThreadSyncGenerator.Storage
{
    public class RedisGrainStorageOptions
    {
        /// <summary>
        /// Connection string forRedis storage.
        /// </summary>
        [Redact]
        public string ConnectionString { get; set; }

        /// <summary>
        /// Stage of silo lifecycle where storage should be initialized.  Storage must be initialized prior to use.
        /// </summary>
        public int InitStage { get; set; } = DEFAULT_INIT_STAGE;
        /// <summary>
        /// Default init stage in silo lifecycle.
        /// </summary>
        public const int DEFAULT_INIT_STAGE = ServiceLifecycleStage.ApplicationServices;

        /// <summary>
        /// The default ADO.NET invariant used for storage if none is given. 
        /// </summary>
        public const string DEFAULT_Redis_INVARIANT = "db01";
        /// <summary>
        /// The invariant name for storage.
        /// </summary>
        public string Invariant { get; set; } = DEFAULT_Redis_INVARIANT;

        /// <summary>
        /// Whether storage string payload should be formatted in JSON.
        /// <remarks>If neither <see cref="UseJsonFormat"/> nor <see cref="UseXmlFormat"/> is set to true, then BinaryFormatSerializer will be configured to format storage string payload.</remarks>
        /// </summary>
        public bool UseJsonFormat { get; set; }
        public bool UseFullAssemblyNames { get; set; }
        public bool IndentJson { get; set; }

        /// <summary>
        /// Whether storage string payload should be formatted in Xml.
        /// <remarks>If neither <see cref="UseJsonFormat"/> nor <see cref="UseXmlFormat"/> is set to true, then BinaryFormatSerializer will be configured to format storage string payload.</remarks>
        /// </summary>
        public bool UseXmlFormat { get; set; }
    }
}
