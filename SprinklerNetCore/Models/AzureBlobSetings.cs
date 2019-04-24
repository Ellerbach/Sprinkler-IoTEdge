using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SprinklerNetCore.Models
{
    public class AzureBlobSetings
    {
        public AzureBlobSetings(string storageAccount,
                                       string storageKey,
                                       string containerName)
        {
            if (string.IsNullOrEmpty(storageAccount))
                throw new ArgumentNullException("StorageAccount");

            if (string.IsNullOrEmpty(storageKey))
                throw new ArgumentNullException("StorageKey");

            if (string.IsNullOrEmpty(containerName))
                throw new ArgumentNullException("ContainerName");

            AccountName = storageAccount;
            AccountKey = storageKey;
            ContainerName = containerName;
        }

        public AzureBlobSetings() { }

        public string AccountName { get; }
        public string AccountKey { get; }
        public string ContainerName { get; }
    }
}
