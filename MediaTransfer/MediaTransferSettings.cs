using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Json;

namespace KohdAndArt.MediaTransfer
{
    [DataContract]
    public sealed class MediaTransferSettings
    {
        #region Constants
        public const string FILENAME = @"MediaTransferSettings.json";
        #endregion

        #region Properties
        [DataMember]
        public string SourceFolder { get; set; }
        [DataMember]
        public string DestinationFolder { get; set; }
        [DataMember]
        public bool RemoveSourceFileAfterCopy { get; set; }
        [DataMember]
        public bool CreateEditsFolder { get; set; }
        [DataMember]
        public bool CreateFinalFolder { get; set; }
        [DataMember]
        public string DestinationFolderName { get; set; }
        #endregion

        public void Load()
        {
            if (File.Exists(FILENAME))
            {
                using (var stream = new FileStream(FILENAME, FileMode.Open))
                {
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MediaTransferSettings));
                    MediaTransferSettings mts = (MediaTransferSettings)ser.ReadObject(stream);
                    SetProperties(mts);
                }
            }
        }

        private void SetProperties(MediaTransferSettings mts)
        {
            SourceFolder = mts.SourceFolder;
            DestinationFolder = mts.DestinationFolder;
            RemoveSourceFileAfterCopy = mts.RemoveSourceFileAfterCopy;
            CreateEditsFolder = mts.CreateEditsFolder;
            CreateFinalFolder = mts.CreateFinalFolder;
            DestinationFolderName = mts.DestinationFolderName;
        }

        public void Save()
        {
            using (var stream = new FileStream(FILENAME, FileMode.Create))
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(MediaTransferSettings));
                ser.WriteObject(stream, this);
            }
        }
    }
}
